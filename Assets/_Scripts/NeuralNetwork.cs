using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;

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

    private Unity.Mathematics.Random rand;

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

        rand = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));

        Synapses = new List<Synapse>();

        InitializeWeightsAndBiases();
        InitializeNeurons(); 
        InitializeSynapses();
    }

    private void InitializeWeightsAndBiases() {
        Weights = new List<float[][]>();
        Biases = new List<float[]>();

        System.Random rand = new();

        for (int i = 1; i < Layers.Count; i++) {
            int previousLayerSize = Layers[i - 1].NeuronCount;
            int currentLayerSize = Layers[i].NeuronCount;

            float[][] layerWeights = new float[currentLayerSize][];
            for (int j = 0; j < currentLayerSize; j++) {
                layerWeights[j] = new float[previousLayerSize];
                for (int k = 0; k < previousLayerSize; k++) {
                    layerWeights[j][k] = (float)rand.NextDouble() * 2f - 1f;
                }
            }
            Weights.Add(layerWeights);

            float[] layerBiases = new float[currentLayerSize];
            for (int j = 0; j < currentLayerSize; j++) {
                layerBiases[j] = (float)rand.NextDouble() * 2f - 1f;
            }
            Biases.Add(layerBiases);
        }
    }

    private void InitializeNeurons() {
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++) {
            int layerSize = Layers[layerIndex].NeuronCount;
            List<Neuron> layerNeurons = new(layerSize);

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
                    Synapse synapse = new(synapseId++, Layers[i].Neurons[j], Layers[i + 1].Neurons[k], (float)rand.NextDouble() * 2f - 1f); // Usando o gerador de números aleatórios
                    Synapses.Add(synapse);
                    Layers[i].Neurons[j].OutgoingSynapses.Add(synapse);
                    Layers[i + 1].Neurons[k].IncomingSynapses.Add(synapse);
                }
            }
        }
    }



    public float[] TrainGPU(float[] input) {
        // Handle NaN or Infinity values in the input
        for (int i = 0; i < input.Length; i++) {
            if (float.IsNaN(input[i]) || float.IsInfinity(input[i])) {
                input[i] = 0.0f; // Set to a default value
            }
        }

        int outputSize = Layers[^1].NeuronCount;

        // Setup NativeArrays
        NativeArray<float> inputNative = new(input, Allocator.TempJob);
        NativeArray<float> weightsNative = new(FlattenWeights(), Allocator.TempJob);
        NativeArray<float> biasesNative = new(Biases[^1], Allocator.TempJob);
        NativeArray<float> outputNative = new(outputSize, Allocator.TempJob);

        // Create and schedule the job
        TrainGPUJob job = new(){
            input = inputNative,
            weights = weightsNative,
            biases = biasesNative,
            output = outputNative
        };
        job.Schedule().Complete();

        // Retrieve the results
        float[] output = outputNative.ToArray();

        // Dispose NativeArrays
        inputNative.Dispose();
        weightsNative.Dispose();
        biasesNative.Dispose();
        outputNative.Dispose();

        return output;
    }

    private NativeArray<float> FlattenWeights() {
        int totalWeightCount = CalculateTotalWeightCount();
        NativeArray<float> flatWeights = new(totalWeightCount, Allocator.Temp);

        int currentIndex = 0;
        foreach (var layerWeights in Weights) {
            foreach (var neuronWeights in layerWeights) {
                for (int i = 0; i < neuronWeights.Length; i++) {
                    flatWeights[currentIndex++] = neuronWeights[i];
                }
            }
        }

        return flatWeights;
    }

    private int CalculateTotalWeightCount() {
        int totalWeightCount = 0;
        foreach (var layerWeights in Weights) {
            foreach (var neuronWeights in layerWeights) {
                totalWeightCount += neuronWeights.Length;
            }
        }
        return totalWeightCount;
    }


    public NeuralNetwork Crossover(NeuralNetwork otherNetwork) {
        List<NeuralLayer> childLayers = new();

        for (int i = 0; i < Layers.Count; i++) {
            // Seleciona a camada de uma das redes parentais alternadamente
            NeuralLayer childLayer = (i % 2 == 0) ? Layers[i].Crossover(otherNetwork.Layers[i]) : otherNetwork.Layers[i].Crossover(Layers[i]);
            childLayers.Add(childLayer);
        }

        // Cria uma nova rede neural com as camadas combinadas
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
            int chooseMutation = UnityEngine.Random.Range(0, 5);
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
                RemoveDisconnectedElements();
                break;
        }
    }

    private void AddConnection() {
        int layerIndex = rand.NextInt(0, Layers.Count - 1);
        int targetLayerIndex = layerIndex + 1;

        if (Layers[layerIndex].Neurons.Count > 0 && Layers[targetLayerIndex].Neurons.Count > 0) {
            int neuronIndex = rand.NextInt(0, Layers[layerIndex].Neurons.Count);
            int targetNeuronIndex = rand.NextInt(0, Layers[targetLayerIndex].Neurons.Count);

            float weight = rand.NextFloat(-1f, 1f);
            Layers[layerIndex].Neurons[neuronIndex].AddOutgoingSynapse(Layers[targetLayerIndex].Neurons[targetNeuronIndex], weight);
        }
    }

    private void MutateConnection(Neuron neuron, float mutationAmount) {
        if (neuron.OutgoingSynapses.Count > 0) {
            int synapseIndex = rand.NextInt(0, neuron.OutgoingSynapses.Count);
            Synapse synapse = neuron.OutgoingSynapses[synapseIndex];
            synapse.Weight += rand.NextFloat(-mutationAmount, mutationAmount);
            neuron.OutgoingSynapses[synapseIndex] = synapse;
        }
    }


    private void ChangeConnectionWeight() {
        int layerIndex = UnityEngine.Random.Range(0, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);
        MutateConnection(Layers[layerIndex].Neurons[neuronIndex], 0.5f);
    }

    private void DisableConnection() {
        int layerIndex = UnityEngine.Random.Range(0, Layers.Count - 1);
        int neuronIndex = UnityEngine.Random.Range(0, Layers[layerIndex].Neurons.Count);
        Neuron neuron = Layers[layerIndex].Neurons[neuronIndex];
        MutateConnection(neuron, Mathf.Infinity);
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

    private void RemoveDisconnectedElements() {
        for (int layerIndex = 1; layerIndex < Layers.Count; layerIndex++) {
            int connectedNeuronsCount = 0;
            List<Neuron> neuronsToRemove = new();

            foreach (Neuron neuron in Layers[layerIndex].Neurons) {
                bool isConnected = false;
                foreach (Synapse synapse in neuron.IncomingSynapses) {
                    if (Layers[layerIndex - 1].Neurons.Contains(synapse.InputNeuron)) {
                        isConnected = true;
                        break;
                    }
                }

                if (!isConnected) {
                    neuronsToRemove.Add(neuron);
                } else {
                    connectedNeuronsCount++;
                }
            }

            foreach (Neuron neuronToRemove in neuronsToRemove) {
                Layers[layerIndex].Neurons.Remove(neuronToRemove);
            }
        }
    }

    public static NeuralNetwork FromJson(string json) {
        return JsonUtility.FromJson<NeuralNetwork>(json);
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}
