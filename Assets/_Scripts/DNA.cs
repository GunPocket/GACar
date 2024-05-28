using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public class DNA {
    public NeuralNetwork NeuralNetwork;
    public float Fitness;

    public DNA(int[] layers) {
        NeuralNetwork = new NeuralNetwork(layers);
        Fitness = 0;
    }

    public DNA(NeuralNetwork neuralNetwork) {
        this.NeuralNetwork = neuralNetwork;
    }

    public DNA Crossover(DNA otherParent) {
        DNA offspring = new DNA(NeuralNetwork.Layers);
        for (int i = 0; i < offspring.NeuralNetwork.Weights.Length; i++) {
            for (int j = 0; j < offspring.NeuralNetwork.Weights[i].Length; j++) {
                for (int k = 0; k < offspring.NeuralNetwork.Weights[i][j].Length; k++) {
                    offspring.NeuralNetwork.Weights[i][j][k] = UnityEngine.Random.Range(0f, 1f) < 0.5f ? this.NeuralNetwork.Weights[i][j][k] : otherParent.NeuralNetwork.Weights[i][j][k];
                }
            }
        }
        return offspring;
    }

    public void Mutate(float mutationRate) {
        NeuralNetwork.Mutate(mutationRate);
    }

    public static DNA FromString(string dnaString) {
        string[] layerStrings = dnaString.Split(';');

        List<int[]> layers = new List<int[]>();

        foreach (string layerString in layerStrings) {
            if (string.IsNullOrEmpty(layerString)) {
                continue;
            }
            string[] neuronStrings = layerString.Split(',');

            int[] neurons = new int[neuronStrings.Length];

            for (int j = 0; j < neuronStrings.Length; j++) {
                if (!float.TryParse(neuronStrings[j].Replace(".", ","), out float neuronValue)) {
                    Debug.LogError("Erro ao converter string para DNA: Valor inválido encontrado.");
                    Debug.LogError("Valor Inválido: " + neuronStrings[j]);
                    return null;
                }
                neurons[j] = Mathf.RoundToInt(neuronValue * 1000);
            }
            layers.Add(neurons);
        }

        NeuralNetwork neuralNetwork = new NeuralNetwork(layers.Select(layer => layer.Length).ToArray());
        return new DNA(neuralNetwork);
    }




    public string ToReadableString(float[][][] weights) {
        StringBuilder sb = new StringBuilder();

        CultureInfo culture = new CultureInfo("en-US");
        for (int i = 0; i < weights.Length; i++) {
            float[][] layerWeights = weights[i];

            for (int j = 0; j < layerWeights.Length; j++) {
                float[] neuronWeights = layerWeights[j];

                for (int k = 0; k < neuronWeights.Length; k++) {
                    sb.Append(neuronWeights[k].ToString("0.###", culture));
                    if (k < neuronWeights.Length - 1) {
                        sb.Append(",");
                    }
                }
                sb.Append(";");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
