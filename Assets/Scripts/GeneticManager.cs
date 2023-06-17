using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// genetic algrism
/// </summary>
public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController carController;

    [Header("Controls")]
    public int populationSize = 100;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.05f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private int naturalSelected;
    /// <summary>
    /// a list of pupulation id, higher fitness duplicate more times
    /// </summary>
    private List<int> genePool = new List<int>();
    private NeuralNetwork[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGene = 0;

    private void Start()
    {
        carController = FindObjectOfType<CarController>();

        CreatePopulation();
    }

    private void CreatePopulation()
    {
        population = new NeuralNetwork[populationSize];
        InitPopulation(population, 0);
        ResetToCurrentGene();
    }

    // Genome
    private void ResetToCurrentGene()
    {
        carController.ResetWithNetwork(population[currentGene]);
    }

    private void InitPopulation(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while (startingIndex < populationSize)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].Init(carController.layerShape);
            startingIndex++;
        }
    }

    public void Death(float fitness, NeuralNetwork network)
    {
        if (currentGene < population.Length - 1)
        {
            population[currentGene].fitness = fitness;
            currentGene++;
            ResetToCurrentGene();
        }
        else
        {
            RePopulate();
        }
    }


    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturalSelected = 0;
        SortPopulation();

        NeuralNetwork[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        InitPopulation(newPopulation, naturalSelected);

        population = newPopulation;

        currentGene = 0;

        ResetToCurrentGene();

    }

    private void Mutate(NeuralNetwork[] newPopulation)
    {

        for (int i = 0; i < naturalSelected; i++)
        {

            for (int c = 0; c < newPopulation[i].weights.Count; c++)
            {

                if (Random.Range(0.0f, 1.0f) < mutationRate)
                {
                    newPopulation[i].weights[c] = MutateMatrix(newPopulation[i].weights[c]);
                }

            }

        }

    }

    Matrix<float> MutateMatrix(Matrix<float> A)
    {

        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;

    }

    private void Crossover(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            NeuralNetwork Child1 = new NeuralNetwork();
            NeuralNetwork Child2 = new NeuralNetwork();

            Child1.Init(carController.layerShape);
            Child2.Init(carController.layerShape);

            Child1.fitness = 0;
            Child2.fitness = 0;

            // weight
            for (int w = 0; w < Child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }

            }

            // bias
            for (int w = 0; w < Child1.biases.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }
            }

            newPopulation[naturalSelected] = Child1;
            naturalSelected++;

            newPopulation[naturalSelected] = Child2;
            naturalSelected++;
        }
    }

    private NeuralNetwork[] PickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[populationSize];

        // pick best
        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturalSelected] = population[i].GetCopy();
            newPopulation[naturalSelected].fitness = 0;
            naturalSelected++;

            int fitness = Mathf.RoundToInt(population[i].fitness * 10);

            for (int j = 0; j < fitness; j++)
            {
                genePool.Add(i);
            }
        }

        // pick worst
        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }

    private void SortPopulation()
    {
        population = population.OrderByDescending(x => x.fitness).ToArray();
        return;
    }
}
