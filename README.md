# Algoritmo Genético em Unity para Condução de Carros

## Descrição
Este projeto implementa um algoritmo genético em Unity para treinar um carro a chegar à linha de chegada evitando obstáculos. O algoritmo utiliza uma população de DNAs que representam as ações do carro (acelerar, frear e virar) e evolui através de seleção, crossover e mutação.

## Estrutura do Projeto
- **DNA.cs**: Define os genes e calcula a aptidão.
- **PopulationManager.cs**: Gerencia a população de DNAs, cálculo de aptidão e evolução.
- **GeneticAlgorithmManager.cs**: Instancia os carros baseados nos melhores genes de cada geração.
- **CarController.cs**: Controla o movimento do carro conforme os genes.

## Como Usar
1. Clone este repositório.
2. Importe o projeto no Unity.
3. Adicione os scripts aos GameObjects correspondentes no Unity.
4. Configure os obstáculos e a linha de chegada na cena.
5. Execute o projeto e observe o algoritmo genético treinar os carros.

## Dependências
- Unity 2020.3 ou superior.


# Genetic Algorithm in Unity for Car Driving

## Description
This project implements a genetic algorithm in Unity to train a car to reach the finish line while avoiding obstacles. The algorithm uses a population of DNAs representing the car's actions (accelerate, brake, and steer) and evolves through selection, crossover, and mutation.

## Project Structure
- **DNA.cs**: Defines genes and calculates fitness.
- **PopulationManager.cs**: Manages the population of DNAs, fitness calculation, and evolution.
- **GeneticAlgorithmManager.cs**: Instantiates cars based on the best genes from each generation.
- **CarController.cs**: Controls the car's movement according to the genes.

## How to Use
1. Clone this repository.
2. Import the project into Unity.
3. Attach the scripts to the corresponding GameObjects in Unity.
4. Set up obstacles and the finish line in the scene.
5. Run the project and observe the genetic algorithm training the cars.

## Dependencies
- Unity 2020.3 or later.
