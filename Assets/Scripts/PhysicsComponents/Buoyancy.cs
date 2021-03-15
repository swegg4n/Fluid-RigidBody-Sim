using System.Collections.Generic;
using UnityEngine;
using static Density_enum;


public class Buoyancy : IPhysicsComponent
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float meshVolume;



    public Buoyancy(Rigidbody rb, MeshSampler ms, float meshVolume)
    {
        this.rb = rb;
        this.ms = ms;
        this.meshVolume = meshVolume;
    }


    public void Update()
    {
        List<SamplePoint> underWaterSamples = ms.MeshApproximation.UnderWaterSamples;

        if (underWaterSamples.Count > 0)
        {
            Vector3 avgUnderWaterCenter = ms.MeshApproximation.AverageSamplePosition(underWaterSamples);
            float underWaterRatio = (float)underWaterSamples.Count / ms.MeshApproximation.SampleCount;
            float approxUnderwaterVolume = meshVolume * underWaterRatio;

            Vector3 buoyantForce = -(int)Material_Density.Water * Physics.gravity * approxUnderwaterVolume;
            rb.AddForceAtPosition(buoyantForce, avgUnderWaterCenter);
        }
    }


    public void DebugDraw()
    {
        Gizmos.color = Color.blue;
        foreach (var sample in ms.MeshApproximation.UnderWaterSamples)
        {
            Gizmos.DrawSphere(sample.GlobalPosition, 0.1f);
        }
    }
}
