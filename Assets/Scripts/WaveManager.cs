using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    private MeshFilter meshFilter;

    [SerializeField] private float amplitude = 0.25f;
    [SerializeField] private float ordinaryFrequency = 1.0f;
    [SerializeField] private float angluarFrequency = 2.0f;
    private float phase = 0.0f;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        meshFilter = GetComponent<MeshFilter>();
    }


    private void Update()
    {
        phase += angluarFrequency * Time.deltaTime;


        Vector3[] vertices = meshFilter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = GetWaveHeight(transform.position + vertices[i]);
        }
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();
    }


    public float GetWaveHeight(Vector3 point)
    {
        return amplitude / transform.localScale.x / transform.localScale.y * Mathf.Sin(point.x / ordinaryFrequency / transform.localScale.x + phase);
    }
}
