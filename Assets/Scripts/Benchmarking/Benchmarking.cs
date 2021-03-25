using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    private const int REFERENCE_BOAT_SAMPLES = 10000;

    [SerializeField] private TestCase[] testCases;
    [SerializeField] private GameObject waterInstance;

    private static GameObject boatInstance = null;
    private static GameObject referenceBoatInstance = null;

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
            this.density = testCase.density;
            this.viscosity = testCase.viscosity;

            this.amplitude = testCase.amplitude;
            this.ordinaryFrequency = testCase.ordinaryFrequency;
            this.angluarFrequency = testCase.angluarFrequency;
        }

        public int testLength;
        public string typeOfTest;
        public string prefabName;

        public int sampleCount;
        public int stratifiedDivisions;
        public float density;
        public float viscosity;

        public float amplitude;
        public float ordinaryFrequency;
        public float angluarFrequency;


        public abstract void SaveFrame(int frame);


        public virtual new List<string> ToString()
        {
            List<string> result = new List<string>()
            {
                "Test length:  " + testLength.ToString(),
                "Type of test:  " + typeOfTest,
                "CPU: " + SystemInfo.processorType + " (" + SystemInfo.processorFrequency + ")",
                "",
                "Prefab name:  " + prefabName,
                "Sample count:  " + sampleCount,
                "Stratified divisions:  " + stratifiedDivisions,
                "",
                "Amplitude:  " + amplitude,
                "Ordinary frequency  " + ordinaryFrequency,
                "Angular frequency  " + angluarFrequency,
                "",
            };
            return result;
        }
    };


    private class PerformanceTestResult : TestResult
    {
        public PerformanceTestResult(TestCase testCase) : base(testCase)
        {
            this.fps = new float[testCase.testLength];
            this.memoryUsage = new long[testCase.testLength];
        }

        public float[] fps;
        public long[] memoryUsage;


        public override void SaveFrame(int frame)
        {
            this.fps[frame] = BenchmarkHelper.GetFPS();
            this.memoryUsage[frame] = BenchmarkHelper.GetMemoryUsage();
        }

        public override List<string> ToString()
        {
            List<string> result = base.ToString();

            result.Add("Avg. FPS:  " + BenchmarkHelper.AverageValue(fps).ToString());
            result.Add("Avg. Memory usage:  " + BenchmarkHelper.AverageValue(memoryUsage).ToString() + " bytes");

            return result;
        }
    };

    private class CorrectnessTestResult : TestResult
    {
        public CorrectnessTestResult(TestCase testCase) : base(testCase)
        {
            this.correctness = new float[testCase.testLength];
        }

        public float[] correctness;


        public override void SaveFrame(int frame)
        {
            this.correctness[frame] = BenchmarkHelper.CalculateCorrectness(Benchmarking.boatInstance.transform, Benchmarking.referenceBoatInstance.transform);
        }

        public override List<string> ToString()
        {
            List<string> result = base.ToString();

            result.Add("Avg. correctness:  " + BenchmarkHelper.AverageValue(correctness).ToString());

            return result;
        }
    };



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
        boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        boatInstance.GetComponent<BoatRigidbody>().Set(testCase.sampleCount, testCase.stratifiedDivisions, testCase.density, testCase.viscosity);

        if (testCase.typeOfTest == TypeOfTest.Correctness)  //If we aim to test correctness => instantiate one more boat with high sample count, to test against.
        {
            referenceBoatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
            referenceBoatInstance.GetComponent<BoatRigidbody>().Set(REFERENCE_BOAT_SAMPLES, testCase.stratifiedDivisions, testCase.density, testCase.viscosity);

            referenceBoatInstance.layer = 6;     //Set layer to "Reference", non-colliding layer
            for (int i = 0; i < referenceBoatInstance.transform.childCount; i++)
                referenceBoatInstance.transform.GetChild(i).gameObject.layer = 6;
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
            Debug.Log($"Running benchmark: {testName}");

            int framesCounter = 0;
            while (framesCounter < testCase.testLength)
            {
                if (framesCounter % 10 == 0)
                    Debug.Log($"{testName}:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress


                testResult.SaveFrame(framesCounter);    //Update the test result values with data from this frame


                yield return new WaitForSeconds(Time.deltaTime);    // Wait for next frame
                ++framesCounter;
            }


            #region Write/Log results
            Debug.Log($"Benchmark \"{testName}\" - Completed");

            foreach (string result in testResult.ToString())
            {
                writer.WriteLine(result);
                Debug.Log(result);
            }
            #endregion
        }

        Destroy(boatInstance);
        Destroy(referenceBoatInstance);

        testComplete = true;
    }


};
