using UnityEngine;

public class HighlightEffect : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material[] originalMaterials; // Stocke les matériaux d'origine
    [SerializeField] private Material outlineMaterial; // Matériau de surbrillance

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // Sauvegarde les matériaux d'origine
            originalMaterials = objectRenderer.materials;
        }
    }

    public void EnableHighlight(bool enable)
    {
        if (objectRenderer == null || outlineMaterial == null) return;

        if (enable)
        {
            // Ajoute le matériau de surbrillance
            AddOutlineMaterial();
        }
        else
        {
            // Réinitialise les matériaux d'origine
            RestoreOriginalMaterials();
        }
    }

    private void AddOutlineMaterial()
    {
        // Vérifie si l'outline n'est pas déjà appliqué
        foreach (Material mat in objectRenderer.materials)
        {
            if (mat == outlineMaterial) return;
        }

        // Crée un tableau avec l'outline ajouté
        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[newMaterials.Length - 1] = outlineMaterial;

        // Applique les nouveaux matériaux
        objectRenderer.materials = newMaterials;
    }

    private void RestoreOriginalMaterials()
    {
        // Réinitialise les matériaux à leur état d'origine
        objectRenderer.materials = originalMaterials;
    }
}
