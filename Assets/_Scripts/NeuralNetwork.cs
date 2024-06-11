using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NeuralNetwork {
    public Neuron[] InputLayer;
    public Neuron[] HiddenLayer;
    public Neuron[] OutputLayer;
    public Synapse[] Synapses;
    public ActivationFunctionType ActivationFunctionType;

    public NeuralNetwork(
        ActivationFunctionType activationFunctionType,
        int inputLayerSize,
        int hiddenLayerSize,
        int outputLayerSize
    ) {
        ActivationFunctionType = activationFunctionType;
        InputLayer = CreateNeurons(inputLayerSize);
        HiddenLayer = hiddenLayerSize > 0 ? CreateNeurons(hiddenLayerSize) : null;
        OutputLayer = CreateNeurons(outputLayerSize);
        Synapses = CreateSynapses(inputLayerSize, hiddenLayerSize, outputLayerSize);
    }

    private Neuron[] CreateNeurons(int count) {
        Neuron[] neurons = new Neuron[count];
        for (int i = 0; i < count; i++) {
            neurons[i] = new Neuron();
        }
        return neurons;
    }

    private Synapse[] CreateSynapses(int inputCount, int hiddenCount, int outputCount) {
        Synapse[] synapses = hiddenCount > 0 ? new Synapse[inputCount * hiddenCount + hiddenCount * outputCount] : new Synapse[inputCount * outputCount];
        int index = 0;

        // Synapses entre a camada de entrada e a camada oculta, se houver
        if (hiddenCount > 0) {
            for (int i = 0; i < inputCount; i++) {
                for (int j = 0; j < hiddenCount; j++) {
                    synapses[index++] = new Synapse(i, j, UnityEngine.Random.Range(-1f, 1f));
                }
            }

            // Synapses entre a camada oculta e a camada de saída
            for (int i = 0; i < hiddenCount; i++) {
                for (int j = 0; j < outputCount; j++) {
                    synapses[index++] = new Synapse(i + inputCount, j + hiddenCount, UnityEngine.Random.Range(-1f, 1f));
                }
            }
        } else {
            // Synapses diretamente entre a camada de entrada e a camada de saída
            for (int i = 0; i < inputCount; i++) {
                for (int j = 0; j < outputCount; j++) {
                    synapses[index++] = new Synapse(i, j, UnityEngine.Random.Range(-1f, 1f));
                }
            }
        }

        return synapses;
    }

    public NeuralNetwork Crossover(NeuralNetwork partner) {
        NeuralNetwork offspring = new(
            ActivationFunctionType,
            InputLayer.Length,
            HiddenLayer != null ? HiddenLayer.Length : 0,
            OutputLayer.Length
        ) {
            // Perform crossover for synapses weights
            Synapses = new Synapse[Synapses.Length]
        };
        for (int i = 0; i < Synapses.Length; i++) {
            offspring.Synapses[i] = new Synapse {
                InputNeuronIndex = Synapses[i].InputNeuronIndex,
                OutputNeuronIndex = Synapses[i].OutputNeuronIndex,
                Weight = UnityEngine.Random.value < 0.5f ? Synapses[i].Weight : partner.Synapses[i].Weight
            };
        }

        // Perform crossover for neurons biases
        offspring.InputLayer = CrossoverNeurons(InputLayer, partner.InputLayer);
        offspring.HiddenLayer = HiddenLayer != null ? CrossoverNeurons(HiddenLayer, partner.HiddenLayer) : null;
        offspring.OutputLayer = CrossoverNeurons(OutputLayer, partner.OutputLayer);
        return offspring;
    }

    private Neuron[] CrossoverNeurons(Neuron[] layer, Neuron[] partnerLayer) {
        Neuron[] offspringLayer = new Neuron[layer.Length];
        for (int i = 0; i < layer.Length; i++) {
            float bias = UnityEngine.Random.value < 0.5f ? layer[i].Bias : partnerLayer[i].Bias;
            offspringLayer[i] = new Neuron { Bias = bias };
        }
        return offspringLayer;
    }

    public void Mutate(float mutationRate) {
        for (int i = 0; i < Synapses.Length; i++) {
            if (UnityEngine.Random.value < mutationRate) {
                Synapses[i].MutateWeight(UnityEngine.Random.Range(-1f, 1f));
            }
        }

        for (int i = 0; i < InputLayer.Length; i++) {
            if (UnityEngine.Random.value < mutationRate) {
                InputLayer[i].Bias += UnityEngine.Random.Range(-1f, 1f);
            }
        }

        for (int i = 0; i < HiddenLayer?.Length; i++) {
            if (UnityEngine.Random.value < mutationRate) {
                HiddenLayer[i].Bias += UnityEngine.Random.Range(-1f, 1f);
            }
        }

        for (int i = 0; i < OutputLayer.Length; i++) {
            if (UnityEngine.Random.value < mutationRate) {
                OutputLayer[i].Bias += UnityEngine.Random.Range(-1f, 1f);
            }
        }

        // Possibilidade de adicionar uma camada oculta durante a mutação
        if (HiddenLayer?.Length == 0 && UnityEngine.Random.value < mutationRate) {
            int hiddenLayerSize = UnityEngine.Random.Range(1, InputLayer.Length + OutputLayer.Length);
            HiddenLayer = CreateNeurons(hiddenLayerSize);
            Synapses = CreateSynapses(InputLayer.Length, hiddenLayerSize, OutputLayer.Length);
        }
    }

    public float[] FeedForward(float[] inputs) {
        if (inputs.Length != InputLayer.Length) {
            Debug.LogWarning($"Número de entradas não corresponde ao tamanho da camada de entrada. O número de entradas é de {inputs.Length} mas o número de neurônios é de {InputLayer.Length}");
        }

        // Configura as entradas na camada de entrada
        for (int i = 0; i < InputLayer.Length; i++) {
            InputLayer[i].Value = inputs[i];
        }

        // Extrair os pesos da estrutura Synapse para um array
        float[] weightsArray = new float[Synapses.Length];
        for (int i = 0; i < Synapses.Length; i++) {
            weightsArray[i] = Synapses[i].Weight;
        }

        // Converter os arrays de entrada e saída para NativeArrays
        NativeArray<float> nativeInputs = new(inputs, Allocator.TempJob);
        NativeArray<float> nativeOutputs = new(OutputLayer.Length, Allocator.TempJob);
        NativeArray<float> nativeWeights = new(weightsArray, Allocator.TempJob);

        // Configurar e executar o job
        var job = new FeedForwardJob {
            Inputs = nativeInputs,
            Weights = nativeWeights,
            Outputs = nativeOutputs
        };
        job.Schedule(OutputLayer.Length, 32).Complete();

        // Copiar os resultados de volta para o array de saída
        float[] outputValues = new float[OutputLayer.Length];
        nativeOutputs.CopyTo(outputValues);

        // Liberar a memória alocada para as NativeArrays
        nativeInputs.Dispose();
        nativeOutputs.Dispose();
        nativeWeights.Dispose();

        return outputValues;
    }
}

[BurstCompile]
public struct FeedForwardJob : IJobParallelFor {
    [ReadOnly] public NativeArray<float> Inputs;
    [ReadOnly] public NativeArray<float> Weights;
    public NativeArray<float> Outputs;

    public void Execute(int index) {
        float sum = 0f;
        for (int i = 0; i < Inputs.Length; i++) {
            sum += Inputs[i] * Weights[index * Inputs.Length + i];
        }
        Outputs[index] = sum;
    }
}

public struct Neuron {
    public float Bias;
    public float Value;

    public Neuron(float bias, float value) {
        Bias = bias;
        Value = value;
    }
}

public struct Synapse {
    public int InputNeuronIndex;
    public int OutputNeuronIndex;
    public float Weight;

    public Synapse(int inputNeuronIndex, int outputNeuronIndex, float weight) {
        InputNeuronIndex = inputNeuronIndex;
        OutputNeuronIndex = outputNeuronIndex;
        Weight = weight;
    }

    public void MutateWeight(float mutationAmount) {
        Weight += mutationAmount;
    }
}

public enum ActivationFunctionType { ReLU, Sigmoid, Tanh }