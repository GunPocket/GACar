using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithmManager : MonoBehaviour {
    public PopulationManager populationManager;
    public List<DNA> bestGenesHistory; // Alteração: Lista de DNAs
    public int currentGenerationIndex = 0;

    private void Start() {
        populationManager = GetComponent<PopulationManager>();
        bestGenesHistory = new List<DNA>(); // Alteração: Inicialização da lista de DNAs
    }

    public void SaveBestGenes(DNA bestGenes) {
        bestGenesHistory.Add(bestGenes); // Alteração: Adiciona o DNA à lista
        currentGenerationIndex++;
    }
}
