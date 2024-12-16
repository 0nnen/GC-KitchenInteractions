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
    [SerializeField] private Camera inventoryCamera; // Cam�ra d�di�e pour les previews
    [SerializeField] private RectTransform inventoryArea; // Zone de l'inventaire
    public RectTransform InventoryArea => inventoryArea; // Propri�t� publique pour acc�der � InventoryArea

    private int maxInventorySize = 9; // Taille maximale de l'inventaire

    private Dictionary<GameObject, GameObject> itemSlotMapping = new Dictionary<GameObject, GameObject>();

    private InputAction clickAction; // Pour d�tecter les clics
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
            Debug.LogError("La zone de l'inventaire (inventoryArea) n'est pas assign�e !");
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
            // Pr�parer un Raycast pour d�tecter l'objet cliqu�
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count == 0)
            {
                Debug.Log("Aucun �l�ment UI d�tect� sous la souris.");
            }

            foreach (var result in results)
            {
                Debug.Log($"Raycast sur : {result.gameObject.name}");

                // V�rifie si le clic est sur un slot de l'inventaire
                if (result.gameObject.transform.IsChildOf(inventoryGrid))
                {
                    Debug.Log($"Clic sur un �l�ment de l'inventaire : {result.gameObject.name}");

                    // Obtenir l'objet li� au slot cliqu�
                    GameObject item = GetItemFromSlot(result.gameObject);
                    if (item != null)
                    {
                        Inventory.Instance.RemoveFromInventory(item);
                        Debug.Log($"Objet {item.name} retir� de l'inventaire.");
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun objet associ� � ce slot : {result.gameObject.name}");
                    }
                    break;
                }
            }
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Vector2 mousePosition = pointAction.ReadValue<Vector2>();

        // Pr�parer le Raycast pour d�tecter l'objet cliqu� dans l'UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // V�rifie si le clic est sur un slot de l'inventaire
            if (result.gameObject.transform.IsChildOf(inventoryGrid))
            {
                Debug.Log($"Clic sur un slot de l'inventaire : {result.gameObject.name}");

                // R�cup�rer l'objet li� au slot
                GameObject item = GetItemFromSlot(result.gameObject);
                if (item != null)
                {
                    // Retirer l'objet de l'inventaire et le replacer dans la sc�ne
                    Inventory.Instance.RemoveFromInventory(item);
                    Debug.Log($"Objet {item.name} retir� de l'inventaire.");
                }
                else
                {
                    Debug.LogWarning("Slot cliqu�, mais aucun objet trouv� !");
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

        // Cr�er un slot dans la grille
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);

        // Capturer l'aper�u de l'objet pour l'UI
        RenderTexture renderTexture = CreateRenderTexture(item);

        // Associer la Render Texture au RawImage du slot
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null)
        {
            slotImage.texture = renderTexture;
        }

        // Lier l'objet r�el au slot via le dictionnaire
        itemSlotMapping[item] = slot;

        Debug.Log($"Objet {item.name} li� au slot {slot.name}.");

        // D�sactiver l'objet r�el dans le monde
        item.SetActive(false);
    }



    public void MoveObjectToScene(GameObject item)
    {
        if (itemSlotMapping.ContainsKey(item))
        {
            // D�truire le slot visuel
            Destroy(itemSlotMapping[item]);
            itemSlotMapping.Remove(item);

            // R�activer l'objet r�el
            item.SetActive(true);
            item.transform.SetParent(null);

            // Positionner l'objet devant la cam�ra
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                item.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
                item.transform.rotation = Quaternion.identity;
            }

            Debug.Log($"{item.name} retir� de l'inventaire et replac� dans la sc�ne.");
        }
        else
        {
            Debug.LogWarning($"{item.name} n'a pas de slot associ� dans l'inventaire !");
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
        // Chercher directement l'objet li� dans le dictionnaire
        foreach (var pair in itemSlotMapping)
        {
            if (pair.Value == slot) // Le slot correspond � un �l�ment du dictionnaire
            {
                return pair.Key; // Retourner l'objet associ�
            }
        }

        Debug.LogWarning($"Aucun objet trouv� pour le slot : {slot.name}");
        return null;
    }


}
