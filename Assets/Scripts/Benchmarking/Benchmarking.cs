using UnityEngine;

public class Benchmarking : MonoBehaviour
{
    private int testLength = 1000;      //numbers of frames to test
    bool running = false;

    private int frameCounter = 0;


    private float[] fps;



    private void Start()
    {
        fps = new float[testLength];

        Debug.Log($"BENCHMARK START - test length: {testLength} frames");
        running = true;
    }


    private void Update()
    {
        if (frameCounter < testLength)
        {
            //info about test scenario (#samples, #divisions, wave properties)

            fps[frameCounter] = GetFPS();
            //memory
            //correctness

            frameCounter++;

            if (frameCounter % 100 == 0)
            {
            Debug.Log($"Benchmarking:  {frameCounter} / {testLength}  complete");
            }

        }
        else if (running)
        {
            StopBenchmark();
        }
    }

    private void StopBenchmark()
    {
        running = false;
        Debug.Log("---BENCHMARK END---");
        Debug.Log("- #Frames tested:  " + frameCounter);
        Debug.Log("- Average FPS:  " + GetAverageFPS());
        Debug.Log("-------------------");
    }

    private void OnApplicationQuit()
    {
        StopBenchmark();
    }



    private float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }
    private float GetAverageFPS()
    {
        float averageFps = 0.0f;

        for (int i = 0; i < fps.Length; i++)
            averageFps += (fps[i] / frameCounter);

        return averageFps;
    }

}
