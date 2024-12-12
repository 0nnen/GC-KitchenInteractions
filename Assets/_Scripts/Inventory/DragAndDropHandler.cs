using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("La caméra principale utilisée pour déterminer les actions de drag-and-drop.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Le parent temporaire utilisé pour manipuler les objets.")]
    [SerializeField] private Transform holdingParent;

    [Tooltip("Le parent par défaut où les objets relâchés sont placés.")]
    [SerializeField] private Transform releasedParent;

    [Header("Réglages Généraux")]
    [Tooltip("Le layer utilisé pour détecter les objets interactables.")]
    [SerializeField] private LayerMask interactableLayer;

    [Tooltip("La portée maximale pour interagir avec les objets.")]
    [Range(1f, 10f)]
    [SerializeField] private float interactionRange = 3f;

    [Header("Paramètres de Drag")]
    [Tooltip("Distance par défaut pour manipuler les objets.")]
    [Range(1f, 10f)]
    [SerializeField] private float dragDepth = 2f;

    [Tooltip("Distance minimale pour manipuler un objet.")]
    [Range(0.1f, 10f)]
    [SerializeField] private float minDragDepth = 1f;

    [Tooltip("Distance maximale pour manipuler un objet.")]
    [Range(1f, 20f)]
    [SerializeField] private float maxDragDepth = 5f;

    [Tooltip("Rayon pour détecter les collisions lors du relâchement.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float overlapSphereRadius = 0.5f;

    [Header("Réglages de Sensibilité")]
    [Tooltip("Sensibilité du défilement lors de la manipulation.")]
    [Range(0.1f, 2f)]
    [SerializeField] private float scrollSensitivity = 0.5f;

    [Tooltip("Vitesse de rotation des objets.")]
    [Range(1f, 20f)]
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Hauteur minimale pour empêcher les objets de passer sous le sol.")]
    [Range(0.1f, 5f)]
    [SerializeField] private float minHeight = 0.5f;

    private GameObject selectedObject;
    private DoorHandler currentDoor;
    private Vector3 offset;
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

        if (isDragging)
        {
            if (currentDoor != null)
            {
                HandleDoorDrag();
            }
            else if (selectedObject != null)
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
            }

            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }
    }

    private void TryStartDragging()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not assigned!");
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Pointer is over UI, ignoring input.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.TryGetComponent<DoorHandler>(out DoorHandler doorHandler))
            {
                currentDoor = doorHandler;
                isDragging = true;
            }
            else if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                float distanceToPlayer = Vector3.Distance(mainCamera.transform.position, hit.collider.transform.position);
                if (distanceToPlayer <= interactionRange)
                {
                    StartDragging(hit.collider.gameObject, hit.point);
                }
                else
                {
                    Debug.Log("Object is out of range!");
                }
            }
        }
    }

    private void StartDragging(GameObject obj, Vector3 hitPoint)
    {
        selectedObject = obj;
        isDragging = true;

        offset = selectedObject.transform.position - hitPoint;

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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 mouseDirection = ray.direction;

        Vector3 targetPosition = mainCamera.transform.position + mouseDirection.normalized * dragDepth;
        targetPosition += offset;
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetPosition, 0.2f);
    }

    private void RotateObject()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        selectedObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World);
        selectedObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World);
    }

    private void HandleDoorDrag()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        currentDoor.RotateDoor(mouseX);
    }

    private void StopDragging()
    {
        if (isDragging)
        {
            if (currentDoor != null)
            {
                currentDoor = null;
            }
            else if (selectedObject != null)
            {
                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = false; // Reset rigidbody state
                }

                // Vérifiez si l'objet est relâché sur l'UI
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    InventoryUI.Instance.AddToInventory(selectedObject);
                    selectedObject.SetActive(false);
                    Debug.Log($"{selectedObject.name} added to inventory.");
                }
                else
                {
                    // Vérifiez les collisions autour de l'objet relâché
                    Collider[] colliders = Physics.OverlapSphere(selectedObject.transform.position, overlapSphereRadius, interactableLayer);
                    foreach (var collider in colliders)
                    {
                        if (collider.TryGetComponent<Interactable>(out Interactable targetInteractable) &&
                            targetInteractable.CanReceiveChildren)
                        {
                            selectedObject.transform.SetParent(targetInteractable.transform);
                            if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody childRb))
                            {
                                childRb.isKinematic = true; // Child becomes kinematic
                            }
                            Debug.Log($"{selectedObject.name} is now a child of {collider.name}");
                            selectedObject = null;
                            isDragging = false;
                            rb.isKinematic = false;
                            return;
                        }
                    }

                    // Sinon, replacer dans ReleasedParent
                    selectedObject.transform.SetParent(releasedParent);
                    Debug.Log($"{selectedObject.name} placed in ReleasedParent.");
                }

                selectedObject = null;
            }
            isDragging = false;
        }
    }

    private void HandleScrollWheel()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        dragDepth += scrollDelta * scrollSensitivity;
        dragDepth = Mathf.Clamp(dragDepth, minDragDepth, maxDragDepth);
    }
}
