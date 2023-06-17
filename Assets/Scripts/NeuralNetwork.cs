using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    /// <summary>
    /// shape
    /// </summary>
    public List<int> layerShape;

    public List<Matrix<float>> allLayers = new List<Matrix<float>>();
    public List<Matrix<float>> weights = new List<Matrix<float>>();
    public List<float> biases = new List<float>();
    
    public bool started;

    public float fitness;

    public void Init(List<int> layerShape)
    {
        started = true;

        Debug.Assert( layerShape != null && layerShape.Count >= 3);
        this.layerShape = layerShape;

        allLayers.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < layerShape.Count; i++)
        {
            // layers
            var layer = MatrixUtil.NewMatrix<float>(1, layerShape[i]);
            allLayers.Add(layer);

            // weights, skip last
            if(i < layerShape.Count - 1)
            {
                Matrix<float> weight = MatrixUtil.NewMatrix<float>(layerShape[i], layerShape[i + 1]);
                weights.Add(weight);
            }

            // bias, skip first
            if (i != 0) 
            { 
                biases.Add(Random.Range(-1f, 1f));
            } 
        }

        RandomWeights();
    }

    public void RandomWeights()
    {
        foreach (var weight in weights)
        {
            // todo ref, matrix extend
            for (int x = 0; x < weight.RowCount; x++)
            {
                for (int y = 0; y < weight.ColumnCount; y++)
                {
                    weight[x, y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public List<float> RunNetwork(List<float> inputs)
    {
        // input layer
        var inputLayer = allLayers.FirstOrDefault();
        for (int i = 0; i < inputs.Count; i++)
        {
            inputLayer[0, i] = inputs[i];
        }
        // todo ref, -1, 1
        // sigmold: 0, 1
        inputLayer = inputLayer.PointwiseTanh();

        // other layers, start from 1
        for (int i = 1; i < allLayers.Count; i++)
        {
            allLayers[i] = ((allLayers[i - 1] * weights[i - 1]) + biases[i - 1]).PointwiseTanh();
        }

        var outputLayer = allLayers.LastOrDefault();
        //.PointwiseTanh().Row(0).ToList(); 
        //(Sigmoid(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
        var output = new List<float>
        {
            Sigmoid(outputLayer[0, 0]),
            (float)Math.Tanh(outputLayer[0, 1])
        };
        return output;
    }

    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }


    #region generic
    public NeuralNetwork GetCopy()
    {
        NeuralNetwork newNet = new NeuralNetwork();
        newNet.Init(this.layerShape);

        // copy DNA
        newNet.weights = this.weights;
        newNet.biases = this.biases;

        return newNet;
    }
    #endregion
}
