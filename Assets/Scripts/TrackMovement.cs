using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrackMovement : MonoBehaviour
{
    private bool tracking = false;
    private Vector3 startPos;
    private Vector3 endPos;
    private Quaternion startRot;
    private Quaternion endRot;

    public bool right;
    public GameObject targetMesh;
    public bool isDebug = false;
    List<Vector3> pathPoints = new List<Vector3>();

    public void EnableTracker()
    {
        switch (right)
        {
            case true:
                startPos = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.position;
                startRot = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.rotation;
                break;
            case false:
                startPos = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.position;
                startRot = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.rotation;
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
                endPos = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.position;
                endRot = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Right Hand/Poke Interactor").transform.rotation;
                break;
            case false:
                endPos = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.position;
                endRot = GameObject.Find("/XR Interaction Hands Setup/XR Origin (XR Rig)/Camera Offset/Left Hand/Poke Interactor").transform.rotation;
                break;
        }
        tracking = false;
        createSpline();
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
        pathPoints.Add(endPos);

        var container = gameObject.AddComponent<SplineContainer>();
        var spline = container.AddSpline();
        var knots = new BezierKnot[pathPoints.Count];

        for (int i = 0; i < pathPoints.Count; i++)
        {
            knots[i] = new BezierKnot(pathPoints[i], 3 * Vector3.forward, 3 * Vector3.forward);
        }

        spline.Knots = knots;

    }

    void Update()
    {
        if (tracking == true)
        {
            //pathPoints.Add(1);
        }
    }
}

