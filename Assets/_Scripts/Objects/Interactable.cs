using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Surlignage")]
    [SerializeField] private HighlightEffect highlightEffect; // Effet visuel (facultatif)

    /// <summary>
    /// Appelée lorsque l'objet est ciblé.
    /// </summary>
    public void OnFocused()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(true);
    }

    /// <summary>
    /// Appelée lorsque l'objet n'est plus ciblé.
    /// </summary>
    public void OnFocusLost()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(false);
    }

    /// <summary>
    /// Appelée lorsque le joueur interagit avec l'objet.
    /// </summary>
    public void Interact()
    {
        Debug.Log($"{gameObject.name} ramassé !");
        Inventory.Instance.AddToInventory(gameObject);
    }
}