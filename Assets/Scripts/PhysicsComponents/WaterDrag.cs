using UnityEngine;

public class WaterDrag : IPhysicsComponent
{
    private Rigidbody rb;
    private MeshSampler ms;
    private float viscosity;

    private int sampleCount;


    public WaterDrag(Rigidbody rb, MeshSampler ms, float viscosity)
    {
        this.rb = rb;
        this.ms = ms;
        this.viscosity = viscosity;
    }


    public void Update()
    {
        foreach (SamplePoint sp in ms.MeshApproximation.Samples)
        {
            if (ms.MeshApproximation.UnderWaterSamples.Contains(sp))
            {
                if (sp.LastPosition != null)
                {
                    Vector3 deltaVelocity = sp.GlobalPosition - (Vector3)sp.LastPosition;
                    rb.AddForceAtPosition(-deltaVelocity * viscosity / ms.MeshApproximation.SampleCount, sp.GlobalPosition, ForceMode.VelocityChange);
                }

                sp.LastPosition = sp.GlobalPosition;
            }
            else
            {
                sp.LastPosition = null;
            }
        }
    }

    public void DebugDraw()
    {

    }

}
