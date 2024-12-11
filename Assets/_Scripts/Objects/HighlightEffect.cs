using UnityEngine;

public class HighlightEffect : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material[] originalMaterials; // Stocke les mat�riaux d'origine
    [SerializeField] private Material outlineMaterial; // Mat�riau de surbrillance

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // Sauvegarde les mat�riaux d'origine
            originalMaterials = objectRenderer.materials;
        }
    }

    public void EnableHighlight(bool enable)
    {
        if (objectRenderer == null || outlineMaterial == null) return;

        if (enable)
        {
            // Ajoute le mat�riau de surbrillance
            AddOutlineMaterial();
        }
        else
        {
            // R�initialise les mat�riaux d'origine
            RestoreOriginalMaterials();
        }
    }

    private void AddOutlineMaterial()
    {
        // V�rifie si l'outline n'est pas d�j� appliqu�
        foreach (Material mat in objectRenderer.materials)
        {
            if (mat == outlineMaterial) return;
        }

        // Cr�e un tableau avec l'outline ajout�
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[newMaterials.Length - 1] = outlineMaterial;

        // Applique les nouveaux mat�riaux
        objectRenderer.materials = newMaterials;
    }

    private void RestoreOriginalMaterials()
    {
        // R�initialise les mat�riaux � leur �tat d'origine
        objectRenderer.materials = originalMaterials;
    }
}
