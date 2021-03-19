using UnityEngine;


public class SamplePoint
{
    public SamplePoint(Vector3 localPosition, Quaternion localRotation, Transform linkedTransform)
    {
        this.localPosition = localPosition;
        this.localRotation = localRotation;
        this.linkedTransform = linkedTransform;
    }

    private Vector3 localPosition;
    private Quaternion localRotation;

    public Vector3 GlobalPosition { get; private set; }
    public Vector3? LastPosition { get; set; }

    private Transform linkedTransform;


    public void SetPosition()
    {
        Matrix4x4 m = Matrix4x4.Rotate(linkedTransform.rotation * Quaternion.Inverse(localRotation));
        Vector3 rotatedOffset = m.MultiplyPoint3x4(localPosition);
        this.GlobalPosition = linkedTransform.position + rotatedOffset;
    }
}



public class MeshSampler
{
    private MeshRenderer[] meshRenderers;

    private Vector3 boundsPos;
    private Vector3 boundsSize;


    public MeshApproximation MeshApproximation { get; private set; }



    public MeshSampler(MeshRenderer[] meshRenderers, Transform[] linkedTransforms, int[] sampleCount_distribution)
    {
        this.meshRenderers = meshRenderers;

        MeshApproximation = new MeshApproximation(sampleCount_distribution);

        SampleMesh(sampleCount_distribution, linkedTransforms);
    }

    private void SampleMesh(int[] sampleCount_distribution, Transform[] linkedTransforms)
    {
        int counter = 0;
        for (int i = 0; i < sampleCount_distribution.Length; i++)
        {
            boundsPos = meshRenderers[i].bounds.center;
            boundsSize = new Vector3(meshRenderers[i].bounds.size.x, meshRenderers[i].bounds.size.y, meshRenderers[i].bounds.size.z);

            MeshCollider collider = linkedTransforms[i].GetComponent<MeshCollider>();
            if (collider) collider.convex = false;

            int loopCap = 1000000;
            int j = 0;
            while (j < sampleCount_distribution[i] && --loopCap > 0)
            {
                Vector3 sample_pos = new Vector3(Random.Range(0, boundsSize.x), Random.Range(0, boundsSize.y), Random.Range(0, boundsSize.z)) + (boundsPos - boundsSize / 2);

                if (ValidateSample(sample_pos))
                {
                    SamplePoint sample = new SamplePoint(sample_pos - linkedTransforms[i].position, linkedTransforms[i].rotation, linkedTransforms[i]);
                    MeshApproximation.Samples[counter++] = sample;
                    ++j;
                }
            }
            if (j < sampleCount_distribution[i])
            {
                throw new System.Exception("Failed to place all sample points");
            }

            if (collider) collider.convex = true;
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
