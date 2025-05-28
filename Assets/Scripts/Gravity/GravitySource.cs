using UnityEngine;

public class GravitySource : MonoBehaviour
{
    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }

    private void OnEnable()
    {
        CustomGravity.RegisterSource(this);
    }

    private void OnDisable()
    {
        CustomGravity.UnregisterSource(this);
    }
}
