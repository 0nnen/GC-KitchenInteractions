using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class FridgeUIManager : MonoBehaviour
{
    public static FridgeUIManager Instance;

    [Header("UI R�f�rences")]
    [SerializeField] private GameObject fridgeUI;       // Canvas de l'interface du frigo
    [SerializeField] private Transform fridgeGrid;      // Grille des slots du frigo
    [SerializeField] private GameObject fridgeSlotPrefab; // Prefab pour un slot du frigo
    [SerializeField] private RectTransform fridgeArea;  // Zone du frigo
    public RectTransform FridgeArea => fridgeArea;

    [Header("Param�tres")]
    [SerializeField] private int maxFridgeSlots = 12;   // Nombre maximum de slots dans le frigo

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
            Debug.LogError("La zone du frigo (fridgeArea) n'est pas assign�e !");
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
            Debug.LogWarning($"{item.name} est d�j� dans le frigo !");
            return;
        }

        if (fridgeSlotPrefab == null || fridgeGrid == null)
        {
            Debug.LogError("FridgeUIManager : fridgeSlotPrefab ou fridgeGrid n'est pas assign� !");
            return;
        }

        // Supprimez l'objet de l'inventaire
        Inventory.Instance.RemoveFromInventory(item);

        // Cr�er un slot pour l'objet
        GameObject slot = Instantiate(fridgeSlotPrefab, fridgeGrid);

        // Ajouter un mapping entre l'item et son slot
        fridgeItemSlotMapping[item] = slot;

        // Configurer le slot avec la texture existante
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            RenderTexture renderTexture = InventoryUI.Instance.GetItemRenderTexture(item);
            if (renderTexture != null)
            {
                slotImage.texture = renderTexture; // R�utilise la RenderTexture existante
                Debug.Log($"{item.name} : Texture existante appliqu�e.");
            }
            else
            {
                Debug.LogWarning($"AddToFridge : Pas de texture trouv�e pour {item.name} !");
            }
        }

        Debug.Log($"{item.name} ajout� au frigo.");
        item.SetActive(false); // D�sactiver l'objet dans la sc�ne
    }

    public void RemoveFromFridge(GameObject item)
    {
        if (fridgeItemSlotMapping.TryGetValue(item, out GameObject slot))
        {
            if (slot != null)
            {
                slot.SetActive(false); // D�sactiver au lieu de d�truire
            }

            fridgeItemSlotMapping.Remove(item);
            Inventory.Instance.AddToInventory(item); // Ajouter � l'inventaire
            Debug.Log($"{item.name} retir� du frigo et ajout� � l'inventaire.");
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans le frigo !");
        }
    }



    private void ClearEventSystemTarget(GameObject target)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == target)
        {
            EventSystem.current.SetSelectedGameObject(null);
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

            // V�rifiez si l'objet cliqu� est valide et pas d�truit
            if (!IsValidGameObject(clickedObject))
            {
                Debug.LogWarning("TryHandleItemClick : Un objet cliqu� est null ou d�truit !");
                continue;
            }

            // Gestion pour l'inventaire
            if (InventoryUI.Instance != null && clickedObject.transform.IsChildOf(InventoryUI.Instance.InventoryArea))
            {
                GameObject item = InventoryUI.Instance.GetItemFromSlot(clickedObject);

                if (item != null && !item.Equals(null)) // V�rifiez si l'item est toujours valide
                {
                    Inventory.Instance.RemoveFromInventory(item);
                    AddToFridge(item); // Transf�rer au frigo
                }
                return;
            }

            // Gestion pour le frigo
            if (clickedObject.transform.IsChildOf(fridgeGrid))
            {
                GameObject item = GetItemFromFridgeSlot(clickedObject);

                if (item != null && !item.Equals(null)) // V�rifiez si l'item est toujours valide
                {
                    RemoveFromFridge(item); // Transf�rer � l'inventaire
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

}
