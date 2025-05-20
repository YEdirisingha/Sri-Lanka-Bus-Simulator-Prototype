using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBusSettings", menuName = "Vehicle/Bus Settings")]
public class BusSettings : ScriptableObject
{
    [Header("Bus Settings")]
    public float motorForce = 3000f;
    public float brakeForce = 8000f;
    public float maxSteerAngle = 25f;
    public float reverseForce = 1500f;

    //future use
    public float topSpeed = 120f;
    public float steerSensitivity = 1f;
    public float mass = 1500f;
    public float drag = 0.5f;

    [Header("Handling & Physics Settings")]
    public Vector3 centerOfMass;
    public float suspensionDistance;
    public float springForce;
    public float damperForce;

    public float forwardFriction = 1f;
    public float sidewaysFriction = 1f;

    [Header("Visual & Audio")]
    public string busName = "City Bus";
    public GameObject busModelPrefab;
    public AudioClip engineSound;
    public AudioClip brakeSound;

}
