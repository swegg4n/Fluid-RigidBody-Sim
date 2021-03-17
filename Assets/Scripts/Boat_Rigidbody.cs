using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Boat_Rigidbody : MonoBehaviour
{
    [SerializeField] private int sampleCount = 100;
    [SerializeField] private float viscosity = 1.0f;

    [SerializeField] private bool debug = true;


    MeshSampler meshSampler;
    Gravity gravity;
    Buoyancy buoyancy;
    WaterDrag waterDrag;


    private void Awake()
    {
        List<Mesh> meshList = new List<Mesh>();
        List<MeshRenderer> meshRendererList = new List<MeshRenderer>();
        List<Transform> transformList = new List<Transform>();

        if (GetComponent<MeshCollider>() != null)
        {
            meshList.Add(GetComponent<MeshFilter>().sharedMesh);
            meshRendererList.Add(GetComponent<MeshRenderer>());
            transformList.Add(transform);
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<MeshCollider>() != null)
            {
                Transform child = transform.GetChild(i);

                meshList.Add(child.GetComponent<MeshFilter>().sharedMesh);
                meshRendererList.Add(child.GetComponent<MeshRenderer>());
                transformList.Add(child);
            }
        }

        Mesh[] meshes = meshList.ToArray();
        MeshRenderer[] meshRenderers = meshRendererList.ToArray();
        Transform[] transforms = transformList.ToArray();

        Rigidbody rb = GetComponent<Rigidbody>();

        float[] meshVolumes = new float[meshes.Length];
        for (int i = 0; i < meshes.Length; i++)
            meshVolumes[i] = MeshVolume.VolumeOfMesh(meshes[i], transforms[i]);

        float totalMeshVolume = meshVolumes.Sum();

        meshSampler = new MeshSampler(meshRenderers, transforms, DistributeSamples(meshVolumes, totalMeshVolume));
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, totalMeshVolume);
        waterDrag = new WaterDrag(rb, meshSampler, viscosity);

        //rb.AddForce(1000, 50, 0, ForceMode.Acceleration);
    }

    private int[] DistributeSamples(float[] meshVolumes, float totalVolume)
    {
        int[] distribution = new int[meshVolumes.Length];
        int totalDistributions = 0;

        for (int i = 0; i < distribution.Length - 1; i++)
        {
            int d = (int)(meshVolumes[i] / totalVolume * sampleCount);
            distribution[i] = d;
            totalDistributions += d;
        }
        distribution[meshVolumes.Length - 1] = sampleCount - totalDistributions;

        return distribution;
    }


    private void FixedUpdate()
    {
        meshSampler.Update();
        gravity.Update();
        buoyancy.Update();
        waterDrag.Update();
    }


    private void OnDrawGizmos()
    {
        if (debug)
        {
            try
            {
                meshSampler.DebugDraw();
                gravity.DebugDraw();
                buoyancy.DebugDraw();
                waterDrag.DebugDraw();
            }
            catch (System.Exception) { }
        }
    }
}
