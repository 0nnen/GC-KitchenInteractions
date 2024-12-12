/*using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleDragAndDrop : MonoBehaviour
{
    [Header("R�f�rences")]
    [SerializeField] private Camera mainCamera; // Cam�ra utilis�e pour le Raycast

    [SerializeField] private Transform holdingParent; // Parent temporaire pendant le drag
    [SerializeField] private Transform playerTransform; // R�f�rence au joueur

    [Header("R�glages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs

    [SerializeField] private float interactionRange = 3f; // Distance maximale pour interagir
    [SerializeField] private float dragDepth = 1f; // Distance de l'objet devant la cam�ra

    private bool isDragging = false; // Indique si l'objet est en train d'�tre d�plac�

    private void Awake()
    {
        // Si la cam�ra principale n'est pas assign�e, tente de trouver celle contr�l�e par Cinemachine
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

        // V�rifier si le joueur est assign�
        if (playerTransform == null)
        {
            Debug.LogError("Le champ 'PlayerTransform' n'est pas assign� !");
        }
    }

    private void OnMouseDown()
    {
        if (mainCamera == null || playerTransform == null)
        {
            Debug.LogError("MainCamera ou PlayerTransform non assign�s !");
            return;
        }

        // Ignorer le clic si la souris est au-dessus d'une UI
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // V�rifier la port�e avant de lancer le Raycast
        if (Vector3.Distance(playerTransform.position, transform.position) > interactionRange)
        {
            Debug.Log("Objet hors de port�e !");
            return; // Objet trop loin
        }

        // Lancer un Raycast pour d�tecter un objet interactif
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

        // D�sactiver la physique pour permettre le d�placement fluide
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
        // Convertir la position de la souris en coordonn�es du monde
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = dragDepth; // Distance de l'objet devant la cam�ra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // D�placer l'objet vers la position de la souris
        transform.position = Vector3.Lerp(transform.position, worldPosition, 0.2f); // Mouvement fluide
    }

    private void StopDragging()
    {
        isDragging = false;

        // R�activer la physique
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        // R�initialiser le parent de l'objet
        transform.SetParent(null);

        // V�rifier si l'objet est rel�ch� sur une UI
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Objet rel�ch� sur une zone UI !");
            InventoryUI.Instance.AddToInventory(gameObject); // Ajoute l'objet � l'inventaire
        }
        else
        {
            Debug.Log("Objet rel�ch� dans la sc�ne !");
        }
    }
}*/