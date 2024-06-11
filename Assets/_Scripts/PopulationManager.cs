using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopulationManager : MonoBehaviour {
    public float TimeScale = 6.0f;

    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject targetPrefab;

    [SerializeField] private int populationSize = 100;
    [Range(0f, 1f)][SerializeField] private float mutationRate = 0.01f;
    [SerializeField] private float targetSpawnRadius = 20f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private float simulationTime = 30f;

    [SerializeField] private int inputLayerSize = 4;
    [SerializeField] private int hiddenLayerSize = 0;
    [SerializeField] private int outputLayerSize = 3;
    [SerializeField] private ActivationFunctionType activationFunctionType = ActivationFunctionType.ReLU;

    [SerializeField] private TMP_Text GenerationText;
    [SerializeField] private TMP_Text ElapsedTime;
    [SerializeField] private TMP_Text BestFitness;

    [SerializeField] private BrainDrawer BrainDrawer;

    private List<DNA> population = new();
    private readonly List<GameObject> targets = new();
    private readonly List<GameObject> cars = new();
    private int generationCount = 0;
    private readonly bool isFinished = false;
    public DNA BestGenes;

    private void Start() {
        BestFitness.text = "0";

        GenerateTargets();
        InitializePopulation();
        UpdateGenerationText();
        SpawnInitialPopulation();
        StartCoroutine(EvolvePopulation());
    }

    private void InitializePopulation() {
        for (int i = 0; i < populationSize; i++) {
            DNA dna = new(new NeuralNetwork(
                activationFunctionType,
                inputLayerSize,
                hiddenLayerSize,
                outputLayerSize
            ));
            population.Add(dna);
        }
        BrainDrawer.UpdateBrain(population[0].NeuralNetwork);
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
        foreach (var dna in population) {
            SpawnCar(dna);
        }
    }

    private void SpawnCar(DNA dna) {
        GameObject carObject = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        var carController = carObject.GetComponent<CarController>();

        SetCarColor(carObject, dna.IsElite);

        carController.SetBrain(dna);
        carController.SetPopulationManager(this);
        cars.Add(carObject);
    }

    private void SetCarColor(GameObject carObject, bool isElite) {
        SpriteRenderer carRenderer = carObject.GetComponentInChildren<SpriteRenderer>();
        if (carRenderer != null) {
            carRenderer.sharedMaterial.color = isElite ? Color.cyan : Color.white;
        }
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
            CreateNewGeneration();
            generationCount++;
            UpdateGenerationText();
        }
    }

    private void ResetBrain(DNA dna) {
        dna.NeuralNetwork = new NeuralNetwork(
            activationFunctionType,
            inputLayerSize,
            hiddenLayerSize,
            outputLayerSize
        );
        dna.Fitness = 0;
    }

    private void CreateNewGeneration() {
        if (population.Count >= 2) {
            List<DNA> elite = new();
            List<DNA> nonZeroFitnessPopulation = population.FindAll(dna => dna.Fitness > 0);

            BestGenes = nonZeroFitnessPopulation.Count > 0 ? nonZeroFitnessPopulation[0] : null;

            if (nonZeroFitnessPopulation.Count > 0) {
                int eliteSize = Mathf.Min(Mathf.RoundToInt(populationSize * 0.1f), nonZeroFitnessPopulation.Count);
                elite = nonZeroFitnessPopulation.GetRange(0, eliteSize);
            }

            List<DNA> newPopulation = new(elite);
            int remainingPopulationSize = populationSize - elite.Count;
            for (int i = 0; i < remainingPopulationSize; i++) {
                DNA parent1 = elite[Random.Range(0, elite.Count)];
                DNA parent2 = elite[Random.Range(0, elite.Count)];

                DNA offspring = parent1.Crossover(parent2);
                offspring.Mutate(mutationRate);

                newPopulation.Add(offspring);
            }

            population = newPopulation;
            ResetScene();
        } else {
            InitializePopulation();
        }
    }

    private void ResetScene() {
        Debug.Log($"Generation {generationCount} best fitness: {BestGenes?.Fitness:F2}");

        DestroyTargets();
        GenerateTargets();
        ResetCars();
    }

    private void ResetCars() {
        foreach (var car in cars) {
            var carController = car.GetComponent<CarController>();
            carController.ResetCar();
            carController.SetPopulationManager(this);
        }
    }

    private void DestroyTargets() {
        foreach (GameObject t in targets) {
            Destroy(t);
        }
        targets.Clear();
    }

    private void UpdateGenerationText() {
        if (GenerationText != null) {
            GenerationText.text = $"Generation: {generationCount}";
        }
        if (ElapsedTime != null) {
            ElapsedTime.text = $"Elapsed Time: {Time.timeSinceLevelLoad:F2}s";
        }
        if (BestGenes != null) {
            BestFitness.text = $"Best Fitness: {BestGenes.Fitness:F2}";
        }
    }

    private void LateUpdate() {
        Time.timeScale = TimeScale;
    }
}