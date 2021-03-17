using System.Collections.Generic;
using System.Linq;
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
        float underWaterSamples = (float)ms.MeshApproximation.IsUnderWater.Sum();

        if (underWaterSamples > 0)
        {
            float underWaterRatio = underWaterSamples / ms.MeshApproximation.SampleCount;
            float approxUnderwaterVolume = meshVolume * underWaterRatio;

            Vector3 buoyantForce = -(int)Material_Density.Water * Physics.gravity * approxUnderwaterVolume;
            rb.AddForceAtPosition(buoyantForce, ms.MeshApproximation.AverageUnderWaterSamplePosition());
        }
    }


    public void DebugDraw()
    {
        Gizmos.color = Color.blue;
        for (int i = 0; i < ms.MeshApproximation.SampleCount; i++)
        {
            if (ms.MeshApproximation.IsUnderWater[i] == 1)
            {
                Gizmos.DrawSphere(ms.MeshApproximation.Samples[i].GlobalPosition, 0.1f);
            }
        }
    }

}
