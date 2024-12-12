using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform holdingParent;
    [SerializeField] private Transform releasedParent;

    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float dragDepth = 2f; // Distance fixe pour le drag
    [SerializeField] private float minDragDepth = 1f; // Distance minimale
    [SerializeField] private float maxDragDepth = 5f; // Distance maximale
    [SerializeField] private float scrollSensitivity = 0.5f; // Sensibilité de la molette
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minHeight = 0.5f; // Hauteur minimale pour éviter le sol

    private GameObject selectedObject;
    private Vector3 offset; // Décalage entre le point cliqué et le centre de l'objet
    private bool isDragging = false;

    private void Awake()
    {
        if (mainCamera == null)
        {
            var cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            {
                mainCamera = cinemachineBrain.OutputCamera;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDragging();
        }

        if (isDragging && selectedObject != null)
        {
            HandleScrollWheel();

            if (Input.GetMouseButton(1))
            {
                RotateObject();
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

    private void TryStartDragging()
    {
        if (mainCamera == null) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                float distanceToPlayer = Vector3.Distance(mainCamera.transform.position, hit.collider.transform.position);
                if (distanceToPlayer <= interactionRange)
                {
                    StartDragging(hit.collider.gameObject, hit.point);
                }
                else
                {
                    Debug.Log("Objet hors de portée !");
                }
            }
        }
    }

    private void StartDragging(GameObject obj, Vector3 hitPoint)
    {
        selectedObject = obj;
        isDragging = true;

        // Calculer le décalage entre le point cliqué et le centre de l'objet
        offset = selectedObject.transform.position - hitPoint;

        selectedObject.SetActive(true);

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (holdingParent != null)
        {
            selectedObject.transform.SetParent(holdingParent);
        }
    }

    private void DragObject()
    {
        // Obtenir la direction de la souris dans l'espace 3D
        Vector3 mouseDirection = mainCamera.ScreenPointToRay(Input.mousePosition).direction;

        // Calculer la position cible à une distance fixe de dragDepth
        Vector3 targetPosition = mainCamera.transform.position + mouseDirection.normalized * dragDepth;

        // Appliquer l'offset pour conserver le point de contact initial
        targetPosition += offset;

        // Limiter la hauteur pour éviter que l'objet passe sous le sol
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        // Déplacer l'objet vers la position cible
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetPosition, 0.2f);
    }

    private void RotateObject()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        selectedObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World);
        selectedObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World);
    }

    private void StopDragging()
    {
        if (selectedObject == null) return;

        isDragging = false;

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            InventoryUI.Instance.AddToInventory(selectedObject);
        }
        else
        {
            if (releasedParent != null)
            {
                selectedObject.transform.SetParent(releasedParent);
            }
            else
            {
                selectedObject.transform.SetParent(null);
            }

            // L'objet reste à la position actuelle (déjà gérée pendant le drag)
        }

        selectedObject = null;
    }

    private void HandleScrollWheel()
    {
        // Récupérer l'entrée de la molette de la souris
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        // Ajuster la profondeur du drag
        dragDepth += scrollDelta * scrollSensitivity;

        // Limiter la profondeur à minDragDepth et maxDragDepth
        dragDepth = Mathf.Clamp(dragDepth, minDragDepth, maxDragDepth);
    }
}
