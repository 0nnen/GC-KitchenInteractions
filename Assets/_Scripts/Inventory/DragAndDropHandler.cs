using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform holdingParent;
    [SerializeField] private Transform releasedParent;
    [SerializeField] private Transform playerTransform;

    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float dragDepth = 1f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minHeight = 0.5f; // Hauteur minimale pour empêcher les objets de passer sous le sol

    private GameObject selectedObject;
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

        if (playerTransform == null)
        {
            Debug.LogError("Le champ 'PlayerTransform' n'est pas assigné !");
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
        if (mainCamera == null || playerTransform == null)
        {
            Debug.LogError("MainCamera ou PlayerTransform non assignés !");
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                float distanceToPlayer = Vector3.Distance(playerTransform.position, hit.collider.transform.position);
                if (distanceToPlayer <= interactionRange)
                {
                    StartDragging(hit.collider.gameObject);
                }
                else
                {
                    Debug.Log("Objet hors de portée !");
                }
            }
        }
    }

    private void StartDragging(GameObject obj)
    {
        selectedObject = obj;
        isDragging = true;

        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;

            // Désactiver temporairement les collisions avec le joueur
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), playerTransform.GetComponent<Collider>(), true);
        }

        if (holdingParent != null)
        {
            selectedObject.transform.SetParent(holdingParent);
        }
    }

    private void DragObject()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Empêcher l'objet de descendre sous la hauteur minimale
        if (worldPosition.y < minHeight)
        {
            worldPosition.y = minHeight;
        }

        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, 0.2f);
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

            // Réactiver les collisions avec le joueur
            Physics.IgnoreCollision(selectedObject.GetComponent<Collider>(), playerTransform.GetComponent<Collider>(), false);
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            Inventory.Instance.AddToInventory(selectedObject);
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
        }

        selectedObject = null;
    }
}
