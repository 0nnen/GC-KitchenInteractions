using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("R�f�rences")]
    [SerializeField] private Camera mainCamera; // Cam�ra utilis�e pour le Raycast
    [SerializeField] private Transform holdingParent; // Parent temporaire pendant le drag
    [SerializeField] private Transform releasedParent; // Parent o� les objets sont plac�s une fois rel�ch�s
    [SerializeField] private Transform playerTransform; // R�f�rence au joueur

    [Header("R�glages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private float interactionRange = 3f; // Distance maximale pour interagir
    [SerializeField] private float dragDepth = 1f; // Distance des objets devant la cam�ra pendant le drag
    [SerializeField] private float rotationSpeed = 5f; // Vitesse de rotation lors du clic droit

    private GameObject selectedObject; // Objet actuellement manipul�
    private bool isDragging = false; // Indique si un objet est en cours de drag-and-drop

    private void Awake()
    {
        // Initialiser la cam�ra principale via Cinemachine
        if (mainCamera == null)
        {
            var cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            {
                mainCamera = cinemachineBrain.OutputCamera;
                Debug.Log("Cam�ra assign�e automatiquement via CinemachineBrain.");
            }
            else
            {
                Debug.LogError("Aucune cam�ra active trouv�e ! Assignez une cam�ra au champ 'MainCamera' dans l'inspecteur.");
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("Le champ 'PlayerTransform' n'est pas assign� !");
        }
    }

    private void Update()
    {
        // D�tecter le clic gauche pour commencer le drag
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDragging();
        }

        // D�placer ou faire tourner l'objet si n�cessaire
        if (isDragging && selectedObject != null)
        {
            if (Input.GetMouseButton(1)) // Rotation avec clic droit
            {
                RotateObject();
            }
            else // D�placement avec clic gauche
            {
                DragObject();
            }

            // Rel�cher l'objet si le bouton gauche est rel�ch�
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
            Debug.LogError("MainCamera ou PlayerTransform non assign�s !");
            return;
        }

        // Ignorer les clics sur une UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Lancer un Raycast pour d�tecter un objet interactif
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // V�rifier si l'objet est interactif et � port�e
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                float distanceToPlayer = Vector3.Distance(playerTransform.position, hit.collider.transform.position);
                if (distanceToPlayer <= interactionRange)
                {
                    StartDragging(hit.collider.gameObject);
                }
                else
                {
                    Debug.Log("Objet hors de port�e !");
                }
            }
        }
    }

    private void StartDragging(GameObject obj)
    {
        selectedObject = obj;
        isDragging = true;

        // D�sactiver la physique pour permettre un d�placement fluide
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // D�placer l'objet sous le parent temporaire
        if (holdingParent != null)
        {
            selectedObject.transform.SetParent(holdingParent);
        }

        Debug.Log($"D�but du drag de : {selectedObject.name}");
    }

    private void DragObject()
    {
        // Convertir la position de la souris en coordonn�es du monde
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance devant la cam�ra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // D�placer l'objet vers la position de la souris
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, 0.2f);
    }

    private void RotateObject()
    {
        // R�cup�rer le mouvement de la souris
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Appliquer une rotation autour des axes X et Y
        selectedObject.transform.Rotate(mainCamera.transform.up, -mouseX, Space.World); // Rotation horizontale
        selectedObject.transform.Rotate(mainCamera.transform.right, mouseY, Space.World); // Rotation verticale
    }

    private void StopDragging()
    {
        if (selectedObject == null) return;

        isDragging = false;

        // R�activer la physique si n�cessaire
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // V�rifier si l'objet est rel�ch� sur une UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet rel�ch� sur une UI !");
            Inventory.Instance.AddToInventory(selectedObject);
        }
        else
        {
            // D�placer l'objet sous le Released Parent
            if (releasedParent != null)
            {
                selectedObject.transform.SetParent(releasedParent);
            }
            else
            {
                selectedObject.transform.SetParent(null);
            }
            Debug.Log($"Objet rel�ch� dans Released Parent : {selectedObject.name}");
        }

        selectedObject = null;
    }
}
