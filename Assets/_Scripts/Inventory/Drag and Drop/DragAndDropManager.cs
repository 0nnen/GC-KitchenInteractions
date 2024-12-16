using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class ObjectConfig
{
    public GameObject prefab;                  // Prefab de l'objet
    public bool canReceiveChildren = false;   // Peut recevoir des enfants ?
    public bool hasDoor = false;              // Possède une porte ?
    public bool isMovable = true;             // L'objet est-il déplaçable ?

    [Tooltip("Zone pour recevoir des enfants. Visible si 'canReceiveChildren' est activé.")]
    public Collider dropZoneCollider;

    [Tooltip("La porte à manipuler. Visible si 'hasDoor' est activé.")]
    public Transform doorTransform;

    [Tooltip("Rotation sur l'axe X. Visible si 'hasDoor' est activé.")]
    public bool rotateDoorOnX = false;

    [Tooltip("Rotation sur l'axe Y. Visible si 'hasDoor' est activé.")]
    public bool rotateDoorOnY = true;

    [Tooltip("Limite de rotation de la porte en degrés. Visible si 'hasDoor' est activé.")]
    public float doorRotationLimit = 120f;
}

public class DragAndDropManager : MonoBehaviour
{
    [Header("Références Générales")]
    [Tooltip("Caméra utilisée pour le Raycast et la gestion du drag.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Transform parent temporaire pendant le drag.")]
    [SerializeField] private Transform holdingParent;

    [Tooltip("Transform parent par défaut après le relâchement.")]
    [SerializeField] private Transform releasedParent;


    [Space(10)]
    [Header("Matériaux et Couleurs")]
    [Tooltip("Matériau utilisé pour afficher un outline.")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("Couleur affichée lorsque l'objet est survolé.")]
    [SerializeField] private Color hoverColor = Color.green;

    [Tooltip("Couleur affichée lorsque l'objet est en cours de drag.")]
    [SerializeField] private Color dragColor = Color.yellow;


    [Space(10)]
    [Header("Paramètres de Drag")]
    [Tooltip("Distance par défaut entre l'objet et la caméra pendant le drag.")]
    [Range(1f, 10f)]
    [SerializeField] private float dragDepth = 2f;

    [Tooltip("Distance minimale entre l'objet et la caméra.")]
    [Range(0.1f, 10f)]
    [SerializeField] private float minDragDepth = 1f;

    [Tooltip("Distance maximale entre l'objet et la caméra.")]
    [Range(1f, 20f)]
    [SerializeField] private float maxDragDepth = 5f;

    [Tooltip("Sensibilité au défilement de la molette pendant le drag.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float scrollSensitivity = 0.5f;


    [Space(10)]
    [Header("Paramètres de Rotation")]
    [SerializeField] private float rotationSpeed = 5f; // Vitesse de rotation


    [Space(10)]
    [Header("Zone de Dépose")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float overlapSphereRadius = 0.5f;


    [Header("Configurations d'Objets")]
    [Tooltip("Liste des objets configurables pour le drag-and-drop.")]
    [SerializeField] private List<ObjectConfig> objectConfigs;

    [Header("Debugging")]
    [Tooltip("Afficher des gizmos pour la détection de drop.")]
    [SerializeField] private bool showGizmos = true;

    [ContextMenu("Reset Drag Depth")]
    private void ResetDragDepth()
    {
        dragDepth = 2f;
        Debug.Log("Drag depth reset to default value.");
    }

    private GameObject selectedObject;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Material[] originalMaterials;
    private ObjectConfig currentConfig;
    private float currentDoorRotation = 0f;

    public static DragAndDropManager Instance { get; private set; }
    public bool IsMovable { get; private set; }

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

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (holdingParent == null || releasedParent == null)
            Debug.LogError("HoldingParent ou ReleasedParent n'est pas assigné !");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDragging();
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(1)) // Rotation avec clic droit
            {
                RotateObject();
            }
            else if (currentConfig != null && currentConfig.hasDoor && currentConfig.doorTransform != null)
            {
                RotateDoor();
            }
            else
            {
                DragObject();
            }

            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }
    }

    // Initialisation avec l'état de l'objet
    private void Initialize(GameObject item)
    {
        if (item.TryGetComponent<ObjectConfig>(out ObjectConfig config))
        {
            IsMovable = config.isMovable;
        }
    }

