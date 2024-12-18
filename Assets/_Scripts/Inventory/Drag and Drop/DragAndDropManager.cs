using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class ObjectConfig
{
    public GameObject prefab;                  // Prefab de l'objet
    public bool canReceiveChildren = false;   // Peut recevoir des enfants ?
    public bool hasDoor = false;              // Poss�de une porte ?
    public bool isMovable = true;             // L'objet est-il d�pla�able ?

    [Tooltip("Zone pour recevoir des enfants. Visible si 'canReceiveChildren' est activ�.")]
    public Collider dropZoneCollider;

    [Tooltip("La porte � manipuler. Visible si 'hasDoor' est activ�.")]
    public Transform doorTransform;

    [Tooltip("Rotation sur l'axe X. Visible si 'hasDoor' est activ�.")]
    public bool rotateDoorOnX = false;

    [Tooltip("Rotation sur l'axe Y. Visible si 'hasDoor' est activ�.")]
    public bool rotateDoorOnY = true;

    [Tooltip("Inverser la direction de rotation de la porte.")]
    public bool invertDoorRotation = false;

    [Tooltip("Limite de rotation de la porte en degr�s. Visible si 'hasDoor' est activ�.")]
    public float doorRotationLimit = 120f;
}

public class DragAndDropManager : MonoBehaviour
{
    [Header("R�f�rences G�n�rales")]
    [Tooltip("Cam�ra utilis�e pour le Raycast et la gestion du drag.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Transform parent temporaire pendant le drag.")]
    [SerializeField] private Transform holdingParent;

    [Tooltip("Transform parent par d�faut apr�s le rel�chement.")]
    [SerializeField] private Transform releasedParent;


    [Space(10)]
    [Header("Mat�riaux et Couleurs")]
    [Tooltip("Mat�riau utilis� pour afficher un outline.")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("Couleur affich�e lorsque l'objet est survol�.")]
    [SerializeField] private Color hoverColor = Color.green;

    [Tooltip("Couleur affich�e lorsque l'objet est en cours de drag.")]
    [SerializeField] private Color dragColor = Color.yellow;


    [Space(10)]
    [Header("Param�tres de Drag")]
    [Tooltip("Distance par d�faut entre l'objet et la cam�ra pendant le drag.")]
    [Range(0.1f, 5f)]
    [SerializeField] private float dragDepth = 2f;

    [Tooltip("Distance minimale entre l'objet et la cam�ra.")]
    [Range(0.1f, 10f)]
    [SerializeField] private float minDragDepth = 1f;

    [Tooltip("Distance maximale entre l'objet et la cam�ra.")]
    [Range(1f, 20f)]
    [SerializeField] private float maxDragDepth = 5f;

