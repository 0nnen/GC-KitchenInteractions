using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera mainCamera; // Caméra utilisée pour le Raycast
    [SerializeField] private Transform holdingParent; // Parent temporaire pendant le drag
    [SerializeField] private Transform releasedParent; // Parent où les objets sont placés une fois relâchés
    [SerializeField] private Transform playerTransform; // Référence au joueur

    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private float interactionRange = 3f; // Distance maximale pour interagir
    [SerializeField] private float dragDepth = 1f; // Distance des objets devant la caméra pendant le drag
    [SerializeField] private float rotationSpeed = 5f; // Vitesse de rotation lors du clic droit

    private GameObject selectedObject; // Objet actuellement manipulé
    private bool isDragging = false; // Indique si un objet est en cours de drag-and-drop

    private void Awake()
    {
        // Initialiser la caméra principale via Cinemachine
        if (mainCamera == null)
        {
            var cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            {
                mainCamera = cinemachineBrain.OutputCamera;
                Debug.Log("Caméra assignée automatiquement via CinemachineBrain.");
            }
            else
            {
                Debug.LogError("Aucune caméra active trouvée ! Assignez une caméra au champ 'MainCamera' dans l'inspecteur.");
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("Le champ 'PlayerTransform' n'est pas assigné !");
        }
    }

    private void Update()
    {
        // Détecter le clic gauche pour commencer le drag
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            TryStartDragging();
        }

        // Déplacer ou faire tourner l'objet si nécessaire
        if (isDragging && selectedObject != null)
        {
            if (Input.GetMouseButton(1)) // Rotation avec clic droit
            {
                RotateObject();
            }
            else // Déplacement avec clic gauche
            {
                DragObject();
            }

            // Relâcher l'objet si le bouton gauche est relâché
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

        // Ignorer les clics sur une UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Lancer un Raycast pour détecter un objet interactif
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // Vérifier si l'objet est interactif et à portée
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

        // Désactiver la physique pour permettre un déplacement fluide
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // Déplacer l'objet sous le parent temporaire
        if (holdingParent != null)
        {
            selectedObject.transform.SetParent(holdingParent);
        }

        Debug.Log($"Début du drag de : {selectedObject.name}");
    }

    private void DragObject()
    {
        // Convertir la position de la souris en coordonnées du monde
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance devant la caméra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Déplacer l'objet vers la position de la souris
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, 0.2f);
    }

    private void RotateObject()
    {
        // Récupérer le mouvement de la souris
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

        // Réactiver la physique si nécessaire
        if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // Vérifier si l'objet est relâché sur une UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet relâché sur une UI !");
            Inventory.Instance.AddToInventory(selectedObject);
        }
        else
        {
            // Déplacer l'objet sous le Released Parent
            if (releasedParent != null)
            {
                selectedObject.transform.SetParent(releasedParent);
            }
            else
            {
                selectedObject.transform.SetParent(null);
            }
            Debug.Log($"Objet relâché dans Released Parent : {selectedObject.name}");
        }

        selectedObject = null;
    }
}
