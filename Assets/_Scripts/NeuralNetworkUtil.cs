using System;
using System.Collections.Generic;
using UnityEngine;

public enum ActivationFunctionType { Sigmoid, ReLU, Tanh }

public enum RegularizationType { None, L1, L2 }

public enum OptimizationAlgorithm { SGD, Adam, RMSprop }

public class Synapse {
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

public class Neuron {
    public int Index { get; set; }
    public ActivationFunctionType ActivationFunction { get; set; }
    public List<Synapse> IncomingSynapses { get; set; }
    public List<Synapse> OutgoingSynapses { get; set; }

    public Neuron(int index, ActivationFunctionType activationFunction) {
        Index = index;
        ActivationFunction = activationFunction;
        IncomingSynapses = new List<Synapse>();
        OutgoingSynapses = new List<Synapse>();
    }

    public float Activate(float input) {
        return ActivationFunction switch {
            ActivationFunctionType.Sigmoid => Sigmoid(input),
            ActivationFunctionType.ReLU => ReLU(input),
            ActivationFunctionType.Tanh => Tanh(input),
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

    public void AddOutgoingSynapse(Neuron outputNeuron, float weight) {
        Synapse synapse = new(0, this, outputNeuron, weight);
        OutgoingSynapses.Add(synapse);
    }

    public void AddIncomingSynapse(Neuron inputNeuron, float weight) {
        Synapse synapse = new(0, inputNeuron, this, weight);
        IncomingSynapses.Add(synapse);
    }
}

public class NeuralLayer {
    public int NeuronCount { get; }
    public ActivationFunctionType ActivationFunction { get; }
    public RegularizationType Regularization { get; }
    public OptimizationAlgorithm Optimization { get; }
    public List<Neuron> Neurons { get; set; }

    public NeuralLayer(int neuronCount, ActivationFunctionType activationFunction, RegularizationType regularization, OptimizationAlgorithm optimization) {
        NeuronCount = neuronCount;
        ActivationFunction = activationFunction;
        Regularization = regularization;
        Optimization = optimization;
        Neurons = new List<Neuron>();
    }

    public NeuralLayer Crossover(NeuralLayer otherLayer) {
        NeuralLayer childLayer = new(NeuronCount, ActivationFunction, Regularization, Optimization);
        for (int i = 0; i < Neurons.Count; i++) {
            Neuron childNeuron = (i % 2 == 0) ? Neurons[i] : otherLayer.Neurons[i];
            childLayer.Neurons.Add(childNeuron);
        }
        return childLayer;
    }

    public void AddNeuron(Neuron neuron) {
        Neurons.Add(neuron);
    }
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