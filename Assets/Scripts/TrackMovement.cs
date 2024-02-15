using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TrackMovement : MonoBehaviour
{
    public bool tracking = false;
    Vector3 point;
    public SplineContainer spline;
    public Transform KnotTarget;
    private Vector3 startPos;
    private Vector3 endPos;
    private Quaternion startRot;
    private Quaternion endRot;

    public bool right;
    public GameObject mesh;

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
    }

    private void FindRight()
    {
        GameObject[] childs = mesh.transform.childCount;
        for (int i = 0; i < childs.Length; i++)
        {
            Debug.Log(childs[i]);

        }

        Vector3 localPos = startPos.transform.InverseTransformPoint();

        if (localPos.x > 0)
        {
            //left
        }
        else if (localPos.x < 0)
        {
            //right
        }
    }

    void Update()
    {
        /*
        if (tracking == true)
        {
            // record position at start

            GameObject userInteractor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            userInteractor.transform.position = startPos;
            userInteractor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);


            // record array of positions
            BezierKnot[] splineArray = spline.Spline.ToArray();
            for (int i = 0; i < splineArray.Length; i++)
            {
            }
            

        }
        */

        var knot0 = spline.Spline.ToArray()[0];
        knot0.Position = spline.transform.InverseTransformPoint(startPos);
        knot0.Rotation = Quaternion.Inverse(spline.transform.rotation) * startRot;
        spline.Spline.SetKnot(0, knot0);

        var knot1 = spline.Spline.ToArray()[1];
        knot1.Position = spline.transform.InverseTransformPoint(endPos);
        knot1.Rotation = Quaternion.Inverse(spline.transform.rotation) * endRot;
        spline.Spline.SetKnot(1, knot1);
    }
}

