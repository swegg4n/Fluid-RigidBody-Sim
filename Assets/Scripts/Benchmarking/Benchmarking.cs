using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    private const int REFERENCE_BOAT_SAMPLES = 20000;

    [SerializeField] private TestCase[] testCases;
    [SerializeField] private GameObject waterInstance;

    private static GameObject boatInstance = null;
    private static GameObject referenceBoatInstance = null;

    private bool testComplete = false;

    private TestResult testResult;


    private abstract class TestResult
    {
        public TestResult(TestCase testCase, int samples)
        {
            this.testLength = testCase.testLength;
            this.typeOfTest = testCase.typeOfTest.ToString();
            this.prefabName = testCase.prefab.name;

            this.sampleCount = samples;
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


        public virtual List<string> Header()
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

        public abstract string Data();
    };


    private class PerformanceTestResult : TestResult
    {
        public PerformanceTestResult(TestCase testCase, int samples) : base(testCase, samples)
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

        public override List<string> Header()
        {
            List<string> result = base.Header();

            result.Add("\nAvg. FPS\tAvg. Memory usage (bytes)");

            return result;
        }

        public override string Data()
        {
            return BenchmarkHelper.AverageValue(fps).ToString() + "\t" + BenchmarkHelper.AverageValue(memoryUsage).ToString();
        }
    };

    private class CorrectnessTestResult : TestResult
    {
        public CorrectnessTestResult(TestCase testCase, int samples) : base(testCase, samples)
        {
            this.correctness_pos = new float[testCase.testLength];
            this.correctness_rot = new float[testCase.testLength];
        }

        public float[] correctness_pos;
        public float[] correctness_rot;

        public override void SaveFrame(int frame)
        {
            this.correctness_pos[frame] = BenchmarkHelper.PositionCorrectness(Benchmarking.boatInstance.transform, Benchmarking.referenceBoatInstance.transform);
            this.correctness_rot[frame] = BenchmarkHelper.RotationCorrectness(Benchmarking.boatInstance.transform, Benchmarking.referenceBoatInstance.transform);
        }

        public override List<string> Header()
        {
            List<string> result = base.Header();

            result.Add("Positional correctness\tAngular correctness");

            return result;
        }

        public override string Data()
        {
            return BenchmarkHelper.AverageValue(correctness_pos).ToString() + "\t" + BenchmarkHelper.AverageValue(correctness_rot).ToString();
        }
    };



    private void Awake()
    {
        benchmarkPath = Application.dataPath + "/Test_Results/";
#if !UNITY_EDITOR       //Creates the Test_Results folder for builds
        if (!Directory.Exists(benchmarkPath))   
            Directory.CreateDirectory(benchmarkPath);
#endif
        /*Change settings to fit benchmark test (eg. remove water, change render settings(?))*/
        waterInstance.GetComponent<MeshRenderer>().enabled = false;
    }


    private void Start()
    {
#if !UNITY_EDITOR
        StartCoroutine(RunAllBenchmarks());
#endif
    }


    private IEnumerator RunAllBenchmarks()
    {
        for (int i = 0; i < testCases.Length; i++)
        {
            string testName = testCases[i].name;
#if UNITY_EDITOR
            testName += "-EDITOR"; //Prefixes the file name so we can differentiate between the tests
#else
        testName += "-BUILD";
#endif
            switch (testCases[i].typeOfTest)
            {
                case TypeOfTest.Performance:
                    testResult = new PerformanceTestResult(testCases[i], testCases[i].sampleCounts.Length);
                    break;

                case TypeOfTest.Correctness:
                    testResult = new CorrectnessTestResult(testCases[i], testCases[i].sampleCounts.Length);
                    break;
            }

            string filePath = benchmarkPath + testName + ".txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int j = 0; j < testResult.Header().Count; j++)
                {
                    writer.WriteLine(testResult.Header()[j]);
                }
            }


            for (int j = 0; j < testCases[i].sampleCounts.Length; j++)
            {
                testComplete = false;
                StartCoroutine(RunBenchmark(testCases[i], testCases[i].sampleCounts[j], filePath));

                while (testComplete == false) { yield return new WaitForSeconds(1.0f); }  //spin wait
            }
        }

        Debug.Log("End of test, closing...");
        yield return new WaitForSeconds(2.0f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }



    private IEnumerator RunBenchmark(TestCase testCase, int samples, string filePath)
    {

        /*Set the wave manager settings according to the test case*/
        this.waterInstance.GetComponent<WaveManager>().Set(testCase.amplitude, testCase.ordinaryFrequency, testCase.angluarFrequency);


        /*Instantiate a new boat to test with*/
        boatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
        boatInstance.GetComponent<BoatRigidbody>().Set(samples/*, testCase.stratifiedDivisions*/, testCase.density, testCase.viscosity);

        if (testCase.typeOfTest == TypeOfTest.Correctness)  //If we aim to test correctness => instantiate one more boat with high sample count, to test against.
        {
            referenceBoatInstance = Instantiate(testCase.prefab, testCase.position, Quaternion.identity);
            referenceBoatInstance.GetComponent<BoatRigidbody>().Set(REFERENCE_BOAT_SAMPLES/*, testCase.stratifiedDivisions*/, testCase.density, testCase.viscosity);

            referenceBoatInstance.layer = 6;     //Set layer to "Reference", non-colliding layer
            for (int i = 0; i < referenceBoatInstance.transform.childCount; i++)
                referenceBoatInstance.transform.GetChild(i).gameObject.layer = 6;
        }


        yield return new WaitForSeconds(Time.deltaTime);    //Delay to not have instantiation manipulate test results. (instantiate is computationally heavy)


        using (StreamWriter writer = File.AppendText(filePath))
        {
            Debug.Log($"Running benchmark: {testCase.name}_s{samples}");

            int framesCounter = 0;
            while (framesCounter < testCase.testLength)
            {
                if (framesCounter % 10 == 0)
                    Debug.Log($"Progress:  {(framesCounter * 100) / testCase.testLength}%");  //DEBUG progress

                testResult.SaveFrame(framesCounter);    //Update the test result values with data from this frame

                yield return new WaitForSeconds(Time.deltaTime);    // Wait for next frame
                ++framesCounter;
            }

#region Write/Log results
            Debug.Log($"Benchmark \"{testCase.name}_s{samples}\" - Completed");

            writer.WriteLine(testResult.Data());
            Debug.Log(testResult.Data());
#endregion
        }

        Destroy(boatInstance);
        Destroy(referenceBoatInstance);

        testComplete = true;
    }


};
