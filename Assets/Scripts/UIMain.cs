using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    public Text turn;
    public Text engine;
    public Text score;

    public Text generation;
    public Text gene;

    public GeneticManager manager;
    public CarController car;

    private void Update()
    {
        turn.text = car.turn.ToString();
        engine.text = car.accelerate.ToString();
        score.text = car.overallFitness.ToString();

        generation.text = manager.currentGeneration.ToString();
        gene.text = manager.currentGene.ToString();
    }
}
