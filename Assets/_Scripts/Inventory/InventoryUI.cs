using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventaire UI")]
    [SerializeField] private Transform inventoryGrid; // Parent des slots
    [SerializeField] private GameObject inventorySlotPrefab; // Prefab d'un slot
    private int maxInventorySize = 12; // Taille maximale de l'inventaire

    private GameObject draggedItem; // Objet actuellement en cours de drag
    private GameObject originalSlot; // Slot d'origine de l'objet
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Sauvegarde des tailles d'origine

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddToInventory(GameObject item)
    {
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein !");
            return;
        }

        // Sauvegarder la taille d'origine de l'objet
        if (!originalScales.ContainsKey(item))
        {
            originalScales[item] = item.transform.localScale;
        }

        // Cr�er un slot
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // D�placer l'objet sous le slot
        item.transform.SetParent(slot.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one; // Ajuster au slot d'inventaire

        // Configurer les �v�nements de drag
        EventTrigger trigger = slot.AddComponent<EventTrigger>();

        // Ajouter le d�but du drag
        EventTrigger.Entry dragStart = new EventTrigger.Entry
        {
            eventID = EventTriggerType.BeginDrag
        };
        dragStart.callback.AddListener((data) => OnBeginDrag(item, slot));
        trigger.triggers.Add(dragStart);

        // Ajouter la fin du drag
        EventTrigger.Entry dragEnd = new EventTrigger.Entry
        {
            eventID = EventTriggerType.EndDrag
        };
        dragEnd.callback.AddListener((data) => OnEndDrag(item, slot));
        trigger.triggers.Add(dragEnd);

        // D�sactiver l'objet dans la sc�ne
        item.SetActive(false);

        Debug.Log($"{item.name} ajout� � l'inventaire.");
    }

    private void OnBeginDrag(GameObject item, GameObject slot)
    {
        draggedItem = item;
        originalSlot = slot;

        // R�activer l'objet pour le drag
        item.SetActive(true);
        item.transform.localScale = originalScales[item]; // Restaurer la taille d'origine

        Debug.Log($"D�but du drag de : {item.name}");
    }

    private void OnEndDrag(GameObject item, GameObject slot)
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Rel�cher l'objet dans la sc�ne
            Inventory.Instance.RemoveFromInventory(item);
            item.transform.SetParent(null); // Retirer le parent du slot

            // Positionner l'objet devant la cam�ra
            item.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;

            Debug.Log($"Objet rel�ch� dans la sc�ne : {item.name}");
        }
        else
        {
            // Si l'objet est rel�ch� dans l'UI, le replacer dans le slot
            item.SetActive(false);
            item.transform.SetParent(originalSlot.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one; // Ajuster au slot
            Debug.Log($"Objet remis dans l'inventaire : {item.name}");
        }

        draggedItem = null;
        originalSlot = null;
    }
}
