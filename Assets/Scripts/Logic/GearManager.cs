using UnityEngine;
using UnityEngine.UI;

public class GearManager : MonoBehaviour
{
   
    public Slider gearSlider;
    public float snapThreshold = 0.5f;
    private bool isDragging = false;
    public Text[] indicator;

    public enum Gear { Reverse, Drive }
    public Gear currentGear = Gear.Drive;
    public playerController playerController;

    void Start()
    {
        gearSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        isDragging = true;
    }

    void Update()
    {
        if (isDragging && !Input.GetMouseButton(0) && !Input.touchCount.Equals(1))
        {
            float value = gearSlider.value;
            if (value < snapThreshold)
            {
                gearSlider.value = 0;
                currentGear = Gear.Drive;
                indicator[0].color= Color.yellow;
                indicator[1].color = Color.black;
                playerController.isReversing= false;
            }
            else
            {
                gearSlider.value = 1;
                currentGear = Gear.Reverse;
                indicator[0].color = Color.black;
                indicator[1].color = Color.yellow;
                playerController.isReversing = true;
            }
            isDragging = false;
        }
    }

    public int GetDirectionMultiplier()
    {
        return currentGear == Gear.Drive ? 1 : -1;
    }
}


