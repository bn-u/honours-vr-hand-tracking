using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrackMovement : MonoBehaviour
{
    private bool tracking = false;
    private Vector3 startPos;
    private Vector3 endPos;

    public bool right;
    public GameObject targetMesh;
    public bool isDebug = false;
    List<Vector3> pathPoints = new List<Vector3>();
    List<Vector3> points = new List<Vector3>();

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
        pathPoints.Add(startPos);
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
        trackPos();
        pathPoints.Add(endPos);
        createSpline();
    }

    static Vector3 LeftPos()
    {
        return GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.position;
    }

    static Vector3 RightPos()
    {
        return GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.position;
    }



    private void FindRight()
    {
        int childCount = targetMesh.transform.hierarchyCount;
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
        }

    }

    void createSpline()
    {
        var container = gameObject.GetComponent<SplineContainer>();
        if (container == null)
        {
            container = gameObject.AddComponent<SplineContainer>();
        }
        container.RemoveSpline(container.Spline);
        var spline = container.AddSpline();
        var knots = new BezierKnot[pathPoints.Count];
        for (int i = 0; i < pathPoints.Count; i++)
        {
            knots[i] = new BezierKnot(pathPoints[i], -0.05f * Vector3.forward, 0.05f * Vector3.forward);

            spline.Knots = knots;
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
    }

    void trackPos()
    {
        var result = EveryNthElement(points, 12);
        for (int i = 0;i < result.Count;i++)
        {
            pathPoints.Add(result[i]);
        }
        Debug.Log("Points: " + points.Count);
        Debug.Log("Result: " + result.Count);
    }

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

}

