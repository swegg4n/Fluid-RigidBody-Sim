using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshApproximation
{
    private Transform linkedTransform;

    public List<SamplePoint> Samples { get; private set; }
    public List<SamplePoint> UnderWaterSamples { get; private set; }

    public int SampleCount { get; private set; }


    public MeshApproximation(Transform linkedTransform, int sampleCount)
    {
        this.linkedTransform = linkedTransform;

        this.Samples = new List<SamplePoint>();
        this.UnderWaterSamples = new List<SamplePoint>();

        this.SampleCount = sampleCount;
    }

    public void Update()
    {
        UpdateSamplesPosition();
        UpdateUnderWaterSamples();
    }

    private void UpdateSamplesPosition()
    {
        foreach (SamplePoint sp in Samples)
        {
            sp.SetPosition(linkedTransform);
        }
    }
    private void UpdateUnderWaterSamples()
    {
        UnderWaterSamples.Clear();
        foreach (SamplePoint sp in Samples)
        {
            if (sp.GlobalPosition.y <= WaveManager.instance.GetWaveHeight(sp.GlobalPosition))
            {
                UnderWaterSamples.Add(sp);
            }
        }
    }


    public Vector3 AverageSamplePosition()
    {
        Vector3 avg = Vector3.zero;

        foreach (SamplePoint sp in Samples)
            avg += sp.GlobalPosition / SampleCount;

        return avg;
    }
    public Vector3 AverageSamplePosition(ICollection samplePoints)
    {
        Vector3 avg = Vector3.zero;

        foreach (SamplePoint sp in samplePoints)
            avg += sp.GlobalPosition / samplePoints.Count;

        return avg;
    }

}
