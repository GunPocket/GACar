using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {
    public ComputeShader NeuralNetworkCompute;

    public List<NeuralLayer> Layers { get; private set; }
    public List<float[][]> Weights { get; private set; }
    public List<float[]> Biases { get; private set; }

    public ActivationFunctionType ActivationFunctionType { get; private set; }
    public RegularizationType RegularizationType { get; private set; }
    public OptimizationAlgorithm OptimizationAlgorithm { get; private set; }

    public float RegularizationRate { get; private set; }
    public float LearningRate { get; private set; }
    public float DropoutRate { get; private set; }
    public int Epochs { get; private set; }

    public List<Synapse> Synapses { get; private set; }
    private int synapseId = 0;

    private ComputeBuffer inputBuffer;
    private ComputeBuffer weightBuffer;
    private ComputeBuffer biasBuffer;
    private ComputeBuffer outputBuffer;

    public NeuralNetwork(List<NeuralLayer> layers, ActivationFunctionType activationFunctionType,
                         RegularizationType regularizationType = RegularizationType.None,
                         float regularizationRate = 0.01f,
                         OptimizationAlgorithm optimizationAlgorithm = OptimizationAlgorithm.SGD,
                         float learningRate = 0.01f,
                         int epochs = 100,
                         float dropoutRate = 0.1f) {
        Layers = layers;
        ActivationFunctionType = activationFunctionType;
        RegularizationType = regularizationType;
        RegularizationRate = regularizationRate;
        OptimizationAlgorithm = optimizationAlgorithm;
        LearningRate = learningRate;
        Epochs = epochs;
        DropoutRate = dropoutRate;

        Synapses = new List<Synapse>();
        InitializeWeightsAndBiases();
        InitializeNeurons();
        InitializeSynapses();
    }

    private void InitializeWeightsAndBiases() {
        Weights = new List<float[][]>();
        Biases = new List<float[]>();

        for (int i = 1; i < Layers.Count; i++) {
            int previousLayerSize = Layers[i - 1].NeuronCount;
            int currentLayerSize = Layers[i].NeuronCount;

            float[][] layerWeights = new float[currentLayerSize][];
            for (int j = 0; j < currentLayerSize; j++) {
                layerWeights[j] = new float[previousLayerSize];
                for (int k = 0; k < previousLayerSize; k++) {
                    layerWeights[j][k] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
            Weights.Add(layerWeights);

            float[] layerBiases = new float[currentLayerSize];
            for (int j = 0; j < currentLayerSize; j++) {
                layerBiases[j] = UnityEngine.Random.Range(-1f, 1f);
            }
            Biases.Add(layerBiases);
        }
    }

    private void InitializeNeurons() {
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++) {
            int layerSize = Layers[layerIndex].NeuronCount;
            List<Neuron> layerNeurons = new();

            for (int neuronIndex = 0; neuronIndex < layerSize; neuronIndex++) {
                Neuron neuron = new(neuronIndex, ActivationFunctionType);
                layerNeurons.Add(neuron);
            }

            Layers[layerIndex].Neurons = layerNeurons;
        }
    }

    private void InitializeSynapses() {
        synapseId = 0;
        for (int i = 0; i < Layers.Count - 1; i++) {
            int currentLayerSize = Layers[i].NeuronCount;
            int nextLayerSize = Layers[i + 1].NeuronCount;

            for (int j = 0; j < currentLayerSize; j++) {
                for (int k = 0; k < nextLayerSize; k++) {
                    Synapse synapse = new(synapseId++, Layers[i].Neurons[j], Layers[i + 1].Neurons[k], UnityEngine.Random.Range(-1f, 1f));
                    Synapses.Add(synapse);
                    Layers[i].Neurons[j].OutgoingSynapses.Add(synapse);
                    Layers[i + 1].Neurons[k].IncomingSynapses.Add(synapse);
                }
            }
        }
    }

    public float[] TrainGPU(float[] input) {
        for (int i = 0; i < input.Length; i++) {
            if (float.IsNaN(input[i]) || float.IsInfinity(input[i])) {
                // Handle NaN or Infinity (e.g., set to default value)
                input[i] = 0.0f;
            }
        }

        // Setup buffers
        int inputSize = input.Length;
        int outputSize = Layers[Layers.Count - 1].NeuronCount;

        inputBuffer = new ComputeBuffer(inputSize, sizeof(float));
        weightBuffer = new ComputeBuffer(inputSize * outputSize, sizeof(float));
        biasBuffer = new ComputeBuffer(outputSize, sizeof(float));
        outputBuffer = new ComputeBuffer(outputSize, sizeof(float));

        // Set data to buffers
        inputBuffer.SetData(input);
        weightBuffer.SetData(FlattenWeights());
        biasBuffer.SetData(Biases[Biases.Count - 1]);

        // Set buffers and other parameters to the compute shader
        int kernelHandle = NeuralNetworkCompute.FindKernel("CSMain");
        NeuralNetworkCompute.SetBuffer(kernelHandle, "inputBuffer", inputBuffer);
        NeuralNetworkCompute.SetBuffer(kernelHandle, "weightBuffer", weightBuffer);
        NeuralNetworkCompute.SetBuffer(kernelHandle, "biasBuffer", biasBuffer);
        NeuralNetworkCompute.SetBuffer(kernelHandle, "outputBuffer", outputBuffer);
        NeuralNetworkCompute.SetInt("inputSize", inputSize);
        NeuralNetworkCompute.SetInt("outputSize", outputSize);

        // Execute the compute shader
        NeuralNetworkCompute.Dispatch(kernelHandle, outputSize, 1, 1);

        // Retrieve the results
        float[] output = new float[outputSize];
        outputBuffer.GetData(output);

        // Clean up
        inputBuffer.Release();
        weightBuffer.Release();
        biasBuffer.Release();
        outputBuffer.Release();

        return output;
    }

    private float[] FlattenWeights() {
        List<float> flatWeights = new List<float>();
        foreach (var layerWeights in Weights) {
            foreach (var neuronWeights in layerWeights) {
                flatWeights.AddRange(neuronWeights);
            }
        }
        return flatWeights.ToArray();
    }

    public float[] Train(float[] input) {
        for (int epoch = 0; epoch < Epochs; epoch++) {
            float[] output = FeedForward(input);
            Backpropagate(input);
        }
        return FeedForward(input);
    }

    private float[] FeedForward(float[] input) {
        float[] currentActivation = input;

        for (int layerIndex = 0; layerIndex < Layers.Count - 1; layerIndex++) {
            int nextLayerSize = Layers[layerIndex + 1].NeuronCount;
            float[] nextActivation = new float[nextLayerSize];

            for (int nextNeuronIndex = 0; nextNeuronIndex < nextLayerSize; nextNeuronIndex++) {
                float weightedSum = 0;

                for (int currentNeuronIndex = 0; currentNeuronIndex < currentActivation.Length; currentNeuronIndex++) {
                    Neuron currentNeuron = Layers[layerIndex].Neurons[currentNeuronIndex];
                    Synapse synapse = currentNeuron.OutgoingSynapses[nextNeuronIndex];
                    weightedSum += currentNeuron.Activate(currentActivation[currentNeuronIndex]) * synapse.Weight;
                }

                weightedSum += Biases[layerIndex][nextNeuronIndex];
                nextActivation[nextNeuronIndex] = Activate(weightedSum, ActivationFunctionType);
            }

            currentActivation = nextActivation;
        }

        return currentActivation;
    }

    private void Backpropagate(float[] input) {
        float[][] delta = new float[Layers.Count][];

        // Inicializando o delta para todas as camadas
        for (int i = 0; i < Layers.Count; i++) {
            delta[i] = new float[Layers[i].NeuronCount];
        }

        // Calculando o erro da camada de saída
        for (int i = 0; i < Layers[Layers.Count - 1].NeuronCount; i++) {
            float error = 0; // Removendo a referência ao 'output'
            delta[Layers.Count - 1][i] = error * Derivative(Layers[Layers.Count - 1].Neurons[i].Activate(0)); // Usando a ativação da última camada diretamente
        }

        // Propagando o erro de volta pelas camadas ocultas
        for (int layerIndex = Layers.Count - 2; layerIndex >= 0; layerIndex--) {
            for (int i = 0; i < Layers[layerIndex].NeuronCount; i++) {
                float error = 0;
                for (int j = 0; j < Layers[layerIndex + 1].NeuronCount; j++) {
                    error += delta[layerIndex + 1][j] * Layers[layerIndex].Neurons[i].OutgoingSynapses[j].Weight;
                }
                delta[layerIndex][i] = error * Derivative(Layers[layerIndex].Neurons[i].Activate(0));
            }
        }

        // Atualizando os pesos e os vieses
        for (int layerIndex = 0; layerIndex < Layers.Count - 1; layerIndex++) {
            for (int i = 0; i < Layers[layerIndex + 1].NeuronCount; i++) {
                for (int j = 0; j < Layers[layerIndex].NeuronCount; j++) {
                    Synapse synapse = Layers[layerIndex].Neurons[j].OutgoingSynapses[i];
                    float weightChange = -LearningRate * delta[layerIndex + 1][i] * Layers[layerIndex].Neurons[j].Activate(0);
                    synapse.Weight += weightChange;
                }
                Biases[layerIndex][i] += -LearningRate * delta[layerIndex + 1][i];
            }
        }
    }


    private float Activate(float value, ActivationFunctionType activationFunction) {
        return activationFunction switch {
            ActivationFunctionType.Sigmoid => Sigmoid(value),
            ActivationFunctionType.ReLU => ReLU(value),
            ActivationFunctionType.Tanh => Tanh(value),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private float Derivative(float value) {
        return ActivationFunctionType switch {
            ActivationFunctionType.Sigmoid => value * (1 - value),
            ActivationFunctionType.ReLU => value > 0 ? 1 : 0,
            ActivationFunctionType.Tanh => 1 - value * value,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private float Sigmoid(float x) {
        return 1f / (1f + Mathf.Exp(-x));
    }

    private float ReLU(float x) {
        return Mathf.Max(0, x);
    }

    private float Tanh(float x) {
        return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
    }

    public NeuralNetwork Crossover(NeuralNetwork otherNetwork) {
        List<NeuralLayer> childLayers = new();

        for (int i = 0; i < Layers.Count; i++) {
            NeuralLayer childLayer;
            if (i % 2 == 0) {
                childLayer = Layers[i].Crossover(otherNetwork.Layers[i]);
            } else {
                childLayer = otherNetwork.Layers[i].Crossover(Layers[i]);
            }
            childLayers.Add(childLayer);
        }

        NeuralNetwork childNetwork = new(
            childLayers,
            ActivationFunctionType,
            RegularizationType,
            RegularizationRate,
            OptimizationAlgorithm,
            LearningRate,
            Epochs,
            DropoutRate
        );

        return childNetwork;
    }

    public void Mutate(float mutationRate) {
        if (UnityEngine.Random.Range(0f, 1f) < mutationRate) {
            int chooseMutation = UnityEngine.Random.Range(0, 6);
            ApplyMutation(chooseMutation);
        }
    }

    private void ApplyMutation(int mutationType) {
        switch (mutationType) {
            case 0:
                AddConnection();
                break;
            case 1:
                ChangeConnectionWeight();
                break;
            case 2:
                DisableConnection();
                break;
            case 3:
                EvolveToAddNeuron();
                break;
            case 4:
                ChangeActivationFunction();
                break;
            case 5:
                RemoveDisconnectedElements();
                break;
        }
    }

    private void AddConnection() {
        int layerIndex = UnityEngine.Random.Range(0, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);
        int targetNeuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex + 1].Neurons.Count);

        float weight = UnityEngine.Random.Range(-1f, 1f);
        Layers[layerIndex].Neurons[neuronIndex].AddOutgoingSynapse(Layers[layerIndex + 1].Neurons[targetNeuronIndex], weight);
    }

    private void ChangeConnectionWeight() {
        int layerIndex = UnityEngine.Random.Range(0, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);
        int synapseIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons[neuronIndex].OutgoingSynapses.Count);

        Layers[layerIndex].Neurons[neuronIndex].OutgoingSynapses[synapseIndex].Weight += UnityEngine.Random.Range(-0.5f, 0.5f);
    }

    private void DisableConnection() {
        int layerIndex = UnityEngine.Random.Range(0, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);
        int synapseIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons[neuronIndex].OutgoingSynapses.Count);

        Layers[layerIndex].Neurons[neuronIndex].OutgoingSynapses[synapseIndex].Weight = 0f;
    }

    private void EvolveToAddNeuron() {
        int layerIndex = UnityEngine.Random.Range(1, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);

        Neuron newNeuron = new(neuronIndex, ActivationFunctionType);
        foreach (Neuron previousNeuron in Layers[layerIndex - 1].Neurons) {
            float weight = UnityEngine.Random.Range(-1f, 1f);
            newNeuron.AddIncomingSynapse(previousNeuron, weight);
        }

        Layers[layerIndex].AddNeuron(newNeuron);
    }

    private void ChangeActivationFunction() {
        int layerIndex = UnityEngine.Random.Range(1, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);

        ActivationFunctionType newActivationFunction = (ActivationFunctionType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ActivationFunctionType)).Length);
        Layers[layerIndex].Neurons[neuronIndex].ActivationFunction = newActivationFunction;
    }

    private void RemoveDisconnectedElements() {
        for (int layerIndex = 1; layerIndex < Layers.Count; layerIndex++) {
            List<Neuron> connectedNeurons = new();

            foreach (Neuron neuron in Layers[layerIndex].Neurons) {
                bool isConnected = false;
                foreach (Synapse synapse in neuron.IncomingSynapses) {
                    if (Layers[layerIndex - 1].Neurons.Contains(synapse.InputNeuron)) {
                        isConnected = true;
                        break;
                    }
                }
                if (isConnected) {
                    connectedNeurons.Add(neuron);
                }
            }

            Layers[layerIndex].Neurons = connectedNeurons;
        }
    }

    public void AddLayer(NeuralLayer layer) {
        Layers.Add(layer);
    }

    public static NeuralNetwork FromJson(string json) {
        return JsonUtility.FromJson<NeuralNetwork>(json);
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}
