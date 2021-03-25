using System.Collections;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    [SerializeField] private TestCase[] testCases;
    [SerializeField] private GameObject waterInstance;

    private bool testComplete = false;

    TestResult testResult;


    private abstract class TestResult
    {
        public TestResult(TestCase testCase)
        {
            this.testLength = testCase.testLength;
            this.typeOfTest = testCase.typeOfTest.ToString();
            this.prefabName = testCase.prefab.name;
            this.sampleCount = testCase.sampleCount;
            this.stratifiedDivisions = testCase.stratifiedDivisions;
            this.amplitude = testCase.amplitude;
            this.ordinaryFrequency = testCase.ordinaryFrequency;
            this.angluarFrequency = testCase.angluarFrequency;
        }

        public int testLength;
        public string typeOfTest;
        public string prefabName;

        public int sampleCount;
        public int stratifiedDivisions;
        //public int density;
        //public int viscosity;

        public float amplitude;
        public float ordinaryFrequency;
        public float angluarFrequency;
    }


    private class PerformanceTestResult : TestResult
    {
        public PerformanceTestResult(TestCase testCase) : base(testCase)
        {
            this.fps = new float[testCase.testLength];
        }

        public float[] fps;
        //memory
        //...
    }

    private class CorrectnessTestResult : TestResult
    {
        public CorrectnessTestResult(TestCase testCase) : base(testCase)
        {
            this.correctness = new float[testCase.testLength];
        }

        public float[] correctness;
        //...
    }



    private void Awake()
    {
        benchmarkPath = Application.dataPath + "/Test_Results/";
#if !UNITY_EDITOR
        if (!Directory.Exists(benchmarkPath))   //Creates the Test_Results folder for builds
            Directory.CreateDirectory(benchmarkPath);
#endif
        /*Change settings to fit benchmark test (eg. remove water, change render settings(?))*/
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
        switch (testCase.typeOfTest)
        {
            case TypeOfTest.Performance:
                testResult = new PerformanceTestResult(testCase);
                break;

            case TypeOfTest.Correctness:
                testResult = new CorrectnessTestResult(testCase);
                break;
        }

        /*Set the wave manager settings according to the test case*/
        this.waterInstance.GetComponent<WaveManager>().Set(testCase.amplitude, testCase.ordinaryFrequency, testCase.angluarFrequency);

        /*Instantiate a new boat to test with*/
        GameObject boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        boatInstance.GetComponent<BoatRigidbody>().Set(testCase.sampleCount, testCase.stratifiedDivisions, testCase.density, testCase.viscosity);

        GameObject referenceBoatInstance;
        if (testCase.typeOfTest == TypeOfTest.Correctness)  //If we aim to test correctness => instantiate one more boat with high sample count, to test against.
        {
            referenceBoatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
            boatInstance.GetComponent<BoatRigidbody>().Set(10000, testCase.stratifiedDivisions, testCase.density, testCase.viscosity);
            boatInstance.layer = 6;     //Set layer to "Reference", non-colliding layer
        }


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
                    Debug.Log($"{testName}:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

                SaveTestData(framesCounter, testCase.typeOfTest);   // Save data for this frame, based on which type of test we are running

                yield return new WaitForSeconds(Time.deltaTime);    // Wait for next frame
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



    private void SaveTestData(int frame, TypeOfTest typeOfTest)
    {
        //fps[framesCounter] = GetFPS();
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
