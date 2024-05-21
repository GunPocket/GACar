using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneticAlgorithmManager : MonoBehaviour {
    public GameObject carPrefab;
    public GameObject targetPrefab; // Prefab dos alvos
    public TMP_Text generationText;
    public PopulationManager populationManager;
    public List<float[]> bestGenesHistory;
    public int currentGenerationIndex = 0;
    public float targetSpawnRadius = 20f; // Raio de spawn dos alvos

    void Start() {
        populationManager = GetComponent<PopulationManager>();
        bestGenesHistory = new List<float[]>(); // Inicializa a lista
        StartCoroutine(InstantiateCarsAndTargets()); // Chama a coroutine que instanciará os carros e os alvos
    }

    IEnumerator InstantiateCarsAndTargets() {
        while (currentGenerationIndex < bestGenesHistory.Count) {
            // Instancia o carro
            GameObject car = Instantiate(carPrefab);
            Vector3 carPosition = car.transform.position; // Posição do carro

            // Instancia os alvos em posições aleatórias dentro do raio em relação ao carro
            for (int i = 0; i < populationManager.maxTargets; i++) {
                Vector3 randomOffset = Random.insideUnitCircle * targetSpawnRadius;
                GameObject target = Instantiate(targetPrefab, carPosition + randomOffset, Quaternion.identity);
            }

            yield return new WaitForSeconds(6f);
            currentGenerationIndex++;
        }
    }

    void Update() {
        generationText.text = "Generation: " + currentGenerationIndex;
    }
}
