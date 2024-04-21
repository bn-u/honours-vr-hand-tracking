using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MeshAdaptor : MonoBehaviour
{
    Mesh originalMesh;
    Mesh clonedMesh;
    MeshFilter meshFilter;

    Vector3[] vertices;
    int[] triangles;
    Vector3 point;

    public bool isReset = false;
    public bool isDebug = false;

    private void Start()
    {
        InitMesh();
    }

    /// <summary>
    /// Initial set up and cloning of mesh for the reset functionality
    /// </summary>
    private void InitMesh()
    {
        //Clones mesh for reset function
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
        clonedMesh = new Mesh();

        clonedMesh.name = "clone";
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        //clonedMesh.normals = originalMesh.normals;
        //clonedMesh.uv = originalMesh.uv;
        meshFilter.mesh = clonedMesh;

        //Collects mesh verticies
        vertices = clonedMesh.vertices;
        triangles = clonedMesh.triangles;
        if (isDebug == true)
        {
            Debug.Log("Cloned mesh");
        }

        for (int i = 0; i < clonedMesh.vertices.Length; i++)
        {
            PointAssignment(i);
        }
    }
    /// <summary>
    /// Creates a game object at point of vertices
    /// </summary>
    private void PointAssignment(int i)
    {
        point = transform.TransformPoint(clonedMesh.vertices[i]);

        GameObject userInteractor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        userInteractor.transform.parent = transform;
        userInteractor.transform.position = point;
        userInteractor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        var cubeRenderer = userInteractor.GetComponent<Renderer>();
        cubeRenderer.material.SetColor("_Color", Color.red);
        cubeRenderer.enabled = false;
        userInteractor.transform.hasChanged = false;

        if (isDebug == true)
        {
            Debug.Log("point: " + point + "int: " + i);
        }
    }

    /// <summary>
    /// Finds vertices which relate to eachother and returns a list of them
    /// </summary>
    private List<int> FindRelatedVertices(Vector3 targetPt, bool findConnected)
    {
        List<int> relatedVertices = new List<int>();

        int idx = 0;
        Vector3 pos;

        for (int t = 0; t < triangles.Length; t++)
        {
            idx = triangles[t];
            pos = vertices[idx];
            if (pos == targetPt)
            {
                relatedVertices.Add(idx);
                if (findConnected)
                {
                    if (t == 0)
                    {
                        relatedVertices.Add(triangles[t + 1]);
                    }
                    if (t == triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                    }
                    if (t > 0 && t < triangles.Length - 1)
                    {
                        relatedVertices.Add(triangles[t - 1]);
                        relatedVertices.Add(triangles[t + 1]);
                    }
                }
            }
        }
        return relatedVertices;
    }

    /// <summary>
    /// Pulls vertices to match game object's movement
    /// </summary>
    private void PullSimilarVertices(int index, Vector3 newPos)
    {
        Vector3 targetVertexPos = vertices[index]; 
        List<int> relatedVertices = FindRelatedVertices(targetVertexPos, false);
        foreach (int i in relatedVertices)
        {
            vertices[i] = newPos;
        }
        clonedMesh.vertices = vertices;
        clonedMesh.RecalculateNormals();
    }

    void Update()
    {
        Debugger();
        CheckMovement();
        if(isReset == true)
        {
            Reset();
        }
    }

    private void Debugger()
    {
        foreach (Transform child in transform)
        {
            var cubeRenderer = child.GetComponent<Renderer>();
            cubeRenderer.enabled = isDebug;
        }
    }

    /// <summary>
    /// Checks for movement in the created game objects to pull mesh's vertices accordingly
    /// </summary>
    private void CheckMovement()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            if (child.transform.hasChanged)
            {
                PullSimilarVertices(i, child.transform.localPosition);
                child.transform.hasChanged = false;
            }
            i++;
        }
    }

    /// <summary>
    /// Resets mesh and points to original
    /// </summary>
    public void Reset()
    {
        if (clonedMesh != null && originalMesh != null)
        {
            clonedMesh.vertices = originalMesh.vertices;
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;
            meshFilter.mesh = clonedMesh;

            vertices = clonedMesh.vertices;
            triangles = clonedMesh.triangles;

            List<Transform> childList = new List<Transform>();
            foreach (Transform child in gameObject.transform)
            {
                childList.Add(child);
            }

            for (int i = 0; i < originalMesh.vertices.Length; i++)
            {
                point = transform.TransformPoint(originalMesh.vertices[i]);

                Transform child = childList[i];
                child.transform.position = point;
            }
            
        }
    }
}
