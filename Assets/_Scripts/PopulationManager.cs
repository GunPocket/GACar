using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class PopulationManager : MonoBehaviour {
    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject targetPrefab;

    [Header("Características da população")]
    [SerializeField] private int populationSize = 100;
    [SerializeField][Range(0f, 1f)] private float mutationRate = 0.01f;
    [SerializeField] private float targetSpawnRadius = 20f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float simulationTime = 30f;

    [Header("DNA Predefinido")]
    [TextArea(1, 5)][SerializeField] private string predefinedDNAString;
    private DNA predefinedDNA;

    [Header("Características da Rede Neural")]
    [SerializeField] private int inputLayerSize = 14;
    [SerializeField] private int outputLayerSize = 3;

    [Header("Textos da UI")]
    [SerializeField] private TMP_Text GenerationText;
    [SerializeField] private TMP_Text ElapsedTime;
    [SerializeField] private TMP_Text BestFitness;

    [Header("Script de desenhará o cérebro")]
    [SerializeField] private BrainDrawer BrainDrawer;

    private List<DNA> population = new List<DNA>();
    private List<DNA> matingPool = new List<DNA>();
    private List<GameObject> targets = new List<GameObject>();
    private List<GameObject> cars = new List<GameObject>();
    private List<float> fitnessHistory = new List<float>();
    private int generationCount = 0;
    private bool isFinished = false;
    private float bestFitness = 0f;
    public DNA BestGenes;

    private void Start() {
        BestFitness.text = "0";
        if (string.IsNullOrEmpty(predefinedDNAString)) {
            predefinedDNA = DNA.FromString(predefinedDNAString);
        }
        GenerateTargets();
        if (predefinedDNA == null) {
            SpawnInitialPopulation();
        } else {
            SpawnCar(predefinedDNA);
        }
        UpdateGenerationText();
        StartCoroutine(EvolvePopulation());
    }


    private void GenerateTargets() {
        targets.Clear();
        for (int i = 0; i < maxTargets; i++) {
            Vector3 randomPosition = Vector3.zero;
            bool validPosition = false;

            while (!validPosition) {
                randomPosition = new Vector3(
                    Random.Range(-targetSpawnRadius, targetSpawnRadius),
                    Random.Range(-targetSpawnRadius, targetSpawnRadius),
                    0);

                if (Vector3.Distance(randomPosition, Vector3.zero) < 10f) continue;

                validPosition = true;
                foreach (GameObject target in targets) {
                    if (Vector3.Distance(target.transform.position, randomPosition) < 10f) {
                        validPosition = false;
                        break;
                    }
                }
            }

            GameObject targetObject = Instantiate(targetPrefab, randomPosition, Quaternion.identity);
            targets.Add(targetObject);
        }
    }


    private void SpawnInitialPopulation() {
        for (int i = 0; i < populationSize; i++) {
            SpawnCar(new DNA(new int[] { inputLayerSize, outputLayerSize }));
        }
    }

    private void SpawnCar(DNA dna) {
        if (population.Count != 100) population.Add(dna);
        GameObject car = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        car.GetComponent<CarController>().Brain = dna;
        car.GetComponent<CarController>().SetPopulationManager(this);
        cars.Add(car);
    }

    public List<Vector3> GetTargetPositions() {
        List<Vector3> targetPositions = new List<Vector3>();
        foreach (var target in targets) {
            targetPositions.Add(target.transform.position);
        }
        return targetPositions;
    }

    private IEnumerator EvolvePopulation() {
        while (!isFinished) {
            yield return new WaitForSeconds(simulationTime);
            EvaluatePopulation();
            CreateNewGeneration();
            generationCount++;
            UpdateGenerationText();

            fitnessHistory.Add(bestFitness);
        }
    }


    private void EvaluatePopulation() {
        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        int matingPoolSize = Mathf.Max(1, Mathf.FloorToInt(populationSize * 0.1f));

        int rangeCount = Mathf.Min(matingPoolSize, population.Count);

        matingPool = new List<DNA>(population.GetRange(0, rangeCount));

        bestFitness = population[0].Fitness;
        BestGenes = population[0];
        BrainDrawer.DrawBrain(BestGenes);
    }

    private void CreateNewGeneration() {
        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        List<DNA> elite = new List<DNA>(population.GetRange(0, 10));

        List<DNA> newPopulation = new List<DNA>();

        newPopulation.AddRange(elite);

        for (int i = 0; i < populationSize - 10; i++) {
            DNA parent1 = matingPool[Random.Range(0, matingPool.Count)];
            DNA parent2 = matingPool[Random.Range(0, matingPool.Count)];

            DNA offspring = parent1.Crossover(parent2);
            offspring.Mutate(mutationRate);
            newPopulation.Add(offspring);
        }
        population = newPopulation;

        ResetScene();
    }

    private void ResetScene() {
        print($"Generation {generationCount} best fitness: {bestFitness.ToString("F2")}");

        DestroyTargets();
        GenerateTargets();
        DestroyCars();
        GenerateCars();
    }

    private void DestroyTargets() {
        foreach (GameObject t in targets) {
            Destroy(t);
        }
        targets.Clear();
    }

    private void DestroyCars() {
        List<GameObject> carsCopy = new List<GameObject>(cars);

        foreach (GameObject car in carsCopy) {
            Destroy(car);
        }
        cars.Clear();
    }

    private void GenerateCars() {
        foreach (DNA dna in population) {
            SpawnCar(dna);
        }
    }

    private void UpdateGenerationText() {
        if (GenerationText != null) {
            GenerationText.text = $"Geração atual: {generationCount.ToString()}";
        }
    }

    private void SaveFitnessHistory() {
        string fileName = "fitness_history.txt";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        Debug.Log("Caminho do arquivo: " + filePath);

        using (StreamWriter writer = new StreamWriter(filePath)) {
            foreach (float fitness in fitnessHistory) {
                writer.WriteLine(fitness.ToString());
            }
        }
    }

    private void Update() {
        if (ElapsedTime != null) {
            ElapsedTime.text = $"Tempo decorrido: {Mathf.RoundToInt(Time.timeSinceLevelLoad).ToString()}s";
        }
        if (BestFitness != null) {
            BestFitness.text = $"Melhor Fitness: {bestFitness.ToString("F2")}";
        }
    }

    private void OnApplicationQuit() {
        SaveFitnessHistory();
        if (BestGenes != null) {
            Debug.Log("Melhor DNA do carro:");
            Debug.Log(BestGenes.ToReadableString(BestGenes.NeuralNetwork.Weights));
        }
    }

    private void OnDrawGizmos() {
        if (targets.Count == 0) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targets[0].transform.position, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targets[1].transform.position, 0.5f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targets[2].transform.position, 0.5f);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(targets[3].transform.position, 0.5f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(targets[4].transform.position, 0.5f);
    }
}