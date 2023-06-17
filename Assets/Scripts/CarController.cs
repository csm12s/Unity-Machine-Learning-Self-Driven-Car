using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class CarController : MonoBehaviour
{
    [Header("Network Options")]
    public List<int> layerShape;
    public float sensorDist = 10;
    public float viewAngle = 180; //In degrees


    [Header("information")]
    [Range(-1f, 1f)]
    public float accelerate, turn;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    private Vector3 startPosition, startRotation;
    private NeuralNetwork network;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    [SerializeField]
    private List<float> sensorInputs;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NeuralNetwork>();

        sensorInputs = new List<float>();
        for (int i = 0; i < layerShape.FirstOrDefault(); i++)
        {
            sensorInputs.Add(0);
        }

    }

    public void ResetWithNetwork(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {
        overallFitness = 0f;

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }

    private void FixedUpdate()
    {
        InputSensors();
        
        lastPosition = transform.position;

        var res = network.RunNetwork(sensorInputs);

        accelerate = res.FirstOrDefault();
        turn = res.LastOrDefault();
        MoveCar(accelerate, turn);

        CalculateFitness();

        timeSinceStart += Time.deltaTime;
    }

    private void Death()
    {
        GameObject.FindObjectOfType<GeneticManager>()
            .Death(overallFitness, network);
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler)
            + (avgSpeed * avgSpeedMultiplier)
            + (sensorInputs.Average() * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
        }

        if (overallFitness >= 1000)
        {
            Death();
        }

    }

    private void InputSensors()
    {
        // init lasers
        var laserCount = layerShape[0];
        var anglePiece = viewAngle / (laserCount - 1);

        for (int i = 0; i < laserCount; i++)
        {
            float currentDegree = anglePiece * i - viewAngle / 2;
            var direction = transform.forward;
            direction = Quaternion.AngleAxis(currentDegree, Vector3.up) * direction;

            RaycastHit hit;
            Ray ray = new Ray(transform.position, direction);
            Vector3 finalPoint = transform.position + direction * sensorDist;

            if (Physics.Raycast(ray, out hit, sensorDist))
            {
                sensorInputs[i] = hit.distance / sensorDist;
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
            else
            {
                sensorInputs[i] = 1;
                Debug.DrawLine(ray.origin, finalPoint, Color.green);
            }

        }
    }

    private Vector3 inp;
    public void MoveCar(float acce, float turn)
    {
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, acce * 30f), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;

        transform.eulerAngles += new Vector3(0, (turn * 90) * 0.02f, 0);
    }

}
