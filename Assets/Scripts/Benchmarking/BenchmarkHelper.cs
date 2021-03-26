using UnityEngine;

public static class BenchmarkHelper
{

    public static T AverageValue<T>(T[] t)
    {
        dynamic avg = 0;

        if (t == null || t.Length < 0)
            return (dynamic)0.0f;

        for (int i = 0; i < t.Length; i++)
            avg += (dynamic)t[i] / t.Length;

        return avg;
    }


    public static float GetComputeTimeMS()
    {
        return Time.deltaTime * 1000;
    }
    public static float GetFPS()
    {
        return 1.0f / Time.deltaTime;
    }

    public static long GetMemoryUsage()
    {
        return System.GC.GetTotalMemory(false); //Returns the memory usage in bytes
    }


    public static float CalculateCorrectness(Transform original, Transform reference)
    {
        float diff_pos_absolute = Vector3.Distance(reference.position, original.position);
        float diff_rot = Mathf.Abs(Quaternion.Angle(reference.rotation, original.rotation));

        return -(diff_pos_absolute + diff_rot);
    }

}
