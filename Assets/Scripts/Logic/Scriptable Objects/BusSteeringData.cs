using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BusSteeringData", menuName = "Vehicle/Steering Data")]
public class BusSteeringData : ScriptableObject
{
    public float maximumSteeringAngle = 180f;
    public float wheelReleasedSpeed = 300f;
    public float valueMultiplier = 0.5f;
    public Vector3 centerOfMassOffset;
}
