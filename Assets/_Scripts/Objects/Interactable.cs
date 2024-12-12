using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Surlignage")]
    [SerializeField] private HighlightEffect highlightEffect; // Effet visuel (facultatif)

    /// <summary>
    /// Appel�e lorsque l'objet est cibl�.
    /// </summary>
    public void OnFocused()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(true);
    }

    /// <summary>
    /// Appel�e lorsque l'objet n'est plus cibl�.
    /// </summary>
    public void OnFocusLost()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(false);
    }

    /// <summary>
    /// Appel�e lorsque le joueur interagit avec l'objet.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"{gameObject.name} ramass� !");
        Inventory.Instance.AddToInventory(gameObject);
    }
}