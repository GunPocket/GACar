using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrainDrawer : MonoBehaviour {
    private DNA bestGenes;
    [SerializeField] private GameObject neuronPrefab;
    [SerializeField] private Color neuronColor;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private Color connectionColor;
    [SerializeField] private RectTransform panelRect;

    public void UpdateBrain(DNA dna) {
        bestGenes = dna;
        DrawBrain();
    }

    private void DrawBrain() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        if (bestGenes == null || bestGenes.NeuralNetwork == null) {
            Debug.LogWarning("No DNA or neural network defined to draw.");
            return;
        }

        DrawConnections();
        DrawNeurons();
    }

    private void DrawNeurons() {
        for (int layerIndex = 0; layerIndex < bestGenes.NeuralNetwork.Layers.Count; layerIndex++) {
            int neuronCount = bestGenes.NeuralNetwork.Layers[layerIndex].Neurons.Count;

            float yOffset = panelRect.rect.height / (neuronCount + 1);

            for (int neuronIndex = 0; neuronIndex < neuronCount; neuronIndex++) {
                float xPos = (layerIndex + 0.5f) * (panelRect.rect.width / (bestGenes.NeuralNetwork.Layers.Count + 1));
                float yPos = (neuronIndex + 1) * yOffset;

                GameObject neuron = Instantiate(neuronPrefab, panelRect);
                RectTransform rt = neuron.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20f, 20f);
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(xPos, yPos);

                float activation = 0;
                Color neuronColor = GetNeuronColor(activation);
                neuron.GetComponent<RawImage>().color = neuronColor;
                //AddNeuronText(neuron, activation);
            }
        }
    }

    private void AddNeuronText(GameObject neuron, float activation) {
        GameObject textObject = new("Text", typeof(RectTransform));
        textObject.transform.SetParent(neuron.transform);
        Text textComponent = textObject.AddComponent<Text>();
        textComponent.text = activation.ToString("F2");
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 10;
        textComponent.color = Color.black;
        textComponent.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = textObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50f, 20f);
        rt.anchoredPosition = Vector2.zero;
    }

    private Color GetNeuronColor(float activation) {
        if (activation > 0.5f) {
            return Color.green; // Activated
        } else if (activation < -0.5f) {
            return Color.red; // Deactivated
        } else {
            return Color.gray; // Neutral
        }
    }

    private void DrawConnections() {
        for (int i = 1; i < bestGenes.NeuralNetwork.Layers.Count; i++) {
            int previousLayerNeuronCount = bestGenes.NeuralNetwork.Layers[i - 1].NeuronCount;
            int currentLayerNeuronCount = bestGenes.NeuralNetwork.Layers[i].NeuronCount;

            float xOffset = panelRect.rect.width / (bestGenes.NeuralNetwork.Layers.Count + 1);

            for (int previousNeuronIndex = 0; previousNeuronIndex < previousLayerNeuronCount; previousNeuronIndex++) {
                float xPos1 = (i - 1 + 0.5f) * xOffset;
                float yPos1 = ((previousNeuronIndex + 1) * (panelRect.rect.height / (previousLayerNeuronCount + 1)));

                for (int currentNeuronIndex = 0; currentNeuronIndex < currentLayerNeuronCount; currentNeuronIndex++) {
                    float xPos2 = (i + 0.5f) * xOffset;
                    float yPos2 = ((currentNeuronIndex + 1) * (panelRect.rect.height / (currentLayerNeuronCount + 1)));

                    foreach (var synapse in bestGenes.NeuralNetwork.Layers[i].Neurons[currentNeuronIndex].IncomingSynapses) {
                        if (synapse.InputNeuron == bestGenes.NeuralNetwork.Layers[i - 1].Neurons[previousNeuronIndex]) {
                            GameObject connection = Instantiate(connectionPrefab, panelRect);

                            RectTransform rt = connection.GetComponent<RectTransform>();
                            rt.sizeDelta = new Vector2(Vector2.Distance(new Vector2(xPos1, yPos1), new Vector2(xPos2, yPos2)), 2f);
                            rt.anchorMin = new Vector2(0, 0);
                            rt.anchorMax = new Vector2(0, 0);
                            rt.pivot = new Vector2(0, 0.5f);
                            rt.anchoredPosition = new Vector2(xPos1, yPos1);
                            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * Mathf.Atan2(yPos2 - yPos1, xPos2 - xPos1));
                        }
                    }
                }
            }
        }
    }
}
