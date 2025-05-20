using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [Header("Bus Settings")]
    [SerializeField] private float motorForce = 3000f;    //needed to be use scriptable objects
    [SerializeField] private float brakeForce = 8000f;  //needed to be use scriptable objects
    [SerializeField] private float maxSteerAngle = 25f; //needed to be use scriptable objects
    [SerializeField] private float reverseForce = 1500f;    //needed to be use scriptable objects

    [Header("Engine Resistance")]
    [SerializeField] private float engineBrakeForce = 200f; // Braking when throttle is 0
    [SerializeField] private float accelerationRate = 2f; // How fast throttle increases
    [SerializeField] private float decelerationRate = 5f; // How fast throttle decreases

    [Header("Speed Limiting")]
    [SerializeField] private float maxSpeedKPH = 80f; // target top speed

    [Header("Wheels")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Steering Settings")]
    [SerializeField] private float steeringSmoothTime = 0.2f;
    [SerializeField] private float currentSteering = 0f;
    [SerializeField] private float steeringVelocity = 0f;

    /*[Header("References for Suspension")]
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
*/
    /*[Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLenght;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;*/

    /*[Header("Drag Settings")]
    [SerializeField] private float lateralFrictionStrength = 300f;
    [SerializeField] private float dragThresholdSpeed = 5f;

    [Header("Body Lean Settings")]
    [SerializeField] private Transform busBody;               // your chassis/root
    [SerializeField] private float maxBodyRollAngle = 15f;     // deg into turns
    [SerializeField] private float maxBodySquatAngle = 1.5f;    // deg on brake*/

    [Header("Body Spring Settings")]
    [SerializeField] private float bodySpringStiffness = 60f;
    [SerializeField] private float bodyDamping = 6f;
    [SerializeField] private float minLeanSpeed = 2f;  // below this, no lean
    [SerializeField] private float maxLeanSpeed = 30f;  // above this, full effect

    [Header("Steering Bounce")]
    [SerializeField] private float steeringBounceStrength = 3f;
    private float prevSteeringInput = 0f;

    private float currentBodyRoll = 0f;
    private float bodyRollVelocity = 0f;
    private float currentBodyPitch = 0f;
    private float bodyPitchVelocity = 0f;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    private Rigidbody rb;
    private float steeringInput;
    private float throttleInput;
    private float brakeInput;
    private float currentThrottle = 0f;

    //public Text gearIndicator;
    private bool isAccelerating = false;
    private bool isBraking = false;

    //private int[] wheelIsGrounded = new int[4];
    //private bool isGrounded = false;


   
    public bool isReversing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 0.1f; // Tiny drag for realism
        rb.angularDrag = 0.5f;
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f); // Less extreme
    }

    private void FixedUpdate()
    {
        // Smoothly adjust throttleInput
        if (isAccelerating)
            throttleInput = Mathf.MoveTowards(throttleInput, 1f, accelerationRate * Time.fixedDeltaTime);
        else
            throttleInput = Mathf.MoveTowards(throttleInput, 0f, decelerationRate * Time.fixedDeltaTime);

        // Smoothly adjust brakeInput
        if (isBraking)
            brakeInput = Mathf.MoveTowards(brakeInput, 1f, accelerationRate * Time.fixedDeltaTime);
        else
            brakeInput = Mathf.MoveTowards(brakeInput, 0f, decelerationRate * Time.fixedDeltaTime);

        //CheckReverseState();
        HandleMotor();
        HandleSteering();
        /*float steerDelta = steeringInput - prevSteeringInput;
        if (Mathf.Abs(steerDelta) > 0.01f)
        {
            // impulse in opposite direction of change → small overshoot/settle
            bodyRollVelocity += -steerDelta * maxBodyRollAngle * steeringBounceStrength;
        }
        prevSteeringInput = steeringInput;

        BodyLean();*/

        UpdateWheels();
        //Suspension();
        //GroundCheck();
        //ApplyLateralDrag();

    }

   /* private void CheckReverseState()
    {
        if (brakeInput > 0f && throttleInput == 0f && rb.velocity.magnitude < 0.5f)
        {
            brakeHoldTime += Time.fixedDeltaTime;
            if (brakeHoldTime >= timeToReverse)
            {
                isReversing = true;
            }
        }
        else if (throttleInput > 0f)
        {
            isReversing = false;
            brakeHoldTime = 0f;
        }
        else if (brakeInput == 0f)
        {
            brakeHoldTime = 0f;
        }
    }*/
    private void HandleMotor()
    {
        if (throttleInput > 0f /*&& !isReversing*/)
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, 1f, accelerationRate * Time.fixedDeltaTime);
        }
        else
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, 0f, decelerationRate * Time.fixedDeltaTime);
        }

        float motorTorque = 0f;
        float currentSpeedKPH = rb.velocity.magnitude * 3.6f;

       if (isReversing)
        {
            motorTorque = throttleInput * -reverseForce;
            //gearIndicator.text = "R";
        }
        else
        {
            if (currentSpeedKPH < maxSpeedKPH)
            {
                motorTorque = currentThrottle * motorForce;
            }
            else
            {
                motorTorque = 0f;
            }
            //gearIndicator.text = "D";
        }

        rearLeftWheelCollider.motorTorque = motorTorque;
        rearRightWheelCollider.motorTorque = motorTorque;

        float totalBrake = 0f;

        /*if (!isReversing)
        {*/
            totalBrake = brakeInput * brakeForce;

            if (Mathf.Approximately(currentThrottle, 0f) && brakeInput == 0f)
            {
                totalBrake += engineBrakeForce;

                if (rb.velocity.magnitude < 10f)
                {
                    totalBrake = 0f;
                }
            }
        /*}*/

        frontLeftWheelCollider.brakeTorque = totalBrake;
        frontRightWheelCollider.brakeTorque = totalBrake;
        rearLeftWheelCollider.brakeTorque = totalBrake;
        rearRightWheelCollider.brakeTorque = totalBrake;
    }


    private void HandleSteering()
    {
        // Smooth steering interpolation
        currentSteering = Mathf.SmoothDamp(currentSteering, steeringInput, ref steeringVelocity, steeringSmoothTime);
        float steerAngle = currentSteering * maxSteerAngle;

        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }


    private void UpdateWheels()
    {
        UpdateWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheel(WheelCollider collider, Transform visualWheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        // Update ONLY the visual wheel
        visualWheelTransform.position = pos;
        visualWheelTransform.rotation = rot;
    }
    /*private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLenght + springTravel;

            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
            {

                wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLenght - currentSpringLength) / springTravel;

                float springVelocity = Vector3.Dot(rb.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springCompression;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelIsGrounded[i] = 0;
                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.green);
            }

        }
    }*/

    /*private void GroundCheck()
    {
        int tempGroundWheels = 0;

        for (int i = 0; i < wheelIsGrounded.Length; i++)
        {
            tempGroundWheels += wheelIsGrounded[i];
        }

        if (tempGroundWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

    }*/

    /*private void ApplyLateralDrag()
    {
        Vector3 velocity = rb.velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Only apply drag when moving forward and above threshold
        if (localVelocity.z > dragThresholdSpeed)
        {
            // Remove sideways movement
            Vector3 lateralVelocity = transform.right * localVelocity.x;

            // Apply force to counteract lateral motion
            rb.AddForce(-lateralVelocity * lateralFrictionStrength);
        }
    }*/

    /*private void BodyLean()
    {
        float speed = rb.velocity.magnitude;

        // 1) if too slow, just snap back to zero
        if (speed < minLeanSpeed)
        {
            // smooth-center both axes
            currentBodyRoll = Mathf.Lerp(currentBodyRoll, 0f, bodyDamping * Time.fixedDeltaTime);
            currentBodyPitch = Mathf.Lerp(currentBodyPitch, 0f, bodyDamping * Time.fixedDeltaTime);
            bodyRollVelocity = 0f;
            bodyPitchVelocity = 0f;
            busBody.localRotation = Quaternion.Euler(currentBodyPitch, 0f, currentBodyRoll);
            return;
        }

        // 2) compute target angles
        float targetRoll = -steeringInput * maxBodyRollAngle;
        float targetPitch = brakeInput * maxBodySquatAngle;

        // 3) speed-scale spring/damper
        float t = Mathf.Clamp01((speed - minLeanSpeed) / (maxLeanSpeed - minLeanSpeed));
        float spring = bodySpringStiffness * t;
        float damp = bodyDamping * t;

        // 4) optionally boost return speed on pitch when brakeInput drops
        if (brakeInput == 0f)
        {
            spring *= 3f;
            damp *= 3f;
        }

        // 5) spring-damper on roll
        float rollForce = (targetRoll - currentBodyRoll) * spring
                          - bodyRollVelocity * damp;
        bodyRollVelocity += rollForce * Time.fixedDeltaTime;
        currentBodyRoll += bodyRollVelocity * Time.fixedDeltaTime;

        // 6) spring-damper on pitch
        float pitchForce = (targetPitch - currentBodyPitch) * spring
                           - bodyPitchVelocity * damp;
        bodyPitchVelocity += pitchForce * Time.fixedDeltaTime;
        currentBodyPitch += bodyPitchVelocity * Time.fixedDeltaTime;

        // 7) apply
        busBody.localRotation = Quaternion.Euler(
            currentBodyPitch,
            0f,
            currentBodyRoll
        );
    }*/


    // PUBLIC METHODS for UI BUTTONS

    public void PressAccelerate()
    {
        isAccelerating = true;
    }

    public void ReleaseAccelerate()
    {
        isAccelerating = false;
    }

    public void PressBrake()
    {
        isBraking = true;
    }

    public void ReleaseBrake()
    {
        isBraking = false;
    }


    public void PressSteerLeft()
    {
        steeringInput = -1f;
    }

    public void PressSteerRight()
    {
        steeringInput = 1f;
    }

    public void ReleaseSteering()
    {
        steeringInput = 0f;
    }

    public void SetSteering(float input)
    {
        steeringInput = input;
    }

    public float GetSteering()
    {
        return steeringInput;
    }


}