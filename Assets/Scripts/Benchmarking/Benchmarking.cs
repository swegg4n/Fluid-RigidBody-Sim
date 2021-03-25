using System.Collections;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    [SerializeField] private TestCase[] testCases;
    [SerializeField] private GameObject waterInstance;

    private bool testComplete = false;



    private void Awake()
    {
        benchmarkPath = Application.dataPath + "/Test_Results/";
#if !UNITY_EDITOR
        if (!Directory.Exists(benchmarkPath))   //Creates the Test_Results folder for builds
            Directory.CreateDirectory(benchmarkPath);
#endif
        /*Change settings to fit benchmark test (eg. remove water, remove self collisions(?), change render settings(?))*/
        waterInstance.GetComponent<MeshRenderer>().enabled = false;
    }


    private void Start()
    {
        StartCoroutine(RunAllBenchmarks());
    }



    private IEnumerator RunAllBenchmarks()
    {
        for (int i = 0; i < testCases.Length; i++)
        {
            testComplete = false;
            StartCoroutine(RunBenchmark(testCases[i]));

            while (testComplete == false) { yield return new WaitForSeconds(1.0f); }  //spin wait
        }

        Debug.Log("End of test, closing...");
        yield return new WaitForSeconds(2.0f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }



    private IEnumerator RunBenchmark(TestCase testCase)
    {
        /*Set the wave manager settings according to the test case*/
        this.waterInstance.GetComponent<WaveManager>().Set(testCase.amplitude, testCase.ordinaryFrequency, testCase.angluarFrequency);

        /*Instantiate a new boat to test with*/
        GameObject boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        boatInstance.GetComponent<BoatRigidbody>().Set(testCase.sampleCount, testCase.stratifiedDivisions, testCase.density, testCase.viscosity);

        string testName = testCase.name;
#if UNITY_EDITOR
        testName += "_EDITOR"; //Prefixes the file name so we can differentiate between the tests
#else
        testName += "_BUILD";
#endif

        yield return new WaitForSeconds(Time.deltaTime);    //Delay to not have instantiation manipulate test results. (instantiate is computationally heavy)

        using (StreamWriter writer = new StreamWriter(benchmarkPath + testName + ".txt"))
        {
            float[] fps = new float[testCase.testLength];

            Debug.Log($"Running benchmark: {testName}");

            int framesCounter = 0;
            while (framesCounter < testCase.testLength)
            {
                if (framesCounter % 100 == 0)
                    Debug.Log($"{testName}:  {(framesCounter * 100) / testCase.testLength}%");

                /*Save relevant data per frame*/
                fps[framesCounter] = GetFPS();


                yield return new WaitForSeconds(Time.deltaTime);
                ++framesCounter;
            }

            /*Debug test results*/
            Debug.Log($"Benchmark \"{testName}\" - Completed");
            Debug.Log($"Test length:  {testCase.testLength} frames");
            Debug.Log($"Average FPS:  {GetAverageFPS(fps, testCase.testLength)}");

            /*Compile test results to file*/
            writer.WriteLine($"{testName}");
            writer.WriteLine($"{testCase.testLength}");
            writer.WriteLine($"{GetAverageFPS(fps, testCase.testLength)}");
        }

        Destroy(boatInstance);

        testComplete = true;
    }



    private float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }
    private float GetAverageFPS(float[] fps, float testLength)
    {
        float averageFps = 0.0f;

        for (int i = 0; i < fps.Length; i++)
            averageFps += (fps[i] / testLength);

        return averageFps;
    }

}
