using SimpleInputNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SteeringWheelController : MonoBehaviour, ISimpleInputDraggable
{
    public SimpleInput.AxisInput axis = new SimpleInput.AxisInput("Horizontal");

    private Graphic wheel;
    private RectTransform wheelTR;
    private Vector2 centerPoint;

    public float maximumSteeringAngle = 180f;
    public float wheelReleasedSpeed = 300f;
    public float valueMultiplier = 0.5f;

    private float wheelAngle = 0f;
    private float wheelPrevAngle = 0f;

    private bool wheelBeingHeld = false;

    private float m_value;
    public float Value { get { return m_value; } }
    public float Angle { get { return wheelAngle; } }

    public playerController playerController; // <-- Added your playerController reference

    private void Awake()
    {
        wheel = GetComponent<Graphic>();
        wheelTR = wheel.rectTransform;

        SimpleInputDragListener eventReceiver = gameObject.AddComponent<SimpleInputDragListener>();
        eventReceiver.Listener = this;
    }

    private void OnEnable()
    {
        axis.StartTracking();
        SimpleInput.OnUpdate += OnUpdate;
    }

    private void OnDisable()
    {
        wheelBeingHeld = false;
        wheelAngle = wheelPrevAngle = m_value = 0f;
        wheelTR.localEulerAngles = Vector3.zero;

        axis.StopTracking();
        SimpleInput.OnUpdate -= OnUpdate;
    }

    private void OnUpdate()
    {
        // If the wheel is released, reset the rotation
        // to initial (zero) rotation by wheelReleasedSpeed degrees per second
        if (!wheelBeingHeld && wheelAngle != 0f)
        {
            float deltaAngle = wheelReleasedSpeed * Time.deltaTime;
            if (Mathf.Abs(deltaAngle) > Mathf.Abs(wheelAngle))
                wheelAngle = 0f;
            else if (wheelAngle > 0f)
                wheelAngle -= deltaAngle;
            else
                wheelAngle += deltaAngle;
            if (Mathf.Abs(wheelAngle) < 1f)
                wheelAngle = 0f;
        }

        // Rotate the wheel image
        wheelTR.localEulerAngles = new Vector3(0f, 0f, -wheelAngle);

        m_value = wheelAngle * valueMultiplier / maximumSteeringAngle;
        axis.value = m_value;

        float normalizedAngle = wheelAngle / maximumSteeringAngle;
        float targetSteering = normalizedAngle * 0.6f; // More linear for bus

        if (playerController != null)
        {
            float smoothingSpeed = 2.5f; // Slower response for realism
            float currentSteering = Mathf.Lerp(playerController.GetSteering(), targetSteering, smoothingSpeed * Time.deltaTime);
            playerController.SetSteering(currentSteering);
        }


    }

    public void OnPointerDown(PointerEventData eventData)
    {
        wheelBeingHeld = true;
        centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, wheelTR.position);
        wheelPrevAngle = GetAngle(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pointerPos = eventData.position;

        float wheelNewAngle = GetAngle(pointerPos);

        if ((pointerPos - centerPoint).sqrMagnitude >= 100f)
        {
            float sensitivity = 1.0f; // Reduced for realistic bus handling

            float deltaAngle = Mathf.DeltaAngle(wheelPrevAngle, wheelNewAngle); // fix wrap-around issues
            wheelAngle += deltaAngle * sensitivity;
        }

        wheelAngle = Mathf.Clamp(wheelAngle, -maximumSteeringAngle, maximumSteeringAngle);
        wheelPrevAngle = wheelNewAngle;
    }

    private float GetAngle(Vector2 pointerPosition)
    {
        Vector2 fromCenter = pointerPosition - centerPoint;
        return Mathf.Atan2(fromCenter.x, fromCenter.y) * Mathf.Rad2Deg;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Executed when mouse/finger stops touching the steering wheel
        // Performs one last OnDrag calculation, just in case
        OnDrag(eventData);

        wheelBeingHeld = false;
    }
}
