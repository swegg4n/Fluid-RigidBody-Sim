using UnityEngine;


[CreateAssetMenu(fileName = "TestCase", menuName = "Benchmark/TestCase", order = 1)]
public class TestCase : ScriptableObject
{
    public int testLength = 1000;  //number of frames to test

    public GameObject prefab;
    public Vector3 position = new Vector3(0, 0, 0);
    public int sampleCount = 100;
    public int stratifiedDivisions = 0;
    public float density = 700.0f;
    public float viscosity = 1.0f;

    public float amplitude = 0.0f;
    public float ordinaryFrequency = 1.5f;
    public float angluarFrequency = 1.0f;
}
