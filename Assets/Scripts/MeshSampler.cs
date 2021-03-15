using UnityEngine;


public class SamplePoint
{
    public SamplePoint(Vector3 localPosition, Quaternion localRotation)
    {
        this.localPosition = localPosition;
        this.localRotation = localRotation;
    }

    private Vector3 localPosition;
    private Quaternion localRotation;

    public Vector3 GlobalPosition { get; private set; }
    public Vector3? LastPosition { get; set; }


    public void SetPosition(Transform linkedTransform)
    {
        Matrix4x4 m = Matrix4x4.Rotate(linkedTransform.rotation * Quaternion.Inverse(localRotation));
        Vector3 rotatedOffset = m.MultiplyPoint3x4(localPosition);
        this.GlobalPosition = linkedTransform.position + rotatedOffset;
    }
}



public class MeshSampler
{
    private MeshRenderer meshRenderer;
    Transform transform;

    private Vector3 boundsPos;
    private Vector3 boundsSize;


    public MeshApproximation MeshApproximation { get; private set; }



    public MeshSampler(MeshRenderer meshRenderer, Transform linkedTransform, int sampleCount)
    {
        this.meshRenderer = meshRenderer;
        this.transform = linkedTransform;

        MeshApproximation = new MeshApproximation(linkedTransform, sampleCount);

        SampleMesh(sampleCount);
    }

    private void SampleMesh(int sampleCount)
    {
        boundsPos = meshRenderer.bounds.center;
        boundsSize = new Vector3(meshRenderer.bounds.size.x, meshRenderer.bounds.size.y, meshRenderer.bounds.size.z);

        int loopCap = 10000;
        while (MeshApproximation.Samples.Count < sampleCount && --loopCap > 0)
        {
            Vector3 sample_pos = new Vector3(Random.Range(0, boundsSize.x), Random.Range(0, boundsSize.y), Random.Range(0, boundsSize.z)) + (boundsPos - boundsSize / 2);

            if (ValidateSample(sample_pos))
            {
                SamplePoint sample = new SamplePoint(sample_pos - transform.position, transform.rotation);
                MeshApproximation.Samples.Add(sample);
            }
        }
    }

    private bool ValidateSample(Vector3 sample_pos)
    {
        return Physics.CheckSphere(sample_pos, 0.01f);
    }


    public void Update()
    {
        MeshApproximation.Update();
    }

    public void DebugDraw()
    {
        foreach (SamplePoint sp in MeshApproximation.Samples)
        {
            Gizmos.DrawWireSphere(sp.GlobalPosition, 0.1f);
        }

        //Gizmos.DrawWireCube(boundsPos, boundsSize);
    }

}
