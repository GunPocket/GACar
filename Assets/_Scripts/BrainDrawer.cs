using System.Collections.Generic;
using UnityEngine;

public class BrainDrawer : MonoBehaviour {
    public GameObject neuronPrefab;
    public GameObject synapsePrefab;
    public RectTransform drawingArea;
    public float layerSpacing = 15f;
    public float neuronSpacing = 10f;
    public Gradient neuronColorGradient; // Gradiente de cores para neurônios
    public Gradient synapseColorGradient; // Gradiente de cores para sinapses
    private readonly List<GameObject> neurons = new();
    private readonly List<GameObject> synapses = new();

    public void UpdateBrain(NeuralNetwork network) {
        
        return;/*
        
        ClearExistingBrain();

        if (network == null || (network.HiddenLayer == null || network.HiddenLayer.Length == 0)) {
            if (network == null) {
                Debug.LogWarning("Network is null. Aborting UpdateBrain.");
            }
            DrawSynapsesDirectly(network?.InputLayer.Length ?? 0, network?.OutputLayer.Length ?? 0);
            return;
        }

        int inputNeuronCount = network.InputLayer.Length;
        int hiddenNeuronCount = network.HiddenLayer.Length;
        int outputNeuronCount = network.OutputLayer.Length;

        float startX = -(inputNeuronCount - 1) * layerSpacing / 2;
        float startY = 0; // Inicializa a posição Y com 0

        for (int i = 0; i < hiddenNeuronCount; i++) {
            Vector3 neuronPosition = new Vector3(startX, startY + i * neuronSpacing, 0); // Posição do neurônio

            GameObject neuronObject = Instantiate(neuronPrefab, drawingArea); // Instancia o objeto filho do drawingArea
            neuronObject.transform.localPosition = neuronPosition; // Define a posição local do objeto filho
            neurons.Add(neuronObject);

            Color neuronColor = neuronColorGradient.Evaluate(CalculateBiasColorValue(network.HiddenLayer[i].Bias));
            neuronObject.GetComponent<Renderer>().material.color = neuronColor;

            foreach (var synapse in network.Synapses) {
                if (synapse.InputNeuronIndex == i + inputNeuronCount) {
                    int targetNeuronIndex = synapse.OutputNeuronIndex - inputNeuronCount - hiddenNeuronCount;
                    if (targetNeuronIndex >= 0 && targetNeuronIndex < outputNeuronCount) {
                        Vector3 targetPosition = new Vector3(layerSpacing, targetNeuronIndex * neuronSpacing, 0); // Posição do alvo

                        GameObject synapseObject = Instantiate(synapsePrefab, drawingArea); // Instancia o objeto filho do drawingArea
                        synapseObject.transform.localPosition = neuronPosition; // Define a posição local do objeto filho
                        LineRenderer lineRenderer = synapseObject.GetComponent<LineRenderer>();
                        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, targetPosition }); // Define a posição relativa do LineRenderer
                        synapses.Add(synapseObject);

                        Color synapseColor = synapseColorGradient.Evaluate(CalculateWeightColorValue(synapse.Weight));
                        lineRenderer.material.color = synapseColor;
                    }
                }
            }
        }
    }

    private float CalculateBiasColorValue(float bias) {
        // Normaliza o valor do bias para estar dentro do intervalo [0, 1]
        return Mathf.Clamp01((bias + 1f) / 2f);
    }

    private float CalculateWeightColorValue(float weight) {
        // Normaliza o valor do peso para estar dentro do intervalo [0, 1]
        return Mathf.Clamp01(weight);
    }

    private void ClearExistingBrain() {
        foreach (var neuron in neurons) {
            Destroy(neuron);
        }
        neurons.Clear();

        foreach (var synapse in synapses) {
            Destroy(synapse);
        }
        synapses.Clear();
    }

    private void DrawSynapsesDirectly(int inputLayerSize, int outputLayerSize) {
        // Calcular a posição inicial Y para os neurônios de entrada e saída
        float startYInput = -(inputLayerSize - 1) * neuronSpacing / 2;
        float startYOutput = -(outputLayerSize - 1) * neuronSpacing / 2;

        // Obter o tamanho do RectTransform da área de desenho
        Vector2 drawingAreaSize = drawingArea.rect.size;

        // Obter o tamanho do primeiro neurônio na camada de entrada
        GameObject sampleNeuron = Instantiate(neuronPrefab);
        RectTransform sampleNeuronRectTransform = sampleNeuron.GetComponent<RectTransform>();
        float neuronWidth = sampleNeuronRectTransform.rect.width;
        float neuronHeight = sampleNeuronRectTransform.rect.height;

        // Calcular o tamanho total ocupado pelos neurônios de entrada e saída
        float totalInputWidth = neuronWidth;
        float totalOutputWidth = neuronWidth + layerSpacing;
        float totalWidth = totalInputWidth + totalOutputWidth;

        // Calcular o espaçamento horizontal com base no tamanho total
        float horizontalSpacing = (drawingAreaSize.x - totalWidth) / (outputLayerSize + 1);

        // Desenhar sinapses das entradas para as saídas
        for (int i = 0; i < inputLayerSize; i++) {
            Vector3 inputNeuronPosition = new Vector3(horizontalSpacing + totalInputWidth / 2, startYInput + i * neuronHeight, 0);
            GameObject inputNeuronObject = Instantiate(neuronPrefab, drawingArea);
            inputNeuronObject.transform.localPosition = inputNeuronPosition + new Vector3(-drawingAreaSize.x / 2, -drawingAreaSize.y / 2, 0);
            neurons.Add(inputNeuronObject);

            for (int j = 0; j < outputLayerSize; j++) {
                Vector3 outputNeuronPosition = new Vector3(horizontalSpacing + totalInputWidth + layerSpacing + (j * (horizontalSpacing + neuronWidth)) + neuronWidth / 2, startYOutput + i * neuronHeight, 0);
                GameObject outputNeuronObject = Instantiate(neuronPrefab, drawingArea);
                outputNeuronObject.transform.localPosition = outputNeuronPosition + new Vector3(-drawingAreaSize.x / 2, -drawingAreaSize.y / 2, 0);
                neurons.Add(outputNeuronObject);

                Vector3 synapseStartPosition = inputNeuronObject.transform.position;
                Vector3 synapseEndPosition = outputNeuronObject.transform.position;

                GameObject synapseObject = Instantiate(synapsePrefab, drawingArea);
                LineRenderer lineRenderer = synapseObject.GetComponent<LineRenderer>();
                lineRenderer.SetPositions(new Vector3[] { synapseStartPosition, synapseEndPosition });
                synapses.Add(synapseObject);

                Color synapseColor = synapseColorGradient.Evaluate(0.5f); // Define uma cor intermediária para as sinapses
                lineRenderer.material.color = synapseColor;
            }
        }

        // Destruir o neurônio de exemplo usado para obter o tamanho
        Destroy(sampleNeuron);*/
    }

}
