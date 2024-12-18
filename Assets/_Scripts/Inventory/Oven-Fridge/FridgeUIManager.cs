using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class FridgeUIManager : MonoBehaviour
{
    public static FridgeUIManager Instance;

    [Header("UI Références")]
    [SerializeField] private GameObject fridgeUI;       // Canvas de l'interface du frigo
    [SerializeField] private Transform fridgeGrid;      // Grille des slots du frigo
    [SerializeField] private GameObject fridgeSlotPrefab; // Prefab pour un slot du frigo
    [SerializeField] private RectTransform fridgeArea;  // Zone du frigo
    public RectTransform FridgeArea => fridgeArea;

    [Header("Paramètres")]
    [SerializeField] private int maxFridgeSlots = 9;   // Nombre maximum de slots dans le frigo

    private Dictionary<GameObject, GameObject> fridgeItemSlotMapping = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (fridgeArea == null)
            Debug.LogError("La zone du frigo (fridgeArea) n'est pas assignée !");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryHandleItemClick();
        }
    }

    public bool IsFridgeOpen()
    {
        return fridgeUI.activeSelf;
    }

    public void AddToFridge(GameObject item)
    {
        if (fridgeItemSlotMapping.ContainsKey(item))
        {
            Debug.LogWarning($"{item.name} est déjà dans le frigo !");
            return;
        }

        if (fridgeSlotPrefab == null || fridgeGrid == null)
        {
            Debug.LogError("FridgeUIManager : fridgeSlotPrefab ou fridgeGrid n'est pas assigné !");
            return;
        }

        // Récupérer la configuration de l'objet
        ObjectConfig config = DragAndDropManager.Instance.GetConfigForPrefab(item);
        if (config == null || config.ingredientData == null)
        {
            Debug.LogError($"AddToFridge : Aucun IngredientData trouvé pour {item.name} !");
            return;
        }

        // Supprimer l'objet de l'inventaire
        Inventory.Instance.RemoveFromInventory(item);

        // Créer un slot pour l'objet
        GameObject slot = Instantiate(fridgeSlotPrefab, fridgeGrid);

        // Ajouter un mapping entre l'item et son slot
        fridgeItemSlotMapping[item] = slot;

        // Configurer le slot avec les données du ScriptableObject
        SetupSlotWithIngredientData(slot, config.ingredientData);

        Debug.Log($"{item.name} ajouté au frigo.");
        item.SetActive(false); // Désactiver l'objet dans la scène
    }

    public void RemoveFromFridge(GameObject item)
    {
        if (fridgeItemSlotMapping.TryGetValue(item, out GameObject slot))
        {
            if (slot != null)
            {
                slot.SetActive(false); // Désactiver au lieu de détruire
            }

            fridgeItemSlotMapping.Remove(item);
            Inventory.Instance.AddToInventory(item); // Ajouter à l'inventaire
            Debug.Log($"{item.name} retiré du frigo et ajouté à l'inventaire.");
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans le frigo !");
        }
    }

    private void TryHandleItemClick()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GameObject clickedObject = result.gameObject;

            // Vérifiez si l'objet cliqué est valide et pas détruit
            if (!IsValidGameObject(clickedObject))
            {
                Debug.LogWarning("TryHandleItemClick : Un objet cliqué est null ou détruit !");
                continue;
            }

            // Gestion pour l'inventaire
            if (InventoryUI.Instance != null && clickedObject.transform.IsChildOf(InventoryUI.Instance.InventoryArea))
            {
                GameObject item = InventoryUI.Instance.GetItemFromSlot(clickedObject);

                if (item != null && !item.Equals(null)) // Vérifiez si l'item est toujours valide
                {
                    Inventory.Instance.RemoveFromInventory(item);
                    AddToFridge(item); // Transférer au frigo
                }
                return;
            }

            // Gestion pour le frigo
            if (clickedObject.transform.IsChildOf(fridgeGrid))
            {
                GameObject item = GetItemFromFridgeSlot(clickedObject);

                if (item != null && !item.Equals(null)) // Vérifiez si l'item est toujours valide
                {
                    RemoveFromFridge(item); // Transférer à l'inventaire
                }
                return;
            }
        }
    }

    private GameObject GetItemFromFridgeSlot(GameObject slot)
    {
        foreach (var pair in fridgeItemSlotMapping)
        {
            if (pair.Value == slot)
            {
                return pair.Key;
            }
        }
        return null;
    }

    private bool IsValidGameObject(GameObject obj)
    {
        return obj != null && !obj.Equals(null);
    }

    private void SetupSlotWithIngredientData(GameObject slot, IngredientData data)
    {
        // Configure l'image
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null && data.ingredientSprite != null)
        {
            slotImage.texture = data.ingredientSprite.texture;
        }

        // Configure d'autres détails si nécessaire (comme un tooltip)
        // Exemple : Ajouter un événement de survol pour afficher le nom et la description
    }
}
