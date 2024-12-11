using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class SimpleDragAndDrop : MonoBehaviour
{
    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private Transform holdingParent;    // Parent temporaire pendant le drag
    [SerializeField] private float dragDepth = 1f;       // Distance de l'objet devant la caméra

    private Camera mainCamera;
    private bool isDragging = false;

    private void Awake()
    {
        // Cherche automatiquement la caméra active contrôlée par Cinemachine
        if (mainCamera == null)
        {
            var cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            {
                mainCamera = cinemachineBrain.OutputCamera;
            }
            else
            {
                Debug.LogError("Aucune caméra Cinemachine active trouvée !");
            }
        }
    }

    private void OnMouseDown()
    {
        // Vérifie si le clic est sur un objet interactif
        if (EventSystem.current.IsPointerOverGameObject()) return; // Éviter les clics sur les UI

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // Vérifie si l'objet a un Collider et est interactif
            if (hit.collider.gameObject == gameObject)
            {
                StartDragging();
            }
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            DragObject();
        }
    }

    private void OnMouseUp()
    {
        if (isDragging)
        {
            StopDragging();
        }
    }

    private void StartDragging()
    {
        isDragging = true;

        // Désactiver la physique pour permettre le déplacement
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // Déplacer l'objet sous le parent temporaire
        if (holdingParent != null)
        {
            transform.SetParent(holdingParent);
        }
    }

    private void DragObject()
    {
        // Récupère la position de la souris
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance devant la caméra

        // Convertir la position de la souris en coordonnées du monde
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Déplacer l'objet
        transform.position = Vector3.Lerp(transform.position, worldPosition, 0.2f); // Déplacement fluide
    }

    private void StopDragging()
    {
        isDragging = false;

        // Réactiver la physique si nécessaire
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // Supprimer le parent temporaire
        transform.SetParent(null);

        // Vérifie si l'objet est relâché sur une zone d'inventaire
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet ajouté à l'inventaire");
            InventoryUI.Instance.AddToInventory(gameObject);
        }
    }
}
