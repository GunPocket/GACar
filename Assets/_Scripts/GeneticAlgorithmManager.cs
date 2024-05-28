using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithmManager : MonoBehaviour {
    public PopulationManager populationManager;
    public List<float[]> bestGenesHistory;
    public int currentGenerationIndex = 0;

    private void Start() {
        populationManager = GetComponent<PopulationManager>();
        bestGenesHistory = new List<float[]>();
    }

    private void Update() {
        if (populationManager.BestGenes != null && currentGenerationIndex < bestGenesHistory.Count) {
            bestGenesHistory[currentGenerationIndex] = populationManager.BestGenes.NeuralNetwork.Weights[0][0];
        } else if (populationManager.BestGenes != null) {
            bestGenesHistory.Add(populationManager.BestGenes.NeuralNetwork.Weights[0][0]);
        }
    }

    public void SaveBestGenes(DNA bestGenes) {
        bestGenesHistory.Add(bestGenes.NeuralNetwork.Weights[0][0]);
        currentGenerationIndex++;
    }
}
