using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("R�glages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private Transform holdingParent;    // Parent temporaire pendant le drag

    private Camera mainCamera;
    private GameObject selectedObject; // Objet actuellement manipul�
    private bool isDragging; // Indique si l'utilisateur drag un objet
    private InputSystem_Actions inputActions; // Classe g�n�r�e automatiquement

    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new InputSystem_Actions(); // Initialise la classe d'entr�e
    }

    private void OnEnable()
    {
        inputActions.UI.Enable(); // Active l'Action Map "UI"

        // Abonne l'action "Click" aux m�thodes de d�but et fin du drag
        inputActions.UI.Click.started += StartDragging;
        inputActions.UI.Click.canceled += StopDragging;
    }

    private void OnDisable()
    {
        // D�sabonne les �v�nements pour �viter les erreurs
        inputActions.UI.Click.started -= StartDragging;
        inputActions.UI.Click.canceled -= StopDragging;

        inputActions.UI.Disable(); // D�sactive l'Action Map "UI"
    }

    private void Update()
    {
        if (isDragging && selectedObject != null)
        {
            MoveObjectWithMouse();
        }
    }

    private void StartDragging(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        // Lancer un raycast pour d�tecter un objet interactif
        Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                selectedObject = hit.collider.gameObject;
                isDragging = true;

                // D�sactiver la physique pour permettre un d�placement fluide
                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                }

                // Associer l'objet au holdingParent
                selectedObject.transform.SetParent(holdingParent);
                Debug.Log($"D�but du drag de l'objet : {selectedObject.name}");
            }
        }
    }

    private void StopDragging(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (selectedObject != null)
        {
            // V�rifie si l'objet est rel�ch� sur une UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Objet rel�ch� dans une UI !");
                InventoryUI.Instance.AddToInventory(selectedObject);
            }
            else
            {
                // R�activer la physique si l'objet reste dans la sc�ne
                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                }

                // R�initialiser l'objet
                selectedObject.transform.SetParent(null);
                Debug.Log($"Fin du drag de l'objet : {selectedObject.name}");
            }

            selectedObject = null;
            isDragging = false;
        }
    }

    private void MoveObjectWithMouse()
    {
        // Convertir la position de la souris en coordonn�es du monde
        Vector3 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        mousePosition.z = 1f; // Distance arbitraire pour placer l'objet devant la cam�ra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // D�placer l'objet de mani�re fluide
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, 0.2f);
    }
}
