public class DNA {
    public NeuralNetwork NeuralNetwork;
    public float Fitness = 0;
    public bool IsElite = false;

    public DNA(NeuralNetwork neuralNetwork) {
        NeuralNetwork = neuralNetwork;
    }

    public DNA Crossover(DNA partner) {
        NeuralNetwork offspringNetwork = NeuralNetwork.Crossover(partner.NeuralNetwork);
        DNA offspring = new(offspringNetwork);
        return offspring;
    }

    public void Mutate(float mutationRate) {
        NeuralNetwork.Mutate(mutationRate);
    }
}
