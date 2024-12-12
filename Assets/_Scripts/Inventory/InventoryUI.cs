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
    [SerializeField] private Camera inventoryCamera; // Cam�ra d�di�e pour les previews
    [SerializeField] private RectTransform inventoryArea; // Zone de l'inventaire
    private int maxInventorySize = 9; // Taille maximale de l'inventaire

    private Dictionary<GameObject, RenderTexture> renderTextures = new Dictionary<GameObject, RenderTexture>();
    private GameObject draggedItem = null; // R�f�rence � l'objet en cours de drag

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
        {
            Debug.LogError("La cam�ra d'inventaire n'est pas assign�e !");
        }

        if (inventoryArea == null)
        {
            Debug.LogError("La zone de l'inventaire (inventoryArea) n'est pas assign�e !");
        }
    }

    public void AddToInventory(GameObject item)
    {
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein ! Impossible d'ajouter un nouvel objet.");
            return;
        }

        // Cr�er un slot
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // Capturer l'aper�u de l'objet
        RenderTexture renderTexture = CreateRenderTexture(item);

        // Associer la Render Texture au RawImage du slot
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            slotImage.texture = renderTexture;
        }

        // Ajouter des �v�nements de drag-and-drop
        AddDragAndDropEvents(slot, item);

        // D�sactiver l'objet apr�s capture
        item.SetActive(false);

        Debug.Log($"{item.name} ajout� � l'inventaire avec aper�u.");
    }

    private RenderTexture CreateRenderTexture(GameObject item)
    {
        RenderTexture renderTexture = new RenderTexture(256, 256, 16);
        renderTexture.Create();

        // Configurer la cam�ra pour capturer l'objet
        inventoryCamera.targetTexture = renderTexture;

        // Positionner la cam�ra face � l'objet
        Bounds bounds = item.GetComponent<Renderer>().bounds;
        inventoryCamera.transform.position = bounds.center + Vector3.back * bounds.size.z * 2 + Vector3.up * bounds.size.y;
        inventoryCamera.transform.LookAt(bounds.center);

        // Effectuer le rendu
        inventoryCamera.Render();

        // Lib�rer la cam�ra de la Render Texture
        inventoryCamera.targetTexture = null;

        // Sauvegarder la Render Texture
        renderTextures[item] = renderTexture;

        return renderTexture;
    }

    private void AddDragAndDropEvents(GameObject slot, GameObject item)
    {
        EventTrigger trigger = slot.AddComponent<EventTrigger>();

        // D�but du drag
        EventTrigger.Entry dragStart = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        dragStart.callback.AddListener((data) => OnBeginDrag(item, slot));
        trigger.triggers.Add(dragStart);

        // Fin du drag
        EventTrigger.Entry dragEnd = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        dragEnd.callback.AddListener((data) => OnEndDrag(item, slot));
        trigger.triggers.Add(dragEnd);
    }

    private void OnBeginDrag(GameObject item, GameObject slot)
    {
        // Activer imm�diatement l'objet pour le drag
        draggedItem = item;
        draggedItem.SetActive(true);

        // Positionner devant la cam�ra principale pour �viter les t�l�portations
        PlaceObjectInFrontOfPlayer(draggedItem);

        Debug.Log($"D�but du drag de {draggedItem.name}");
    }

    private void OnEndDrag(GameObject item, GameObject slot)
    {
        if (IsPointerOverInventoryArea())
        {
            // Si l'objet est rel�ch� dans la zone d'inventaire
            Debug.Log($"{item.name} rel�ch� dans la zone d'inventaire.");

            // V�rifier si l'inventaire est plein
            if (inventoryGrid.childCount >= maxInventorySize)
            {
                Debug.LogWarning("Inventaire plein ! L'objet n'a pas �t� ajout�.");
                draggedItem = null;
                return;
            }

            // Ajouter un nouveau slot si l'objet n'est pas d�j� dans l'inventaire
            if (!IsItemAlreadyInInventory(item))
            {
                AddToInventory(item);
            }
        }
        else
        {
            // Retirer l'objet de l'inventaire et le laisser suivre la souris
            Inventory.Instance.RemoveFromInventory(item);
            Destroy(slot);

            Debug.Log($"{item.name} rel�ch� dans la sc�ne.");
        }

        draggedItem = null;
    }

    private void Update()
    {
        // Si un objet est en cours de drag, le d�placer avec la souris
        if (draggedItem != null)
        {
            FollowMousePosition(draggedItem);
        }
    }

    private void FollowMousePosition(GameObject item)
    {
        // Convertir la position de la souris en coordonn�es du monde
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            item.transform.position = hit.point;
        }
        else
        {
            // Positionner l'objet � une distance par d�faut si rien n'est sous la souris
            item.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                2f)); // Distance par rapport � la cam�ra
        }
    }

    private void PlaceObjectInFrontOfPlayer(GameObject item)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 positionInFront = mainCamera.transform.position + mainCamera.transform.forward * 2f;
            positionInFront.y = Mathf.Max(1f, positionInFront.y); // Ajuster la hauteur pour �viter les collisions
            item.transform.position = positionInFront;
            item.transform.rotation = Quaternion.identity; // R�initialiser la rotation
        }
    }

    private bool IsPointerOverInventoryArea()
    {
        // V�rifier si la souris est dans la zone d'inventaire
        Vector2 localMousePosition;
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryArea,
            Input.mousePosition,
            null,
            out localMousePosition);

        if (isInside)
        {
            return inventoryArea.rect.Contains(localMousePosition);
        }

        return false;
    }

    private bool IsItemAlreadyInInventory(GameObject item)
    {
        foreach (Transform slot in inventoryGrid)
        {
            if (slot.childCount > 0 && slot.GetChild(0).gameObject == item)
            {
                return true;
            }
        }
        return false;
    }
}
