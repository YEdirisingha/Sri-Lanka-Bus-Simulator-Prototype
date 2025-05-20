using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float maxSideOffset = 3f;   // sideways shift at full steer
    [SerializeField] private float maxDutchAngle = 5f;   // camera tilt at full steer
    [SerializeField] private float responsiveness = 4f;   // how fast cam moves

    [Header("Drive Settings")]
    [SerializeField] private float minSpeed = 1f;   // below this, no peek

    private CinemachineVirtualCamera vcam;
    private CinemachineTransposer transposer;
    private CinemachineComposer composer;
    private playerController bus;
    private Rigidbody busRb;

    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        bus = FindObjectOfType<playerController>();

        if (bus == null)
        {
            Debug.LogError("BusCameraController: No playerController found in scene.");
            enabled = false;
            return;
        }

        busRb = bus.GetComponent<Rigidbody>();
        if (busRb == null)
            Debug.LogWarning("BusCameraController: playerController has no Rigidbody; speed check will fail.");
    }

    private void LateUpdate()
    {
        // 1) Check speed
        float speed = (busRb != null) ? busRb.velocity.magnitude : 0f;

        // 2) Read steer only if moving
        float steer = (speed > minSpeed) ? bus.GetSteering() : 0f;

        // 3) Compute targets
        float targetX = steer * maxSideOffset;
        float targetDutch = -steer * maxDutchAngle;

        // 4) Slide the Transposer’s follow-offset
        Vector3 off = transposer.m_FollowOffset;
        off.x = Mathf.Lerp(off.x, targetX, Time.deltaTime * responsiveness);
        transposer.m_FollowOffset = off;

        // 5) Nudge the Composer’s tracked-object offset
        Vector3 toff = composer.m_TrackedObjectOffset;
        toff.x = Mathf.Lerp(toff.x, -targetX * 0.5f, Time.deltaTime * responsiveness);
        composer.m_TrackedObjectOffset = toff;

        // 6) Apply Dutch tilt on the Virtual Camera’s lens
        vcam.m_Lens.Dutch = Mathf.Lerp(vcam.m_Lens.Dutch, targetDutch, Time.deltaTime * responsiveness);
    }
}
