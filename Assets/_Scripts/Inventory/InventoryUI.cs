using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventaire UI")]
    [SerializeField] private Transform inventoryGrid; // Parent pour les slots dans le Canvas
    [SerializeField] private GameObject inventorySlotPrefab; // Prefab de slot d'inventaire

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Ajouter un objet à l'inventaire
    public void AddToInventory(GameObject item)
    {
        // Crée un slot d'inventaire
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // Placer l'objet dans le slot
        item.transform.SetParent(slot.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        // Désactiver les composants inutiles pour un objet dans l'inventaire
        if (item.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Destroy(rb);
        }
        if (item.TryGetComponent<Collider>(out Collider collider))
        {
            Destroy(collider);
        }
    }
}
