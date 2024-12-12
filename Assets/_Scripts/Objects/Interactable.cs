using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Surlignage")]
    [SerializeField] private Material outlineMaterial;
    private Renderer objectRenderer;
    private Material[] originalMaterials;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.materials;
        }
    }

    public void OnFocused()
    {
        EnableHighlight(true);
    }

    public void OnFocusLost()
    {
        EnableHighlight(false);
    }

    private void EnableHighlight(bool enable)
    {
        if (objectRenderer == null || outlineMaterial == null) return;

        if (enable)
        {
            Material[] newMaterials = new Material[originalMaterials.Length + 1];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                newMaterials[i] = originalMaterials[i];
            }
            newMaterials[newMaterials.Length - 1] = outlineMaterial;
            objectRenderer.materials = newMaterials;
        }
        else
        {
            objectRenderer.materials = originalMaterials;
        }
    }

    public void Interact()
    {
        Debug.Log($"{gameObject.name} ramassé !");
        InventoryManager.Instance.AddToInventory(gameObject);
    }
}
