using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("R�glages du Drag")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactables
    [SerializeField] private float dragDepth = 2f; // Distance fixe pour le drag
    [SerializeField] private float minHeight = 0.5f; // Hauteur minimale pour �viter le sol

    private Camera mainCamera;
    private GameObject draggedObject;
    private bool isDragging = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("La cam�ra principale n'est pas assign�e !");
        }
    }

    private void Update()
    {
        if (isDragging && draggedObject != null)
        {
            DragObject();

            if (Input.GetMouseButtonUp(0)) // L�cher l'objet avec clic gauche
            {
                EndDrag();
            }
        }
    }

    public void BeginDrag(GameObject item)
    {
        if (item == null) return;

        draggedObject = item;
        draggedObject.SetActive(true);

        if (draggedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true; // D�sactiver la physique pendant le drag
        }

        isDragging = true;
    }

    private void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = mainCamera.transform.position + ray.direction * dragDepth;

        // Limiter la hauteur pour �viter que l'objet passe sous le sol
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        draggedObject.transform.position = targetPosition;
    }

    private void EndDrag()
    {
        if (draggedObject == null) return;

        // R�activer la physique si l'objet en a une
        if (draggedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        Debug.Log($"{draggedObject.name} rel�ch� dans la sc�ne.");
        isDragging = false;
        draggedObject = null;
    }
}
