/*using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleDragAndDrop : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera mainCamera; // Caméra utilisée pour le Raycast

    [SerializeField] private Transform holdingParent; // Parent temporaire pendant le drag
    [SerializeField] private Transform playerTransform; // Référence au joueur

    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs

    [SerializeField] private float interactionRange = 3f; // Distance maximale pour interagir
    [SerializeField] private float dragDepth = 1f; // Distance de l'objet devant la caméra

    private bool isDragging = false; // Indique si l'objet est en train d'être déplacé

    private void Awake()
    {
        // Si la caméra principale n'est pas assignée, tente de trouver celle contrôlée par Cinemachine
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

        // Vérifier si le joueur est assigné
        if (playerTransform == null)
        {
            Debug.LogError("Le champ 'PlayerTransform' n'est pas assigné !");
        }
    }

    private void OnMouseDown()
    {
        if (mainCamera == null || playerTransform == null)
        {
            Debug.LogError("MainCamera ou PlayerTransform non assignés !");
            return;
        }

        // Ignorer le clic si la souris est au-dessus d'une UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Vérifier la portée avant de lancer le Raycast
        if (Vector3.Distance(playerTransform.position, transform.position) > interactionRange)
        {
            Debug.Log("Objet hors de portée !");
            return; // Objet trop loin
        }

        // Lancer un Raycast pour détecter un objet interactif
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
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

        // Désactiver la physique pour permettre le déplacement fluide
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // Placer l'objet sous le parent temporaire
        if (holdingParent != null)
        {
            transform.SetParent(holdingParent);
        }
    }

    private void DragObject()
    {
        // Convertir la position de la souris en coordonnées du monde
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance de l'objet devant la caméra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Déplacer l'objet vers la position de la souris
        transform.position = Vector3.Lerp(transform.position, worldPosition, 0.2f); // Mouvement fluide
    }

    private void StopDragging()
    {
        isDragging = false;

        // Réactiver la physique
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // Réinitialiser le parent de l'objet
        transform.SetParent(null);

        // Vérifier si l'objet est relâché sur une UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet relâché sur une zone UI !");
            InventoryUI.Instance.AddToInventory(gameObject); // Ajoute l'objet à l'inventaire
        }
        else
        {
            Debug.Log("Objet relâché dans la scène !");
        }
    }
}*/