    [Tooltip("Sensibilit� au d�filement de la molette pendant le drag.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float scrollSensitivity = 0.5f;

    [Space(2)]
    [Header("Smooth Drag")]
    [Tooltip("Vitesse de lissage pendant le drag (valeurs basses pour un mouvement plus doux).")]
    [SerializeField] private float smoothSpeed = 8f;

    [Space(10)]
    [Header("Param�tres de Rotation")]
    [SerializeField] private float rotationSpeed = 5f; // Vitesse de rotation


    [Space(10)]
    [Header("Zone de D�pose")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float overlapSphereRadius = 0.5f;


    [Header("Configurations d'Objets")]
    [Tooltip("Liste des objets configurables pour le drag-and-drop.")]
    [SerializeField] private List<ObjectConfig> objectConfigs;

    [Header("Debugging")]
    [Tooltip("Afficher des gizmos pour la d�tection de drop.")]
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
    private Material[] doorOriginalMaterials; // Mat�riaux d'origine de la porte
    private Renderer doorRenderer; // Renderer de la porte pour appliquer l'outline
    private GameObject hoveredObject; // R�f�rence � l'objet survol�
    private Renderer hoveredRenderer; // Renderer de l'objet survol�
    private Material[] hoveredOriginalMaterials; // Mat�riaux originaux de l'objet survol�


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
            if (selectedObject != null)
            {
                selectedObject.SetActive(false); // D�sactiver l'objet pour �viter les interactions
                Destroy(selectedObject, 0.1f);   // D�truire apr�s un d�lai pour laisser l'Input System se mettre � jour
                selectedObject = null;          // Nettoyer la r�f�rence
            }
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (holdingParent == null || releasedParent == null)
            Debug.LogError("HoldingParent ou ReleasedParent n'est pas assign� !");
    }

    private void Update()
    {
        HandleHover(); // Gestion du survol

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


    // Initialisation avec l'�tat de l'objet
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
            GameObject hitObject = hit.collider.gameObject;

            foreach (var config in objectConfigs)
            {
                // V�rifier si la porte a �t� touch�e directement
                if (config.doorTransform != null && hitObject == config.doorTransform.gameObject)
                {
                    Debug.Log($"S�lection directe de la porte : {hitObject.name}");
                    selectedObject = config.prefab; // S�lectionner le prefab parent
                    currentConfig = config;
                    dragDepth = Vector3.Distance(mainCamera.transform.position, selectedObject.transform.position);
                    StartDragging(hit.point);
                    return;
                }

                // Sinon, v�rifier si c'est le prefab lui-m�me
                if (hitObject == config.prefab)
                {
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
            Debug.Log("Aucun objet interactif d�tect� sous la souris.");
        }
    }


    private void StartDragging(Vector3 hitPoint)
    {
        isDragging = true;

        // Initialiser la distance de drag
        dragDepth = Vector3.Distance(mainCamera.transform.position, selectedObject.transform.position);
        dragDepth = Mathf.Clamp(dragDepth, minDragDepth, maxDragDepth);

        // Appliquer l'outline
        if (currentConfig.hasDoor && currentConfig.doorTransform != null)
        {
            doorRenderer = currentConfig.doorTransform.GetComponent<Renderer>();
            if (doorRenderer != null)
            {
                doorOriginalMaterials = doorRenderer.materials;
                var materials = new List<Material>(doorOriginalMaterials) { outlineMaterial };
                doorRenderer.materials = materials.ToArray();
                outlineMaterial.SetColor("_Color", dragColor);
            }
        }
        else
        {
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalMaterials = renderer.materials;
                var materials = new List<Material>(originalMaterials) { outlineMaterial };
                renderer.materials = materials.ToArray();
                outlineMaterial.SetColor("_Color", dragColor);
            }
        }

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = true;

        selectedObject.transform.SetParent(holdingParent);
    }


    private void DragObject()
    {
        // Gestion du Scroll pour ajuster la profondeur
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon) // Si un scroll est d�tect�
        {
            dragDepth += scroll * scrollSensitivity; // Ajuster la profondeur
            dragDepth = Mathf.Clamp(dragDepth, minDragDepth, maxDragDepth); // Limiter la profondeur
        }

        // Calculer la nouvelle position cible
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = mainCamera.transform.position + ray.direction.normalized * dragDepth;

        // V�rifier les collisions avec un Raycast
        RaycastHit hit;
        Vector3 direction = targetPosition - selectedObject.transform.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(selectedObject.transform.position, direction.normalized, out hit, distance))
        {
            // Limiter la position juste avant la collision
            targetPosition = hit.point - direction.normalized * 0.1f;
        }

        // Appliquer un lissage avec Lerp pour un mouvement fluide
        selectedObject.transform.position = Vector3.Lerp(
            selectedObject.transform.position,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
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

        // Inversion de la direction si n�cessaire
        if (currentConfig.invertDoorRotation)
        {
            mouseX = -mouseX;
        }

        // Calcule la nouvelle rotation
        float newRotation = currentDoorRotation + mouseX;

        // Applique le clamp pour respecter les limites (positives et n�gatives)
        newRotation = Mathf.Clamp(newRotation, 0, currentConfig.doorRotationLimit);

        // Applique la rotation en fonction de l'axe s�lectionn�
        if (currentConfig.rotateDoorOnX)
        {
            currentConfig.doorTransform.localRotation = Quaternion.Euler(newRotation, 0f, 0f);
        }
        else if (currentConfig.rotateDoorOnY)
        {
            currentConfig.doorTransform.localRotation = Quaternion.Euler(0f, newRotation, 0f);
        }

        // Met � jour la valeur de la rotation actuelle
        currentDoorRotation = newRotation;
    }

    private void StopDragging()
    {
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = false;

        // R�initialisation des mat�riaux
        if (currentConfig.hasDoor && doorRenderer != null)
        {
            doorRenderer.materials = doorOriginalMaterials;
            doorRenderer = null;
        }
        else
        {
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
                renderer.materials = originalMaterials;
        }

        // V�rifie si l'objet est rel�ch� dans l'inventaire
        if (InventoryUI.Instance.IsPointerOverInventoryArea())
        {
            if (currentConfig.isMovable) // V�rifie si l'objet est d�pla�able
            {
                Inventory.Instance.AddToInventory(selectedObject);
                Debug.Log($"{selectedObject.name} ajout� � l'inventaire.");
            }
            else
            {
                Debug.LogWarning($"{selectedObject.name} ne peut pas �tre ajout� � l'inventaire car il n'est pas d�pla�able.");
            }
        }
        else if (currentConfig.canReceiveChildren)
        {
            // V�rifie si l'objet est rel�ch� sur un objet qui peut recevoir des enfants
            Collider[] colliders = Physics.OverlapSphere(selectedObject.transform.position, overlapSphereRadius, interactableLayer);
            foreach (var collider in colliders)
            {
                if (collider == currentConfig.dropZoneCollider)
                {
                    selectedObject.transform.SetParent(collider.transform);
                    Debug.Log($"{selectedObject.name} est devenu un enfant de {collider.name}");
                    break;
                }
            }
        }
        else
        {
            selectedObject.transform.SetParent(releasedParent);
            Debug.Log($"{selectedObject.name} rel�ch� dans la sc�ne.");
        }

        // Nettoyage des variables
        selectedObject = null;
        currentConfig = null;
        isDragging = false;
    }

    private void HandleHover()
    {
        if (isDragging) return; // Ne pas g�rer le survol pendant un drag

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hoveredObject != hitObject) // Nouveau survol d�tect�
            {
                ClearHover(); // R�initialise l'outline pr�c�dent

                foreach (var config in objectConfigs)
                {
                    // V�rifie si la porte est survol�e
                    if (config.hasDoor && config.doorTransform != null && hitObject == config.doorTransform.gameObject)
                    {
                        hoveredObject = hitObject;
                        hoveredRenderer = config.doorTransform.GetComponent<Renderer>();

                        if (hoveredRenderer != null)
                        {
                            ApplyOutline(hoveredRenderer);
                            return; // Sortir apr�s avoir appliqu� l'outline sur la porte
                        }
                    }

                    // Sinon, v�rifie si c'est le prefab entier
                    if (hitObject == config.prefab)
                    {
                        hoveredObject = hitObject;
                        hoveredRenderer = hoveredObject.GetComponent<Renderer>();

                        if (hoveredRenderer != null)
                        {
                            ApplyOutline(hoveredRenderer);
                            return; // Sortir apr�s avoir appliqu� l'outline sur le prefab
                        }
                    }
                }
            }
        }
        else
        {
            ClearHover(); // Si rien n'est survol�
        }
    }


    private void ClearHover()
    {
        if (hoveredRenderer != null && hoveredOriginalMaterials != null)
        {
            hoveredRenderer.materials = hoveredOriginalMaterials; // Restaure les mat�riaux d'origine
        }

        hoveredObject = null;
        hoveredRenderer = null;
        hoveredOriginalMaterials = null;
    }

    private void ApplyOutline(Renderer renderer)
    {
        hoveredOriginalMaterials = renderer.materials;
        var materials = new List<Material>(hoveredOriginalMaterials) { outlineMaterial };
        renderer.materials = materials.ToArray();
        outlineMaterial.SetColor("_Color", hoverColor); // Applique la couleur de survol
    }

    private bool IsPointerOverInventory()
    {
        if (InventoryUI.Instance == null) return false;

        Vector2 localMousePosition;
        RectTransform inventoryRect = InventoryUI.Instance.InventoryArea; // Utilisation de la propri�t� InventoryArea

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
        Debug.Log($"{selectedObject.name} ajout� � l'inventaire.");
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

        // V�rification si `currentConfig.dropZoneCollider` est nul
        if (currentConfig.canReceiveChildren && currentConfig.dropZoneCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentConfig.dropZoneCollider.transform.position, overlapSphereRadius);
        }

        // V�rification si `currentConfig.prefab` est nul
        if (currentConfig.prefab != null)
        {
            Gizmos.color = Color.cyan;

            if (mainCamera != null)
            {
                Gizmos.DrawLine(mainCamera.transform.position, currentConfig.prefab.transform.position);
            }

            // Affiche un label uniquement si Handles est disponible (�diteur Unity)
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
