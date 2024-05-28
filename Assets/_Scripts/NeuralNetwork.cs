using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork {
    public int[] Layers;
    public float[][] Neurons;
    public float[][][] Weights;
    public float[][] LayerActivations;

    public NeuralNetwork(int[] layers) {
        this.Layers = layers;
        InitializeNeurons();
        InitializeWeights();
        InitializeLayerActivations();
    }

    private void InitializeNeurons() {
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < Layers.Length; i++) {
            neuronsList.Add(new float[Layers[i]]);
        }

        Neurons = neuronsList.ToArray();
    }

    private void InitializeWeights() {
        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < Layers.Length; i++) {
            List<float[]> layerWeightsList = new List<float[]>();

            int neuronsInPreviousLayer = Layers[i - 1];

            for (int j = 0; j < Layers[i]; j++) {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++) {
                    neuronWeights[k] = UnityEngine.Random.Range(-1f, 1f);
                }

                layerWeightsList.Add(neuronWeights);
            }

            weightsList.Add(layerWeightsList.ToArray());
        }

        Weights = weightsList.ToArray();
    }

    private void InitializeLayerActivations() {
        List<float[]> activationsList = new List<float[]>();

        for (int i = 0; i < Layers.Length; i++) {
            activationsList.Add(new float[Layers[i]]);
        }

        LayerActivations = activationsList.ToArray();
    }

    public float[] FeedForward(float[] inputs) {
        for (int i = 0; i < inputs.Length; i++) {
            Neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < Layers.Length; i++) {
            for (int j = 0; j < Neurons[i].Length; j++) {
                float value = 0f;

                for (int k = 0; k < Neurons[i - 1].Length; k++) {
                    value += Weights[i - 1][j][k] * Neurons[i - 1][k];
                }

                Neurons[i][j] = (float)Math.Tanh(value);
                LayerActivations[i][j] = Neurons[i][j]; // Definindo a ativação da camada
            }
        }

        return Neurons[Neurons.Length - 1];
    }

    public void CopyWeightsFrom(NeuralNetwork sourceNetwork) {
        if (sourceNetwork.Layers.Length != Layers.Length) {
            Debug.LogError("As redes neurais têm diferentes números de camadas.");
            return;
        }

        for (int i = 0; i < Layers.Length; i++) {
            if (sourceNetwork.Layers[i] != Layers[i]) {
                Debug.LogError("As redes neurais têm diferentes números de neurônios em uma ou mais camadas.");
                return;
            }

            for (int j = 0; j < Weights[i].Length; j++) {
                Array.Copy(sourceNetwork.Weights[i][j], Weights[i][j], sourceNetwork.Weights[i][j].Length);
            }
        }
    }

    public void Mutate(float mutationRate) {
        for (int i = 0; i < Weights.Length; i++) {
            for (int j = 0; j < Weights[i].Length; j++) {
                for (int k = 0; k < Weights[i][j].Length; k++) {
                    if (UnityEngine.Random.Range(0f, 1f) <= mutationRate) {
                        Weights[i][j][k] = UnityEngine.Random.Range(-1f, 1f);
                    }
                }
            }
        }

        for (int i = 1; i < Layers.Length - 1; i++) {
            if (UnityEngine.Random.Range(0f, 1f) <= mutationRate) {
                if (UnityEngine.Random.Range(0f, 1f) <= 0.5f && Layers[i] > 1) {
                    Array.Resize(ref Weights[i - 1], Weights[i - 1].Length - 1);
                    Layers[i]--;
                } else {
                    int previousLayerSize = Layers[i - 1];
                    float[] neuronWeights = new float[previousLayerSize];

                    for (int j = 0; j < previousLayerSize; j++) {
                        neuronWeights[j] = UnityEngine.Random.Range(-1f, 1f);
                    }

                    Array.Resize(ref Weights[i - 1], Weights[i - 1].Length + 1);
                    Weights[i - 1][Weights[i - 1].Length - 1] = neuronWeights;
                    Layers[i]++;
                }
            }
        }
    }
}
