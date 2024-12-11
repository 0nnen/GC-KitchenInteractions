using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventaire UI")]
    [SerializeField] private Transform inventoryGrid; // Parent des slots
    [SerializeField] private GameObject inventorySlotPrefab; // Prefab d'un slot
    private int maxInventorySize = 12; // Taille maximale de l'inventaire

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddToInventory(GameObject item)
    {
        // V�rifiez que l'inventaire a de la place
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein !");
            return;
        }

        // Ajouter l'objet � l'UI
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // D�placer l'objet sous le slot
        item.transform.SetParent(slot.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        // Ajoutez un bouton pour retirer l'objet
        Button button = slot.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => RemoveFromInventory(item, slot));
        }

        // D�sactiver l'objet dans la sc�ne
        item.SetActive(false);

        Debug.Log($"{item.name} ajout� � l'inventaire.");
    }

    private void RemoveFromInventory(GameObject item, GameObject slot)
    {
        Inventory.Instance.RemoveFromInventory(item);

        // D�truire le slot apr�s retrait
        Destroy(slot);
    }
}
