using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Réglages du Drag")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactables
    [SerializeField] private float dragDepth = 2f; // Distance fixe pour le drag
    [SerializeField] private float minHeight = 0.5f; // Hauteur minimale pour éviter le sol

    private Camera mainCamera;
    private GameObject draggedObject;
    private bool isDragging = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("La caméra principale n'est pas assignée !");
        }
    }

    private void Update()
    {
        if (isDragging && draggedObject != null)
        {
            DragObject();

            if (Input.GetMouseButtonUp(0)) // Lâcher l'objet avec clic gauche
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
            rb.isKinematic = true; // Désactiver la physique pendant le drag
        }

        isDragging = true;
    }

    private void DragObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPosition = mainCamera.transform.position + ray.direction * dragDepth;

        // Limiter la hauteur pour éviter que l'objet passe sous le sol
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight);

        draggedObject.transform.position = targetPosition;
    }

    private void EndDrag()
    {
        if (draggedObject == null) return;

        // Réactiver la physique si l'objet en a une
        if (draggedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = false;
        }

        Debug.Log($"{draggedObject.name} relâché dans la scène.");
        isDragging = false;
        draggedObject = null;
    }
}
