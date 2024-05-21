using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour {
    public DNA[] population;
    public List<DNA> matingPool;
    public List<float[]> bestGenesHistory;
    public int populationSize = 100;
    public float mutationRate = 0.01f;
    public bool isFinished = false;
    public int generationCount = 0;
    public float[] bestGenes;
    public GameObject carPrefab;
    public GameObject targetPrefab;
    public int maxTargets = 5;
    public float simulationTime = 30f;

    public GameObject[] targets;
    private Vector3[] targetPositions;

    void Start() {
        population = new DNA[populationSize];
        matingPool = new List<DNA>();
        bestGenesHistory = new List<float[]>();
        targets = new GameObject[maxTargets];
        targetPositions = new Vector3[maxTargets];

        for (int i = 0; i < populationSize; i++) {
            population[i] = new DNA(50); // 50 genes para representar o comportamento do carro
        }

        StartCoroutine(EvolvePopulation());
    }

    IEnumerator EvolvePopulation() {
        while (!isFinished && generationCount <= 100) {
            yield return StartCoroutine(CalculateFitness());
            bestGenes = new float[50];
            float bestFitness = 0f;
            for (int i = 0; i < populationSize; i++) {
                if (population[i].fitness > bestFitness) {
                    bestFitness = population[i].fitness;
                    bestGenes = population[i].genes;
                }
            }
            bestGenesHistory.Add(bestGenes);

            if (bestFitness >= 100f) {
                isFinished = true;
                break;
            }

            GenerateMatingPool();
            BreedNewPopulation();

            generationCount++;
            yield return null;
        }
    }

    IEnumerator CalculateFitness() {
        GameObject[] cars = new GameObject[populationSize];
        for (int i = 0; i < populationSize; i++) {
            cars[i] = Instantiate(carPrefab);
            CarController carController = cars[i].GetComponent<CarController>();
            carController.actions = population[i].genes;
            carController.SetPopulationManager(this);
        }

        yield return new WaitForSeconds(simulationTime); // Tempo para os carros completarem suas ações

        for (int i = 0; i < populationSize; i++) {
            Destroy(cars[i]);
        }
    }

    void GenerateMatingPool() {
        matingPool.Clear();
        for (int i = 0; i < populationSize; i++) {
            int n = (int)(population[i].fitness * 100);
            for (int j = 0; j < n; j++) {
                matingPool.Add(population[i]);
            }
        }
    }

    void BreedNewPopulation() {
        for (int i = 0; i < populationSize; i++) {
            DNA parentA = matingPool[Random.Range(0, matingPool.Count)];
            DNA parentB = matingPool[Random.Range(0, matingPool.Count)];
            DNA offspring = parentA.Crossover(parentB);
            offspring.Mutate();
            population[i] = offspring;
        }
    }

    public void CarDestroyed(GameObject car) {
        Destroy(car);
    }

    public void MoveTargets() {
        for (int i = 0; i < maxTargets; i++) {
            targetPositions[i] = GetRandomPosition();
            if (targets[i] == null) {
                targets[i] = Instantiate(targetPrefab);
            }
            targets[i].transform.position = targetPositions[i];
        }
    }

    Vector3 GetRandomPosition() {
        return new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f);
    }
}
