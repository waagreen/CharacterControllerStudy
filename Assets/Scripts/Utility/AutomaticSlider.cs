using UnityEngine;
using UnityEngine.Events;

public class AutomaticSlider : MonoBehaviour
{
    [System.Serializable] public class OnValueChangedEvent : UnityEvent<float> { };

    [SerializeField][Min(0f)] private float duration;
    [SerializeField] private bool disableOnAwake = false, autoReverse = false, smoothStep = false;
    [SerializeField] private OnValueChangedEvent onValueChanged = default;

    private float value;
    private float SmoothedValue => 3f * value * value - 2f * value * value * value; //Smooth step formula (3v² - 2v³)

    public bool Reversed { get; set; }
    public bool AutoReverse
    {
        get => autoReverse;
        set => autoReverse = value;
    }

    private void Awake()
    {
        enabled = !disableOnAwake;
    }

    private void FixedUpdate()
    {
        float delta = Time.deltaTime / duration;

        if (Reversed)
        {
            value -= delta;
            if (value <= 0f)
            {
                if (autoReverse)
                {
                    // Prevent overshoot in the case of extremely short durations
                    value = Mathf.Min(1f, -value);
                    Reversed = false;
                }
                else
                {
                    value = 0f;
                    enabled = false;
                }
            }

        }
        else
        {
            value += delta;
            if (value >= 1f)
            {
                if (autoReverse)
                {
                    // Prevent overshoot in the case of extremely short durations
                    value = Mathf.Max(0f, 2f - value);
                    Reversed = true;
                }
                else
                {
                    value = 1f;
                    enabled = false;
                }
            }
        }

        onValueChanged.Invoke(smoothStep ? SmoothedValue : value);
    }
}
