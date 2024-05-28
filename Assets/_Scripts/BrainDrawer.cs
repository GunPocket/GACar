using UnityEngine;
using UnityEngine.UI;

public class BrainDrawer : MonoBehaviour {
    private DNA bestGenes;
    [SerializeField] private GameObject neuronPrefab;
    [SerializeField] private Color neuronColor;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private Color connectionColor;

    [SerializeField] private RectTransform panelRect;

    public void DrawBrain(DNA dna) {
        bestGenes = dna;

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        if (bestGenes == null || bestGenes.NeuralNetwork == null) {
            Debug.LogWarning("Nenhum DNA ou rede neural definidos para desenhar.");
            return;
        }

        DrawConnections();
        DrawNeurons();
    }

    private void DrawNeurons() {
        for (int layerIndex = 0; layerIndex < bestGenes.NeuralNetwork.Layers.Length; layerIndex++) {
            int neuronCount = bestGenes.NeuralNetwork.Layers[layerIndex];
            float yOffset = (1f / (neuronCount + 1)) * panelRect.rect.height;

            for (int neuronIndex = 0; neuronIndex < neuronCount; neuronIndex++) {
                float xPos = (layerIndex + 0.5f) * (panelRect.rect.width / (bestGenes.NeuralNetwork.Layers.Length + 1));
                float yPos = (neuronIndex + 1) * yOffset;

                GameObject neuron = Instantiate(neuronPrefab, panelRect);
                RectTransform rt = neuron.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20f, 20f);
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(xPos, yPos);

                if (layerIndex < bestGenes.NeuralNetwork.LayerActivations.Length) {
                    if (neuronIndex < bestGenes.NeuralNetwork.LayerActivations[layerIndex].Length) {
                        float activation = bestGenes.NeuralNetwork.LayerActivations[layerIndex][neuronIndex];
                        Color neuronColor = GetNeuronColor(activation);
                        neuron.GetComponent<RawImage>().color = neuronColor;
                    }
                }
            }
        }
    }


    private Color GetNeuronColor(float activation) {
        if (activation > 0.5f) {
            return Color.green; // Ativado
        } else if (activation < -0.5f) {
            return Color.red; // Desativado
        } else {
            return Color.gray; // Neutro
        }
    }

    private void DrawConnections() {
        for (int i = 1; i < bestGenes.NeuralNetwork.Layers.Length; i++) {
            int previousLayerNeuronCount = bestGenes.NeuralNetwork.Layers[i - 1];
            int currentLayerNeuronCount = bestGenes.NeuralNetwork.Layers[i];

            float xOffset = panelRect.rect.width / (bestGenes.NeuralNetwork.Layers.Length + 1);

            for (int previousNeuronIndex = 0; previousNeuronIndex < previousLayerNeuronCount; previousNeuronIndex++) {
                float xPos1 = (i - 1 + 0.5f) * xOffset;
                float yPos1 = ((previousNeuronIndex + 1) * (1f / (previousLayerNeuronCount + 1))) * panelRect.rect.height;

                for (int currentNeuronIndex = 0; currentNeuronIndex < currentLayerNeuronCount; currentNeuronIndex++) {
                    float xPos2 = (i + 0.5f) * xOffset;
                    float yPos2 = ((currentNeuronIndex + 1) * (1f / (currentLayerNeuronCount + 1))) * panelRect.rect.height;

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
