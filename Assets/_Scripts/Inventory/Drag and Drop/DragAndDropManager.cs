#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ObjectConfig
{
    public string category;                   // Cat�gorie de l'objet
    public GameObject prefab;                 // Prefab de l'objet
    public IngredientData ingredientData;     // Le ScriptableObject associ�
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
    [Range(0f, 120f)]
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


    [Header("UI Settings")]
    [Tooltip("Canvas World Space pour le message 'Left Click To Drag'.")]
    [SerializeField] private Canvas hoverCanvasWorldSpace;

    [Tooltip("Texte affich� dans le Canvas World Space.")]
    [SerializeField] private TMPro.TextMeshProUGUI hoverWorldText;

    [Tooltip("Canvas Overlay pour afficher les d�tails de l'objet.")]
    [SerializeField] private Canvas overlayCanvas;

    [Tooltip("Texte affich� dans le Canvas Overlay.")]
    [SerializeField] private TMPro.TextMeshProUGUI overlayNameText;

    [Tooltip("Image affich�e dans le Canvas Overlay.")]
    [SerializeField] private Image overlayImage;

    [Tooltip("Texte affich� lors du drag.")]
    [SerializeField] private TMP_Text dragOverlayText;
    [SerializeField] private TextAnimationManager textAnimationManager; // Script d'animation du texte

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
    public List<ObjectConfig> ObjectConfigs => objectConfigs;

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
    private Vector3 hoverOffset = new Vector3(0, 0.5f, 0); // D�calage vertical
    private Dictionary<GameObject, HashSet<GameObject>> parentChildMap = new();

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
                dragOverlayText.gameObject.SetActive(false); // Masque le texte en bas � droite

            }
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (holdingParent == null || releasedParent == null)
            Debug.LogError("HoldingParent ou ReleasedParent n'est pas assign� !");

        if (dragOverlayText == null)
        {
            Debug.LogWarning("Drag Overlay Text is not assigned!");
        }
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
                if (currentConfig != null && currentConfig.isMovable)
                {
                    RotateObject();
                }
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
        CheckChildrenContact(); // V�rifier si les enfants restent en contact
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
                    if (!config.isMovable)
                    {
                        Debug.LogWarning($"{hitObject.name} n'est pas d�pla�able (isMovable=false).");
                        return; // Ne pas permettre le drag
                    }

                    selectedObject = config.prefab;
                    currentConfig = config;

                    if (currentConfig.ingredientData != null)
                    {
                        Debug.Log($"D�but du drag pour {currentConfig.ingredientData.ingredientName}");
                    }

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
        // Masque le Canvas World Space au d�but du drag
        HideHoverCanvasWorldSpace();

        isDragging = true;

        // R�initialiser l'outline pr�c�dent
        ResetOutline(hoveredRenderer, hoveredOriginalMaterials);

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
        // Affiche le texte en bas � droite
        /*        
        if (dragOverlayText != null)
        {
            dragOverlayText.text = $"Dragging: {selectedObject.name}"; // Personnalisez le message
            dragOverlayText.gameObject.SetActive(true); // Affiche le texte
        }*/

        if (textAnimationManager != null)
        {
            // Masque le texte en bas � droite
            if (dragOverlayText != null)
            {
                dragOverlayText.gameObject.SetActive(true);
            }
            textAnimationManager.StartTextAnimation();
        }
        else
        {
            Debug.LogWarning("TextAnimationManager non assign� dans DragAndDropManager !");
        }

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = true;

        selectedObject.transform.SetParent(holdingParent);
    }

    private void DragObject()
    {
        if (!currentConfig.isMovable)
        {
            Debug.LogWarning($"{selectedObject.name} ne peut pas �tre d�plac� (isMovable=false).");
            return;
        }

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
        if (currentConfig == null || !currentConfig.isMovable)
        {
            Debug.LogWarning("Rotation non autoris�e : l'objet n'est pas d�pla�able.");
            return;
        }

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

        // R�initialiser les mat�riaux apr�s le drag
        ResetOutline(selectedObject.GetComponent<Renderer>(), originalMaterials);

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

        // R�initialisation des mat�riaux
        if (hoveredRenderer != null && originalMaterials != null)
        {
            hoveredRenderer.materials = originalMaterials;
        }

        // Masque le texte en bas � droite
        if (dragOverlayText != null)
        {
            dragOverlayText.gameObject.SetActive(false);
        }

        // V�rifie si l'objet est rel�ch� dans l'inventaire
        if (IsPointerOverInventory() && InventoryUI.Instance != null)
        {
            Inventory.Instance.AddToInventory(selectedObject);
            Debug.Log($"{selectedObject.name} ajout� � l'inventaire.");
            CleanupDragging();
            return;
        }

        // V�rifie si l'objet est rel�ch� sur un autre objet interactif
        Collider[] colliders = Physics.OverlapSphere(selectedObject.transform.position, overlapSphereRadius, interactableLayer);
        foreach (var collider in colliders)
        {
            GameObject potentialParent = collider.gameObject;
            ObjectConfig potentialParentConfig = objectConfigs.Find(config => config.prefab == potentialParent);

            if (potentialParentConfig != null && potentialParentConfig.canReceiveChildren)
            {
                Debug.Log($"{selectedObject.name} devient enfant de {potentialParent.name}");
                selectedObject.transform.SetParent(potentialParent.transform);

                // M�morise la relation dans le parentChildMap
                if (!parentChildMap.ContainsKey(potentialParent))
                    parentChildMap[potentialParent] = new HashSet<GameObject>();

                parentChildMap[potentialParent].Add(selectedObject);

                CleanupDragging();
                return;
            }
        }

        // Si aucun parent n'est trouv�, rel�cher l'objet dans la sc�ne
        selectedObject.transform.SetParent(releasedParent);
        Debug.Log($"{selectedObject.name} rel�ch� dans la sc�ne.");
        CleanupDragging();
    }


    private void UpdateChildrenState()
    {
        foreach (var config in objectConfigs)
        {
            if (config.canReceiveChildren)
            {
                GameObject parent = config.prefab;

                // Initialise une relation si elle n'existe pas d�j�
                if (!parentChildMap.ContainsKey(parent))
                    parentChildMap[parent] = new HashSet<GameObject>();

                // R�cup�re les enfants actuels
                List<Transform> currentChildren = new List<Transform>();
                foreach (Transform child in parent.transform)
                {
                    currentChildren.Add(child);
                }

                // V�rifie si les enfants actuels doivent �tre ajout�s ou retir�s
                Collider[] colliders = Physics.OverlapSphere(parent.transform.position, overlapSphereRadius, interactableLayer);
                foreach (var child in currentChildren)
                {
                    // Si l'enfant est sorti de la zone de d�tection mais qu'il est d�j� dans la map, on le garde
                    if (!parentChildMap[parent].Contains(child.gameObject))
                    {
                        bool isStillInContact = false;
                        foreach (var collider in colliders)
                        {
                            if (collider.gameObject == child.gameObject)
                            {
                                isStillInContact = true;
                                break;
                            }
                        }

                        if (!isStillInContact)
                        {
                            Debug.Log($"{child.name} n'est plus en contact avec {parent.name} et sera d�tach�.");
                            child.SetParent(releasedParent);
                            parentChildMap[parent].Remove(child.gameObject);
                        }
                    }
                }

                // Ajoute les nouveaux enfants d�tect�s
                foreach (var collider in colliders)
                {
                    GameObject potentialChild = collider.gameObject;
                    if (potentialChild.transform.parent != parent.transform)
                    {
                        Debug.Log($"{potentialChild.name} devient enfant de {parent.name}");
                        potentialChild.transform.SetParent(parent.transform);
                        parentChildMap[parent].Add(potentialChild);
                    }
                }
            }
        }
    }

    private void CheckChildrenContact()
    {
        foreach (var config in objectConfigs)
        {
            if (config.canReceiveChildren && config.prefab.transform.childCount > 0)
            {
                Collider[] colliders = Physics.OverlapSphere(
                    config.prefab.transform.position,
                    overlapSphereRadius,
                    interactableLayer
                );

                // R�cup�re les enfants actuels
                List<Transform> currentChildren = new List<Transform>();
                foreach (Transform child in config.prefab.transform)
                {
                    currentChildren.Add(child);
                }

                // V�rifie si chaque enfant est toujours dans la zone
                foreach (var child in currentChildren)
                {
                    // V�rifiez si l'objet est sur le point d'�tre ajout� � l'inventaire
                    if (IsPointerOverInventory() && InventoryUI.Instance != null)
                    {
                        Debug.Log($"{child.name} sera ajout� � l'inventaire, ignore le d�tachement.");
                        continue; // Ignore cette logique pour cet enfant
                    }

                    bool isStillInContact = false;

                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject == child.gameObject)
                        {
                            isStillInContact = true;
                            break;
                        }
                    }

                    // Si l'enfant n'est plus en contact, le d�tacher
                    if (!isStillInContact)
                    {
                        Debug.Log($"{child.name} n'est plus en contact avec {config.prefab.name} et sera d�tach�.");
                        child.SetParent(releasedParent);
                    }
                }
            }
        }
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
                            ApplyHoverOutline(hoveredRenderer);
                            return; // Sortir apr�s avoir appliqu� l'outline sur la porte
                        }
                    }

                    // Sinon, v�rifie si c'est le prefab entier
                    if (hitObject == config.prefab)
                    {
                        hoveredObject = hitObject;
                        hoveredRenderer = hoveredObject.GetComponent<Renderer>();


                        if (hoveredRenderer != null && config.prefab == hitObject && config.isMovable)
                        {
                            // Appliquer l'outline
                            ApplyHoverOutline(hoveredRenderer);

                            // Affiche les Canvas
                            ShowHoverCanvasWorldSpace(hitObject);
                            ShowOverlayCanvas(config);

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
        // R�initialiser les outlines de hover
        ResetOutline(hoveredRenderer, hoveredOriginalMaterials);

        // D�sactive les Canvas
        HideHoverCanvasWorldSpace();
        HideOverlayCanvas();

        hoveredObject = null;
        hoveredRenderer = null;
        hoveredOriginalMaterials = null;
    }

    private void ApplyHoverOutline(Renderer renderer)
    {
        // R�initialiser les outlines pr�c�dents
        ResetOutline(hoveredRenderer, hoveredOriginalMaterials);

        // Appliquer les outlines pour le hover
        if (renderer != null)
        {
            hoveredOriginalMaterials = renderer.materials;
            var materials = new List<Material>(hoveredOriginalMaterials) { outlineMaterial };
            renderer.materials = materials.ToArray();
            outlineMaterial.SetColor("_Color", hoverColor);
        }
    }

    private void ApplyDragOutline(Renderer renderer)
    {
        if (renderer != null && outlineMaterial != null)
        {
            var materials = new List<Material>(originalMaterials) { outlineMaterial };
            renderer.materials = materials.ToArray();
            outlineMaterial.SetColor("_Color", dragColor); // Applique la couleur de drag
        }
    }
    private void ResetOutline(Renderer renderer, Material[] originalMaterials)
    {
        if (renderer != null && originalMaterials != null)
        {
            renderer.materials = originalMaterials;
        }
    }

    private void ShowHoverCanvasWorldSpace(GameObject target)
    {
        if (hoverCanvasWorldSpace != null)
        {
            // Active le Canvas World Space
            hoverCanvasWorldSpace.gameObject.SetActive(true);

            // Positionne le Canvas au-dessus de l'objet
            hoverCanvasWorldSpace.transform.position = target.transform.position + hoverOffset;

            // Oriente le Canvas pour qu'il fasse face � la cam�ra
            Vector3 cameraDirection = (hoverCanvasWorldSpace.transform.position - mainCamera.transform.position).normalized;
            hoverCanvasWorldSpace.transform.forward = cameraDirection;

            // Met � jour le texte
            if (hoverWorldText != null)
            {
                hoverWorldText.text = "Left Click To Drag";
            }
        }
    }


    private void HideHoverCanvasWorldSpace()
    {
        if (hoverCanvasWorldSpace != null)
        {
            hoverCanvasWorldSpace.gameObject.SetActive(false);
        }
    }

    private void ShowOverlayCanvas(ObjectConfig config)
    {
        if (overlayCanvas != null)
        {
            // Active le Canvas Overlay
            overlayCanvas.gameObject.SetActive(true);

            // Met � jour le texte avec le nom de l'objet
            if (overlayNameText != null)
            {
                overlayNameText.text = config.ingredientData != null
                    ? config.ingredientData.ingredientName
                    : config.prefab.name;
            }

            // Met � jour l'image avec le sprite issu de l'ingredientData
            if (overlayImage != null && config.ingredientData != null)
            {
                overlayImage.sprite = config.ingredientData.ingredientSprite; // Assurez-vous que 'IngredientData' a un champ 'Sprite'
                overlayImage.enabled = config.ingredientData.ingredientSprite != null; // Cache l'image si aucune n'est d�finie
            }
            else
            {
                overlayImage.enabled = false; // D�sactive l'image si l'ingredientData ou le sprite est null
            }
        }
    }

    private void HideOverlayCanvas()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }
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

    private void CleanupDragging()
    {
        selectedObject = null;
        currentConfig = null;
        isDragging = false;
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

        // V�rification si currentConfig.dropZoneCollider est nul
        if (currentConfig.canReceiveChildren && currentConfig.dropZoneCollider != null)
        {
            Gizmos.color = Color.green;
            if (selectedObject != null)
            {
                Gizmos.DrawWireSphere(selectedObject.transform.position, overlapSphereRadius);
            }

            foreach (var config in objectConfigs)
            {
                if (config.canReceiveChildren && config.dropZoneCollider != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(config.dropZoneCollider.transform.position, overlapSphereRadius);
                }
            }
        }

        // V�rification si currentConfig.prefab est nul
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