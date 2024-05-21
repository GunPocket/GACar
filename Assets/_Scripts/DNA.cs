using UnityEngine;

public class DNA {
    public float[] genes; // Genes representando acelera��o, frenagem e dire��o
    public float fitness;
    public float mutationRate = 0.01f;
    private System.Random random;

    public DNA(int geneLength) {
        random = new System.Random();
        genes = new float[geneLength];
        for (int i = 0; i < geneLength; i++) {
            genes[i] = Random.Range(-1f, 1f); // Valores entre -1 e 1 para representar dire��o, acelera��o e frenagem
        }
    }

    public void CalculateFitness(float distance) {
        fitness = distance; // Dist�ncia percorrida como m�trica de aptid�o
    }

    public DNA Crossover(DNA partner) {
        DNA offspring = new DNA(genes.Length);
        int midpoint = Random.Range(0, genes.Length);
        for (int i = 0; i < genes.Length; i++) {
            offspring.genes[i] = i <= midpoint ? this.genes[i] : partner.genes[i];
        }
        return offspring;
    }

    public void Mutate() {
        for (int i = 0; i < genes.Length; i++) {
            if (random.NextDouble() < mutationRate) {
                genes[i] = Random.Range(-1f, 1f);
            }
        }
    }
}
