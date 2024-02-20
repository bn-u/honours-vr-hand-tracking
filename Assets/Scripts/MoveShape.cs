using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class movetospline : MonoBehaviour
{

    private SplineContainer rail;

    private Spline currentSpline;

    private Rigidbody rb;

    public GameObject target;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rail = target.GetComponent<SplineContainer>();

        currentSpline = rail.Splines[0];

        var point = target.transform.InverseTransformPoint(gameObject.transform.position);
        Debug.Log(point);
        transform.position = point;

        float distance = SplineUtility.GetNearestPoint(currentSpline, point, out float3 nearest, out float t);

        Vector3 localNear = nearest;

        var near = localNear + target.transform.position;

        transform.position = near;


        Vector3 forward = Vector3.Normalize(currentSpline.EvaluateTangent(t));
        Vector3 up = currentSpline.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 0, 1);
        var remappedUp = new Vector3(0, 1, 0);

        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;
    }
}