using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BenchmarkHelper
{

    public static float AverageValue(float[] t)
    {
        float avg = 0.0f;

        for (int i = 0; i < t.Length; i++)
            avg += (t[i] / t.Length);

        return avg;
    }


    public static float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }


    public static float CalculateCorrectness(Transform original, Transform reference)
    {
        float diff_pos_absolute = Vector3.Distance(reference.position, original.position);  //try distance squared
        float diff_rot = Mathf.Abs(Quaternion.Angle(reference.rotation, original.rotation));

        Debug.Log($"--- diff_pos: {diff_pos_absolute} , diff_rot: {diff_rot} ---");
        return 1.0f / (diff_pos_absolute + diff_rot);
    }

}
