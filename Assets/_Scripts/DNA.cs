using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DNA {
    public NeuralNetwork NeuralNetwork;
    public float Fitness { get; set; }

    public DNA(List<NeuralLayer> layers, ActivationFunctionType activationFunctionType, RegularizationType regularizationType, float regularizationLambda, OptimizationAlgorithm optimizationAlgorithm) {
        NeuralNetwork = new NeuralNetwork(layers, activationFunctionType, regularizationType, regularizationLambda, optimizationAlgorithm);
        Fitness = 0;
    }

    public DNA(string neuralNetworkString) {
        NeuralNetwork = NeuralNetwork.FromJson(neuralNetworkString);
        Fitness = 0;
    }

    public DNA(NeuralNetwork neuralNetwork) {
        NeuralNetwork = neuralNetwork;
    }

    public DNA Crossover(DNA otherParent) {
        NeuralNetwork childNetwork = NeuralNetwork.Crossover(otherParent.NeuralNetwork);
        return new DNA(childNetwork);
    }

    public void Mutate(float mutationRate) {
        NeuralNetwork.Mutate(mutationRate);
    }

    public static DNA FromJson(string json) {
        return JsonUtility.FromJson<DNA>(json);
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}
