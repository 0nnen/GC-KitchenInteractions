using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventaire UI")]
    [SerializeField] private Transform inventoryGrid; // Parent des slots
    [SerializeField] private GameObject inventorySlotPrefab; // Prefab d'un slot
    [SerializeField] private Camera inventoryCamera; // Caméra dédiée pour les previews
    [SerializeField] private RectTransform inventoryArea; // Zone de l'inventaire
    public RectTransform InventoryArea => inventoryArea; // Propriété publique pour accéder à InventoryArea

    private int maxInventorySize = 9; // Taille maximale de l'inventaire

    private Dictionary<GameObject, GameObject> itemSlotMapping = new Dictionary<GameObject, GameObject>();

    private InputAction clickAction; // Pour détecter les clics
    private InputAction pointAction; // Pour la position de la souris


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

        if (inventoryArea == null)
        {
            Debug.LogError("La zone de l'inventaire (inventoryArea) n'est pas assignée !");
        }

        // Initialisation des actions Input System
        clickAction = new InputAction("Click", binding: "<Mouse>/leftButton");
        pointAction = new InputAction("Point", binding: "<Mouse>/position");

        clickAction.performed += OnClickPerformed;

        clickAction.Enable();
        pointAction.Enable();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Préparer un Raycast pour détecter l'objet cliqué
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.Log("Aucun élément UI détecté sous la souris.");
            }

            foreach (var result in results)
            {
                Debug.Log($"Raycast sur : {result.gameObject.name}");

                // Vérifie si le clic est sur un slot de l'inventaire
                if (result.gameObject.transform.IsChildOf(inventoryGrid))
                {
                    Debug.Log($"Clic sur un élément de l'inventaire : {result.gameObject.name}");

                    // Obtenir l'objet lié au slot cliqué
                    GameObject item = GetItemFromSlot(result.gameObject);
                    if (item != null)
                    {
                        Inventory.Instance.RemoveFromInventory(item);
                        Debug.Log($"Objet {item.name} retiré de l'inventaire.");
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun objet associé à ce slot : {result.gameObject.name}");
                    }
                    break;
                }
            }
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();

        // Préparer le Raycast pour détecter l'objet cliqué dans l'UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Vérifie si le clic est sur un slot de l'inventaire
            if (result.gameObject.transform.IsChildOf(inventoryGrid))
            {
                Debug.Log($"Clic sur un slot de l'inventaire : {result.gameObject.name}");

                // Récupérer l'objet lié au slot
                GameObject item = GetItemFromSlot(result.gameObject);
                if (item != null)
                {
                    // Retirer l'objet de l'inventaire et le replacer dans la scène
                    Inventory.Instance.RemoveFromInventory(item);
                    Debug.Log($"Objet {item.name} retiré de l'inventaire.");
                }
                else
                {
                    Debug.LogWarning("Slot cliqué, mais aucun objet trouvé !");
                }
                break;
            }
        }
    }

    private void OnDestroy()
    {
        clickAction.Disable();
        pointAction.Disable();
    }


    public void AddToInventory(GameObject item)
    {
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein ! Impossible d'ajouter un nouvel objet.");
            return;
        }

        // Créer un slot dans la grille
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // Capturer l'aperçu de l'objet pour l'UI
        RenderTexture renderTexture = CreateRenderTexture(item);

        // Associer la Render Texture au RawImage du slot
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            slotImage.texture = renderTexture;
        }

        // Lier l'objet réel au slot via le dictionnaire
        itemSlotMapping[item] = slot;

        Debug.Log($"Objet {item.name} lié au slot {slot.name}.");

        // Désactiver l'objet réel dans le monde
        item.SetActive(false);
    }



    public void MoveObjectToScene(GameObject item)
    {
        if (itemSlotMapping.ContainsKey(item))
        {
            // Détruire le slot visuel
            Destroy(itemSlotMapping[item]);
            itemSlotMapping.Remove(item);

            // Réactiver l'objet réel
            item.SetActive(true);
            item.transform.SetParent(null);

            // Positionner l'objet devant la caméra
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                item.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
                item.transform.rotation = Quaternion.identity;
            }

            Debug.Log($"{item.name} retiré de l'inventaire et replacé dans la scène.");
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

    private void MoveObjectToInventorySlot(GameObject item, GameObject slot)
    {
        item.transform.SetParent(slot.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.transform.localRotation = Quaternion.identity;
    }

    public bool IsPointerOverInventoryArea()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject.transform.IsChildOf(inventoryArea))
            {
                return true;
            }
        }

        return false;
    }

    private GameObject GetItemFromSlot(GameObject slot)
    {
        // Chercher directement l'objet lié dans le dictionnaire
        foreach (var pair in itemSlotMapping)
        {
            if (pair.Value == slot) // Le slot correspond à un élément du dictionnaire
            {
                return pair.Key; // Retourner l'objet associé
            }
        }

        Debug.LogWarning($"Aucun objet trouvé pour le slot : {slot.name}");
        return null;
    }


}
