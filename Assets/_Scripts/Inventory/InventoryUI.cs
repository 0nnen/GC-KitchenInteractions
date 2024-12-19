using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventaire UI")]
    [SerializeField] private Transform inventoryGrid; // Parent des slots
    [SerializeField] private GameObject inventorySlotPrefab; // Prefab d'un slot
    [SerializeField] private Camera inventoryCamera; // Caméra dédiée pour les previews
    [SerializeField] private RectTransform inventoryArea; // Zone de l'inventaire

    public Camera InventoryCamera => inventoryCamera;
    public RectTransform InventoryArea => inventoryArea;

    private Dictionary<GameObject, GameObject> itemSlotMapping = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, RenderTexture> itemTextures = new Dictionary<GameObject, RenderTexture>();


    private int maxInventorySize = 9;

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

        if (inventoryCamera == null)
            Debug.LogError("La caméra d'inventaire n'est pas assignée !");
        if (inventoryArea == null)
            Debug.LogError("La zone de l'inventaire (inventoryArea) n'est pas assignée !");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryRemoveItemFromInventory();
        }
    }

    public void AddToInventory(GameObject item)
    {
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein ! Impossible d'ajouter un nouvel objet.");
            return;
        }

        if (itemSlotMapping.ContainsKey(item))
        {
            Debug.LogWarning($"{item.name} est déjà dans l'inventaire (via le slot mapping) !");
            return;
        }

        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        RenderTexture renderTexture;
        if (itemTextures.ContainsKey(item))
        {
            renderTexture = itemTextures[item];
        }
        else
        {
            renderTexture = CreateRenderTexture(item);
            itemTextures[item] = renderTexture;
        }

        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            slotImage.texture = renderTexture;
        }

        itemSlotMapping[item] = slot;
        item.SetActive(false);
        Debug.Log($"{item.name} ajouté à l'inventaire (UI).");
    }

    private void ClearEventSystemTarget(GameObject target)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == target)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }


    public void MoveObjectToScene(GameObject item)
    {
        if (itemSlotMapping.ContainsKey(item))
        {
            GameObject slot = itemSlotMapping[item];

            // Désactiver le slot et nettoyer la référence dans l'EventSystem
            if (slot != null)
            {
                ClearEventSystemTarget(slot); // Nettoyage de l'EventSystem
                slot.SetActive(false);
                Destroy(slot, 0.1f); // Détruire avec un léger délai
            }
            itemSlotMapping.Remove(item);

            // Réactiver l'objet dans la scène
            if (item != null)
            {
                item.SetActive(true);
                item.transform.SetParent(null);

                // Positionner l'objet devant la caméra
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    item.transform.position = mainCam.transform.position + mainCam.transform.forward * 0.1f;
                    item.transform.rotation = Quaternion.identity;
                }

                Debug.Log($"{item.name} retiré de l'inventaire et replacé dans la scène.");
            }
        }
        else
        {
            Debug.LogWarning($"{item.name} n'a pas de slot associé dans l'inventaire !");
        }
    }


    private RenderTexture CreateRenderTexture(GameObject item)
    {
        RenderTexture renderTexture = new RenderTexture(256, 256, 16);
        renderTexture.Create();

        inventoryCamera.targetTexture = renderTexture;

        Bounds bounds = item.GetComponent<Renderer>().bounds;
        inventoryCamera.transform.position = bounds.center + Vector3.back * bounds.size.z * 2 + Vector3.up * bounds.size.y;
        inventoryCamera.transform.LookAt(bounds.center);

        inventoryCamera.Render();

        inventoryCamera.targetTexture = null;

        return renderTexture;
    }

    public RenderTexture GetItemRenderTexture(GameObject item)
    {
        if (itemTextures.TryGetValue(item, out RenderTexture renderTexture))
        {
            return renderTexture; // Retourne la RenderTexture existante
        }

        Debug.LogWarning($"GetItemRenderTexture : Aucune texture trouvée pour {item.name}");
        return null;
    }


    public GameObject GetItemFromSlot(GameObject slot)
    {
        foreach (var pair in itemSlotMapping)
        {
            if (pair.Value == slot)
            {
                return pair.Key;
            }
        }

        return null;
    }

    private void TryRemoveItemFromInventory()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GameObject slot = result.gameObject;

            // Vérifiez si le slot appartient à l'inventaire
            if (slot.transform.IsChildOf(inventoryGrid))
            {
                ClearEventSystemTarget(slot); // Nettoyer l'EventSystem
                GameObject item = GetItemFromSlot(slot);

                if (item != null)
                {
                    if (FridgeUIManager.Instance != null && FridgeUIManager.Instance.IsFridgeOpen())
                    {
                        FridgeUIManager.Instance.AddToFridge(item);
                        Inventory.Instance.RemoveFromInventory(item);
                        Debug.Log($"{item.name} transféré vers le frigo.");
                    }
                    else
                    {
                        Inventory.Instance.RemoveFromInventory(item);
                        Debug.Log($"{item.name} retiré de l'inventaire et placé dans la scène.");
                    }
                }
                break;
            }
        }
    }


    public bool IsPointerOverInventoryArea()
    {
        // Préparer un PointerEventData pour détecter la position de la souris
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Effectuer un Raycast sur les objets UI
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        // Vérifier si l'un des objets sous le curseur appartient à l'inventaire
        foreach (var result in raycastResults)
        {
            if (result.gameObject.transform.IsChildOf(inventoryArea))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidSlot(GameObject slot)
    {
        return slot != null && !slot.Equals(null);
    }

    public bool IsItemInUI(GameObject item)
    {
        return itemSlotMapping.ContainsKey(item);
    }

}
