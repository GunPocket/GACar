using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using System;

public enum ActivationFunctionType { Sigmoid, ReLU, Tanh }

public enum RegularizationType { None, L1, L2 }

public enum OptimizationAlgorithm { SGD, Adam, RMSprop }

public struct Synapse {
    public int Id { get; set; }
    public Neuron InputNeuron { get; set; }
    public Neuron OutputNeuron { get; set; }
    public float Weight { get; set; }

    public Synapse(int id, Neuron inputNeuron, Neuron outputNeuron, float weight) {
        Id = id;
        InputNeuron = inputNeuron;
        OutputNeuron = outputNeuron;
        Weight = weight;
    }
}

public struct Neuron {
    public int Index { get; private set; }
    public ActivationFunctionType ActivationFunction { get; private set; }
    public List<Synapse> IncomingSynapses { get; private set; }
    public List<Synapse> OutgoingSynapses { get; private set; }

    public readonly int IncomingSynapseCount => IncomingSynapses.Count;
    public readonly int OutgoingSynapseCount => OutgoingSynapses.Count;

    public Neuron(int index, ActivationFunctionType activationFunction) {
        Index = index;
        ActivationFunction = activationFunction;
        IncomingSynapses = new List<Synapse>();
        OutgoingSynapses = new List<Synapse>();
    }

    public readonly void Dispose() {
        IncomingSynapses.Clear();
        OutgoingSynapses.Clear();
    }

    public readonly void AddOutgoingSynapse(Neuron outputNeuron, float weight) => OutgoingSynapses.Add(new Synapse(0, this, outputNeuron, weight));

    public readonly void AddIncomingSynapse(Neuron inputNeuron, float weight) => IncomingSynapses.Add(new Synapse(0, inputNeuron, this, weight));

    public readonly override bool Equals(object obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }

        Neuron other = (Neuron)obj;
        return Index == other.Index;
    }

    public readonly override int GetHashCode() => Index.GetHashCode();
}


public class NeuralLayer {
    public int NeuronCount { get; }
    public ActivationFunctionType ActivationFunction { get; }
    public RegularizationType Regularization { get; }
    public OptimizationAlgorithm Optimization { get; }
    public List<Neuron> Neurons = new();

    public NeuralLayer(int neuronCount, ActivationFunctionType activationFunction, RegularizationType regularization, OptimizationAlgorithm optimization) {
        NeuronCount = neuronCount;
        ActivationFunction = activationFunction;
        Regularization = regularization;
        Optimization = optimization;
    }

    public NeuralLayer Crossover(NeuralLayer otherLayer) {
        NeuralLayer childLayer = new(NeuronCount, ActivationFunction, Regularization, Optimization);
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron childNeuron = (i % 2 == 0) ? Neurons[i] : otherLayer.Neurons[i];
            childLayer.Neurons.Add(childNeuron);
        }
        return childLayer;
    }

    public void AddNeuron(Neuron neuron) => Neurons.Add(neuron);

}

public class NeuralNetworkSerializer : MonoBehaviour {
    public string SerializeNeuralNetwork(NeuralNetwork neuralNetwork) {
        string json = JsonUtility.ToJson(neuralNetwork);
        return json;
    }
}

public class NeuralNetworkDeserializer : MonoBehaviour {
    public NeuralNetwork DeserializeNeuralNetwork(string json) {
        NeuralNetwork neuralNetwork = JsonUtility.FromJson<NeuralNetwork>(json);
        return neuralNetwork;
    }
}

public struct TrainGPUJob : IJob {
    [ReadOnly] public NativeArray<float> input;
    [ReadOnly] public NativeArray<float> weights;
    [ReadOnly] public NativeArray<float> biases;
    public NativeArray<float> output;

    public void Execute() {
        int inputSize = input.Length;
        int outputSize = output.Length;

        for (int neuronIndex = 0; neuronIndex < outputSize; neuronIndex++) {
            float result = 0.0f;

            for (int i = 0; i < inputSize; i++) {
                result += input[i] * weights[neuronIndex * inputSize + i];
            }

            result += biases[neuronIndex];
            output[neuronIndex] = math.tanh(result);
        }
    }
}