using UnityEngine;

public class MaterialSelector : MonoBehaviour
{
    [SerializeField] private Material[] materials;
    [SerializeField] private MeshRenderer rend;

    public void Select(int index)
    {
        if (materials == null || rend == null) return;
        if (index < 0 || index >= materials.Length) return;

        rend.material = materials[index];
    }
}
