using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithmManager : MonoBehaviour {
    public PopulationManager populationManager;
    public List<DNA> bestGenesHistory; // Altera��o: Lista de DNAs
    public int currentGenerationIndex = 0;

    private void Start() {
        populationManager = GetComponent<PopulationManager>();
        bestGenesHistory = new List<DNA>(); // Altera��o: Inicializa��o da lista de DNAs
    }

    public void SaveBestGenes(DNA bestGenes) {
        bestGenesHistory.Add(bestGenes); // Altera��o: Adiciona o DNA � lista
        currentGenerationIndex++;
    }
}
