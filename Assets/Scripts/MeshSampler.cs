using System.Collections.Generic;
using System.Linq;
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


public class BoundingBox
{
    public Vector3 Center { get; private set; }
    public Vector3 Size { get; private set; }

    public Vector3 MinCorner { get { return Center - Size / 2; } }


    public BoundingBox(Vector3 center, Vector3 size)
    {
        this.Center = center;
        this.Size = size;
    }

    public Vector3 RandomPoint()
    {
        return new Vector3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z)) + MinCorner;
    }

    public bool Valid()
    {
        float shortestSide = Mathf.Min(Size.x, Size.y, Size.z);
        return Physics.CheckSphere(Center, shortestSide / 2);
    }
}


public class MeshSampler
{
    private MeshRenderer[] meshRenderers;
    public MeshApproximation MeshApproximation { get; private set; }


    private List<BoundingBox[]> bounds_stratified = new List<BoundingBox[]>();    //declared globally to be used in debug



    public MeshSampler(MeshRenderer[] meshRenderers, Transform[] linkedTransforms, int[] sampleCount_distribution, int stratifiedDivisions)
    {
        this.meshRenderers = meshRenderers;

        MeshApproximation = new MeshApproximation(sampleCount_distribution);

        SampleMesh(sampleCount_distribution, linkedTransforms, stratifiedDivisions);
    }

    private void SampleMesh(int[] sampleCount_distribution, Transform[] linkedTransforms, int stratifiedDivisions)
    {
        int counter = 0;
        for (int i = 0; i < sampleCount_distribution.Length; i++)
        {
            MeshCollider collider = linkedTransforms[i].GetComponent<MeshCollider>();
            if (collider) collider.convex = false;

            BoundingBox bounds = new BoundingBox(meshRenderers[i].bounds.center, meshRenderers[i].bounds.size);
            BoundingBox[] bounds_stratified = GenerateStratifiedBounds(bounds, stratifiedDivisions);

            bounds_stratified = bounds_stratified.OrderBy(x => Random.value).ToArray();
            this.bounds_stratified.Add(bounds_stratified);//for debugging

            int loopCap = 100 * sampleCount_distribution[i];
            int j = 0;
            while (j < sampleCount_distribution[i] && --loopCap > 0)
            {
                Vector3 sample_pos = bounds_stratified[j % bounds_stratified.Length].RandomPoint();

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


    private BoundingBox[] GenerateStratifiedBounds(BoundingBox boundingBox_original, int divisions)
    {
        List<BoundingBox> boundingBoxes = new List<BoundingBox>((int)Mathf.Pow((divisions + 1), 3.0f)); //(divisions + 1)^3  gives the max number of new bounding boxes

        Vector3 newBoundsSize = boundingBox_original.Size / (divisions + 1);
        for (int x = 0; x <= divisions; x++)
        {
            for (int y = 0; y <= divisions; y++)
            {
                for (int z = 0; z <= divisions; z++)
                {
                    Vector3 newBoundsCenter = new Vector3(x * newBoundsSize.x, y * newBoundsSize.y, z * newBoundsSize.z)
                        + (newBoundsSize / 2)
                        + boundingBox_original.MinCorner;
                    BoundingBox b = new BoundingBox(newBoundsCenter, newBoundsSize);

                    if (b.Valid()) boundingBoxes.Add(b);
                }
            }
        }

        return boundingBoxes.ToArray();
    }


    public void Update()
    {
        MeshApproximation.Update();
    }

    public void DebugDraw()
    {
        Gizmos.color = Color.white;
        foreach (SamplePoint sp in MeshApproximation.Samples)
        {
            Gizmos.DrawWireSphere(sp.GlobalPosition, Gizmos.probeSize);
        }

        Gizmos.color = Color.green;
        foreach (BoundingBox[] b_arr in bounds_stratified)
        {
            foreach (BoundingBox b in b_arr)
            {
                Gizmos.DrawWireCube(b.Center, b.Size);
            }
        }
    }

}