    private void TryStartDragging()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            Debug.Log($"Détection d'un objet interactif : {hit.collider.gameObject.name}");
            foreach (var config in objectConfigs)
            {
                if (hit.collider.gameObject == config.prefab)
                {
                    if (!config.isMovable)
                    {
                        Debug.LogWarning($"{config.prefab.name} n'est pas déplaçable.");
                        return;
                    }

                    selectedObject = config.prefab;
                    currentConfig = config;
                    dragDepth = Vector3.Distance(mainCamera.transform.position, selectedObject.transform.position);
                    StartDragging(hit.point);
                    return;
                }
            }
        }
        else
        {
            Debug.Log("Aucun objet interactif détecté sous la souris.");
        }
    }

    private void StartDragging(Vector3 hitPoint)
    {
        isDragging = true;

        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterials = renderer.materials;
            var materials = new List<Material>(originalMaterials) { outlineMaterial };
            renderer.materials = materials.ToArray();
            outlineMaterial.SetColor("_Color", dragColor);
        }

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = true;

        selectedObject.transform.SetParent(holdingParent);
    }

    private void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = mainCamera.transform.position + ray.direction.normalized * dragDepth;
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetPosition, 0.2f);
    }

    private void RotateObject()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        selectedObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World);
        selectedObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World);
    }

    private void RotateDoor()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;

        float newRotation = Mathf.Clamp(currentDoorRotation + mouseX, 0, currentConfig.doorRotationLimit);
        float rotationStep = newRotation - currentDoorRotation;

        if (currentConfig.rotateDoorOnX)
        {
            currentConfig.doorTransform.Rotate(Vector3.right, rotationStep, Space.Self);
        }
        else if (currentConfig.rotateDoorOnY)
        {
            currentConfig.doorTransform.Rotate(Vector3.up, rotationStep, Space.Self);
        }

        currentDoorRotation = newRotation;
    }



    private void StopDragging()
    {
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = false;

        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null && originalMaterials != null)
            renderer.materials = originalMaterials;

        // Vérifie si l'objet est relâché dans l'inventaire
        if (InventoryUI.Instance.IsPointerOverInventoryArea())
        {
            Inventory.Instance.AddToInventory(selectedObject);
        }
        else if (currentConfig.canReceiveChildren)
        {
            // Vérifie si l'objet est relâché sur un objet qui peut recevoir des enfants
            Collider[] colliders = Physics.OverlapSphere(selectedObject.transform.position, overlapSphereRadius, interactableLayer);
            foreach (var collider in colliders)
            {
                if (collider == currentConfig.dropZoneCollider)
                {
                    selectedObject.transform.SetParent(collider.transform);
                    Debug.Log($"{selectedObject.name} est devenu un enfant de {collider.name}");
                    selectedObject = null;
                    isDragging = false;
                    return;
                }
            }
        }
        else
        {
            // Relâche l'objet dans la scène
            selectedObject.transform.SetParent(releasedParent);
            Debug.Log($"{selectedObject.name} relâché dans la scène.");
        }

        selectedObject = null;
        currentConfig = null;
        isDragging = false;
    }

    private bool IsPointerOverInventory()
    {
        if (InventoryUI.Instance == null) return false;

        Vector2 localMousePosition;
        RectTransform inventoryRect = InventoryUI.Instance.InventoryArea; // Utilisation de la propriété InventoryArea

        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            inventoryRect,
            Input.mousePosition,
            null,
            out localMousePosition);

        return inventoryRect.rect.Contains(localMousePosition);
    }



    private void AddToInventory()
    {
        Inventory.Instance.AddToInventory(selectedObject);
        Debug.Log($"{selectedObject.name} ajouté à l'inventaire.");
    }

    private void SetOutlineColor(Color color)
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_Color", color); // Modification via Shader Graph
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || currentConfig == null) return;

        // Vérification si `currentConfig.dropZoneCollider` est nul
        if (currentConfig.canReceiveChildren && currentConfig.dropZoneCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentConfig.dropZoneCollider.transform.position, overlapSphereRadius);
        }

        // Vérification si `currentConfig.prefab` est nul
        if (currentConfig.prefab != null)
        {
            Gizmos.color = Color.cyan;

            if (mainCamera != null)
            {
                Gizmos.DrawLine(mainCamera.transform.position, currentConfig.prefab.transform.position);
            }

            // Affiche un label uniquement si Handles est disponible (éditeur Unity)
        #if UNITY_EDITOR
                    Handles.Label(currentConfig.prefab.transform.position, $"Object: {currentConfig.prefab.name}");
        #endif
        }
    }

    public ObjectConfig GetConfigForPrefab(GameObject prefab)
    {
        return objectConfigs.Find(config => config.prefab == prefab);
    }

}
