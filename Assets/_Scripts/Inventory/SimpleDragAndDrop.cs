using UnityEngine;
using UnityEngine.EventSystems;
using Cinemachine;

public class SimpleDragAndDrop : MonoBehaviour
{
    [Header("R�glages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private Transform holdingParent;    // Parent temporaire pendant le drag
    [SerializeField] private float dragDepth = 1f;       // Distance de l'objet devant la cam�ra

    private Camera mainCamera;
    private bool isDragging = false;

    private void Awake()
    {
        // Cherche automatiquement la cam�ra active contr�l�e par Cinemachine
        if (mainCamera == null)
        {
            var cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            if (cinemachineBrain != null && cinemachineBrain.OutputCamera != null)
            {
                mainCamera = cinemachineBrain.OutputCamera;
            }
            else
            {
                Debug.LogError("Aucune cam�ra Cinemachine active trouv�e !");
            }
        }
    }

    private void OnMouseDown()
    {
        // V�rifie si le clic est sur un objet interactif
        if (EventSystem.current.IsPointerOverGameObject()) return; // �viter les clics sur les UI

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // V�rifie si l'objet a un Collider et est interactif
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

        // D�sactiver la physique pour permettre le d�placement
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        // D�placer l'objet sous le parent temporaire
        if (holdingParent != null)
        {
            transform.SetParent(holdingParent);
        }
    }

    private void DragObject()
    {
        // R�cup�re la position de la souris
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance devant la cam�ra

        // Convertir la position de la souris en coordonn�es du monde
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // D�placer l'objet
        transform.position = Vector3.Lerp(transform.position, worldPosition, 0.2f); // D�placement fluide
    }

    private void StopDragging()
    {
        isDragging = false;

        // R�activer la physique si n�cessaire
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // Supprimer le parent temporaire
        transform.SetParent(null);

        // V�rifie si l'objet est rel�ch� sur une zone d'inventaire
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet ajout� � l'inventaire");
            InventoryUI.Instance.AddToInventory(gameObject);
        }
    }
}
