using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class DetectionZone : MonoBehaviour
{
    [SerializeField] private UnityEvent onFirstEnter = default, onLastExit = default;

    private List<Collider> colliders;
    private Collider col;

    private void Start()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true;
        colliders = new();

        enabled = false; // Disable this component to avoid needlessly  calling fixed update
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            Collider c = colliders[i];
            if (c && c.gameObject.activeInHierarchy) continue;

            colliders.RemoveAt(i--);
            if (colliders.Count < 1)
            {
                onLastExit.Invoke();
                enabled = false; // Disable this component to avoid needlessly  calling fixed update
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (colliders.Count < 1)
        {
            enabled = true; // Zone is only active when something enters it
            onFirstEnter.Invoke();
        }

        colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (colliders.Remove(other) && colliders.Count < 1)
        {
            onLastExit.Invoke();
            enabled = false; // Disable this component to avoid needlessly  calling fixed update
        }
    }

    private void OnDisable()
    {
        if (colliders.Count > 0)
        {
            colliders.Clear();
            onLastExit.Invoke();
        }
    }
}
