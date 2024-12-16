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
    [SerializeField] private Camera inventoryCamera; // Caméra dédiée pour les previews
    [SerializeField] private RectTransform inventoryArea; // Zone de l'inventaire
    public RectTransform InventoryArea => inventoryArea;

    private Dictionary<GameObject, GameObject> itemSlotMapping = new Dictionary<GameObject, GameObject>();
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

        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        RenderTexture renderTexture = CreateRenderTexture(item);

        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            slotImage.texture = renderTexture;
        }

        itemSlotMapping[item] = slot;

        Debug.Log($"{item.name} ajouté à l'inventaire.");

        item.SetActive(false);
    }

    public void MoveObjectToScene(GameObject item)
    {
        if (itemSlotMapping.ContainsKey(item))
        {
            GameObject slot = itemSlotMapping[item];

            // Vérifiez si le slot existe avant de le détruire
            if (slot != null)
            {
                Destroy(slot);
            }

            itemSlotMapping.Remove(item);

            // Réactiver l'objet réel
            if (item != null)
            {
                item.SetActive(true);
                item.transform.SetParent(null);

                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    item.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
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
            if (result.gameObject.transform.IsChildOf(inventoryGrid))
            {
                GameObject slot = result.gameObject;
                GameObject item = GetItemFromSlot(slot);

                if (item != null)
                {
                    Inventory.Instance.RemoveFromInventory(item);
                    Debug.Log($"{item.name} retiré de l'inventaire.");
                }
                else
                {
                    Debug.LogWarning($"Aucun objet trouvé pour le slot {slot.name}.");
                }

                break;
            }
        }
    }

    private GameObject GetItemFromSlot(GameObject slot)
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

}
