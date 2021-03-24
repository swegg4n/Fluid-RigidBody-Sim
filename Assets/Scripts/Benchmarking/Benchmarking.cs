using System.Collections;
using System.IO;
using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private string benchmarkPath;

    [SerializeField] private MeshRenderer waterRenderer;


    private void Awake()
    {
        benchmarkPath = Application.dataPath + "/Benchmark/";

        /*Change settings to fit benchmark test (eg. remove water, remove self collisions(?), change render settings(?))*/
        waterRenderer.enabled = false;
    }


    private void Start()
    {
        StartCoroutine(RunBenchmark("Fps_test_01", 1000));
    }

    private IEnumerator RunBenchmark(string testName, int testLength)
    {
        using (StreamWriter writer = new StreamWriter(benchmarkPath + testName + ".txt"))
        {
            float[] fps = new float[testLength];


            Debug.Log($"Running benchmark: {testName}");


            int framesCounter = 0;
            while (framesCounter < testLength)
            {
                if (framesCounter % 100 == 0)
                    Debug.Log($"{testName}:  {(framesCounter * 100) / testLength}%");

                /*Save relevant data per frame*/
                fps[framesCounter] = GetFPS();


                yield return new WaitForSeconds(Time.deltaTime);
                ++framesCounter;
            }

            /*Debug test results*/
            Debug.Log($"Benchmark \"{testName}\" - Completed");
            Debug.Log($"Test length:  {testLength} frames");
            Debug.Log($"Average FPS:  {GetAverageFPS(fps, testLength)}");

            /*Compile test results to file*/
            writer.WriteLine($"{testName}");
            writer.WriteLine($"{testLength}");
            writer.WriteLine($"{GetAverageFPS(fps, testLength)}");
        }
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
