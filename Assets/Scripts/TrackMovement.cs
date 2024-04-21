using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrackMovement : MonoBehaviour
{
    private bool tracking = false;
    private Vector3 startPos;
    private Vector3 endPos;
    private Quaternion pointRotation;
    private int gestureType;
    private bool pickEnabled = false;
    private Renderer renderer;
    Material meshMat;

    public bool right;
    public GameObject targetMesh;
    public bool isDebug = false;
    public float pickSize = .02f;

    List<Vector3> pathPoints = new List<Vector3>();
    List<Vector3> points = new List<Vector3>();
    List<GameObject> childList = new List<GameObject>();

    public void CutEnable() { gestureType = 1; EnableTracker(); }

    public void GrowEnable() { gestureType = 2; EnableTracker(); }

    public void PickEnable() { gestureType = 3; Pick(); }

    void Start()
    {
        renderer = targetMesh.GetComponent<Renderer>();
        meshMat = renderer.material;
    }

    /// <summary>
    /// Finds the verticies with radius of hand, ready for the update script to move them
    /// </summary>
    public void Pick()
    {
        //Create radius around point
        switch (right)
        {
            case true:
                startPos = RightPos();
                break;
            case false:
                startPos = LeftPos();
                break;
        }

        //Find collider in radius
        if (targetMesh != null)
        {
            int childCount = targetMesh.transform.hierarchyCount;
            childCount--;

            Collider[] radiusObjects = Physics.OverlapSphere(startPos, pickSize);
            Debug.Log("Objects in radius: " + radiusObjects.Length);

            childList.Clear();

            List<GameObject> colliderList = new List<GameObject>();

            for (int i = 0; i < radiusObjects.Length; i++)
            {
                colliderList.Add(radiusObjects[i].gameObject);
            }

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = targetMesh.transform.GetChild(i).gameObject;

                foreach (GameObject colliderParent in colliderList)
                {
                    if (colliderParent == child)
                    {
                        childList.Add(child);
                    }
                }
            }
            pickEnabled = true;
        }
    }

    public void EnableTracker()
    {
        points.Clear();
        pathPoints.Clear();
        switch (right)
        {
            case true:
                startPos = RightPos();
                break;
            case false:
                startPos = LeftPos();
                break;
        }
        tracking = true;
    }

    public void DisableTracker()
    {
        switch (right)
        {
            case true:
                endPos = RightPos();
                break;
            case false:
                endPos = LeftPos();
                break;
        }
        tracking = false;

        switch (gestureType)
        {
            case 1:
                try
                {
                    meshMat.SetVector("_StartPos", startPos);
                    meshMat.SetVector("_EndPos", endPos);
                }
                catch
                {
                    Debug.Log("Cannot change shader's properites");
                }

                break;
            case 2:
                trackPos();
                pathPoints.Add(endPos);
                createSpline();
                break;
            case 3:
                pickEnabled = false;
                break;
        }

    }

    static Vector3 LeftPos()
    {
        return GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.position;
    }

    static Vector3 RightPos()
    {
        return GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.position;
    }
    
    /// <summary>
    /// Creates a spline following the previously recorded points
    /// </summary>
    void createSpline()
    {
        
        var container = gameObject.GetComponent<SplineContainer>();
        if (container == null)
        {
            container = gameObject.AddComponent<SplineContainer>();
        }
        var spline = container.AddSpline();
        var knots = new BezierKnot[pathPoints.Count];
        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (i != 0)
            {
                pointRotation = Quaternion.LookRotation(pathPoints[i - 1] - pathPoints[i], Vector3.up);
                pointRotation = Quaternion.Inverse(pointRotation);
            }

            knots[i] = new BezierKnot(pathPoints[i], -0.05f * Vector3.forward, 0.05f * Vector3.forward, pointRotation);

        }

        spline.Knots = knots;

        SplineExtrude extrude = gameObject.GetComponent<SplineExtrude>();
        if (extrude == null)
        {
            extrude = gameObject.AddComponent<SplineExtrude>();
        }

        extrude.Container = container;

        switch (gestureType)
        {
            case 1:
                //container.RemoveSpline(container.Spline);
                container.RemoveSpline(container.Spline);
                break;
            case 2:
                extrudeMesh(extrude);
                break;
        }

        //FindRight(spline);
    }

    /// <summary>
    /// Builds a mesh along the spline's path
    /// </summary>
    /// <param name="extrude"></param>
    void extrudeMesh(SplineExtrude extrude)
    {
        if (extrude != null)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh extrusion = new Mesh();
            extrusion.name = "extrusionLine";
            meshFilter.mesh = extrusion;

            Material newMat = new Material(Shader.Find("Standard"));
            MeshRenderer meshRender = GetComponent<MeshRenderer>();
            meshRender.material = newMat;

            extrude.Radius = 0.01f;
            extrude.SegmentsPerUnit = 50;

            extrude.Rebuild();
            Debug.Log("Rebuilt");
        }

    }

    void Update()
    {
        if (tracking == true)
        {
            Vector3 pos;
            switch (right)
            {
                case true:
                    pos = RightPos();
                    break;
                case false:
                    pos = LeftPos();
                    break;
            }
            points.Add(pos);
        }

        //Attach gameObject to finger until finish
        if (pickEnabled == true)
        {
            foreach (GameObject child in childList)
            {
                Vector3 pos = RightPos();
                child.transform.position = pos;
                Debug.Log("picked");
            }
        }
    }

    void trackPos()
    {
        var result = EveryNthElement(points, 12);
        for (int i = 0;i < result.Count;i++)
        {
            pathPoints.Add(result[i]);
        }
        if (isDebug == true)
        {
            Debug.Log("trackPos() Points: " + points.Count);
            Debug.Log("trackPos() Result: " + result.Count);
        }
    }

    /// <summary>
    /// Algorithm for getting a variable every N number of passes
    /// </summary>
    /// <param name="list"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    static List<Vector3> EveryNthElement(List<Vector3> list, int n)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            if ((i % n) == 0)
            {
                result.Add(list[i]);
            }
        }
        return result;
    }

    private void FindRight(Spline currentSpline)
    {
        if (targetMesh != null)
        {
            int childCount = targetMesh.transform.hierarchyCount;
            childCount--;

            for (int j = 0; j < pathPoints.Count; j++)
            {
                Collider[] radiusObjects = Physics.OverlapSphere(pathPoints[j], 0.2f);
                Debug.Log("Objects in radius: " + radiusObjects.Length);
            }

            for (int i = 0; i < childCount; i++)
            {
                GameObject child = targetMesh.transform.GetChild(i).gameObject;

                //convert child to local space of start pos
                //check if its located left/right/up/down to start pos

                //convert child to local space of end pos
                //check if its located left/right/up/down to end pos

                //cross reference if points are both 

                Vector3 localPos = child.transform.InverseTransformPoint(startPos);
                if (localPos.x > 0)
                {
                    //left
                }
                else if (localPos.x < 0)
                {
                    //right
                }



                //if child is in radius then
                MoveToSpline(child, currentSpline);

            }

            if (isDebug == true)
            {
                Debug.Log("FindRight() childCount: " + childCount);
            }
        }
    }

    private void MoveToSpline(GameObject child, Spline currentSpline)
    {
        var point = gameObject.transform.InverseTransformPoint(child.transform.position);
        float distance = SplineUtility.GetNearestPoint(currentSpline, point, out float3 nearest, out float t);

        Vector3 localNear = nearest;
        var near = localNear + gameObject.transform.position;
        child.transform.position = near;
    }

}

