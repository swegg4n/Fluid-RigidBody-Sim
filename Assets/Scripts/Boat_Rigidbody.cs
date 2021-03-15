using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
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
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Rigidbody rb = GetComponent<Rigidbody>();
        float meshVolume = MeshVolume.VolumeOfMesh(mesh, transform);

        meshSampler = new MeshSampler(meshRenderer, transform, sampleCount);
        gravity = new Gravity(rb, meshSampler);
        buoyancy = new Buoyancy(rb, meshSampler, meshVolume);
        waterDrag = new WaterDrag(rb, meshSampler, viscosity);

        //rb.AddForce(1000, 50, 0, ForceMode.Acceleration);
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
