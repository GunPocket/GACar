using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopulationManager : MonoBehaviour {
    [Header("Time Scale")]
    public float TimeScale = 6.0f;

    [Header("Prefabs")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject targetPrefab;

    [Header("Population Characteristics")]
    [SerializeField] private int populationSize = 100;
    [SerializeField][Range(0f, 1f)] private float mutationRate = 0.01f;
    [SerializeField] private float targetSpawnRadius = 20f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float simulationTime = 30f;

    [Header("Predefined DNA")]
    [TextArea(1, 5)][SerializeField] private string predefinedDNAString;
    private DNA predefinedDNA;

    [Header("Neural Network Characteristics")]
    [SerializeField] private int inputLayerSize = 14;
    [SerializeField] private int outputLayerSize = 3;
    [SerializeField] private ActivationFunctionType activationFunctionType = ActivationFunctionType.ReLU;
    [SerializeField] private RegularizationType regularizationType = RegularizationType.L2;
    [SerializeField] private OptimizationAlgorithm optimizationAlgorithm = OptimizationAlgorithm.Adam;

    [Header("UI Texts")]
    [SerializeField] private TMP_Text GenerationText;
    [SerializeField] private TMP_Text ElapsedTime;
    [SerializeField] private TMP_Text BestFitness;

    [Header("Brain Drawing Script")]
    [SerializeField] private BrainDrawer BrainDrawer;

    [Header("Compute Shader")]
    public ComputeShader NeuralNetworkCompute;

    private List<DNA> population = new();
    private readonly List<GameObject> targets = new();
    private readonly List<GameObject> cars = new();
    private int generationCount = 0;
    private readonly bool isFinished = false;
    private float bestFitness = 0f;
    public DNA BestGenes;

    private void Start() {
        Time.timeScale = TimeScale;
        BestFitness.text = "0";
        if (!string.IsNullOrEmpty(predefinedDNAString)) {
            predefinedDNA = DNA.FromJson(predefinedDNAString);
        }
        GenerateTargets();
        //if (predefinedDNA == null) {
            SpawnInitialPopulation();
        //} else {
        //    SpawnCar(predefinedDNA);
        //}
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
                    UnityEngine.Random.Range(-targetSpawnRadius, targetSpawnRadius),
                    UnityEngine.Random.Range(-targetSpawnRadius, targetSpawnRadius),
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
            List<NeuralLayer> layers = new() {
            new NeuralLayer(inputLayerSize, activationFunctionType, regularizationType, optimizationAlgorithm),
            new NeuralLayer(outputLayerSize, activationFunctionType, regularizationType, optimizationAlgorithm)
        };

            NeuralNetwork neuralNetwork = new(
                layers,
                activationFunctionType,
                regularizationType,
                0.01f,
                optimizationAlgorithm,
                0.01f,
                100,
                0.1f
            );

            DNA dna = new(neuralNetwork);

            SpawnCar(dna);
        }
    }

    private void SpawnCar(DNA dna) {
        population.Add(dna);
        GameObject car = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        var carController = car.GetComponent<CarController>();
        carController.SetBrain(dna);
        carController.SetPopulationManager(this);
        carController.SetShader(NeuralNetworkCompute);
        cars.Add(car);
    }

    public List<Vector3> GetTargetPositions() {
        List<Vector3> targetPositions = new();
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
        }
    }

    private void EvaluatePopulation() {
        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        bestFitness = population[0].Fitness;
        BestGenes = population[0];

        BrainDrawer.UpdateBrain(BestGenes);
    }

    private void CreateNewGeneration() {
        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        List<DNA> elite = new(population.GetRange(0, 10));

        List<DNA> newPopulation = new();

        newPopulation.AddRange(elite);

        for (int i = 0; i < populationSize - 10; i++) {
            DNA parent1 = population[UnityEngine.Random.Range(0, population.Count)];
            DNA parent2 = population[UnityEngine.Random.Range(0, population.Count)];

            DNA offspring = parent1.Crossover(parent2);
            offspring.Mutate(mutationRate);
            newPopulation.Add(offspring);
        }
        population = newPopulation;

        ResetScene();
    }

    private void ResetScene() {
        Debug.Log($"Generation {generationCount} best fitness: {bestFitness:F2}");

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
        List<GameObject> carsCopy = new(cars);

        foreach (GameObject car in carsCopy) {
            Destroy(car);
        }
        cars.Clear();
    }

    private void GenerateCars() {
        List<DNA> populationCopy = new List<DNA>(population); // Cria uma cópia da lista population

        foreach (DNA dna in populationCopy) { // Itera sobre a cópia
            SpawnCar(dna);
        }
    }


    private void UpdateGenerationText() {
        if (GenerationText != null) {
            GenerationText.text = $"Current Generation: {generationCount}";
        }
    }

    private void Update() {
        if (ElapsedTime != null) {
            ElapsedTime.text = $"Elapsed Time: {Mathf.RoundToInt(Time.timeSinceLevelLoad)}s";
        }
        if (BestFitness != null) {
            BestFitness.text = $"Best Fitness: {bestFitness:F2}";
        }
    }

    private void OnApplicationQuit() {
        if (BestGenes != null) {
            string json = BestGenes.ToJson(); // Converte o melhor gene em JSON
            string filePath = Application.persistentDataPath + "/BestGene.json"; // Define o caminho do arquivo JSON
            System.IO.File.WriteAllText(filePath, json); // Escreve o JSON no arquivo
            Debug.Log("Melhor gene salvo em: " + filePath); // Mostra o caminho onde o JSON foi salvo
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
