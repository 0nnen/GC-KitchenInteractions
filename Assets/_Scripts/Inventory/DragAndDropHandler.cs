using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private Transform holdingParent;    // Parent temporaire pendant le drag

    private Camera mainCamera;
    private GameObject selectedObject; // Objet actuellement manipulé
    private bool isDragging; // Indique si l'utilisateur drag un objet
    private InputSystem_Actions inputActions; // Classe générée automatiquement

    private void Awake()
    {
        mainCamera = Camera.main;
        inputActions = new InputSystem_Actions(); // Initialise la classe d'entrée
    }

    private void OnEnable()
    {
        inputActions.UI.Enable(); // Active l'Action Map "UI"

        // Abonne l'action "Click" aux méthodes de début et fin du drag
        inputActions.UI.Click.started += StartDragging;
        inputActions.UI.Click.canceled += StopDragging;
    }

    private void OnDisable()
    {
        // Désabonne les événements pour éviter les erreurs
        inputActions.UI.Click.started -= StartDragging;
        inputActions.UI.Click.canceled -= StopDragging;

        inputActions.UI.Disable(); // Désactive l'Action Map "UI"
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
        // Lancer un raycast pour détecter un objet interactif
        Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                selectedObject = hit.collider.gameObject;
                isDragging = true;

                // Désactiver la physique pour permettre un déplacement fluide
                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                }

                // Associer l'objet au holdingParent
                selectedObject.transform.SetParent(holdingParent);
                Debug.Log($"Début du drag de l'objet : {selectedObject.name}");
            }
        }
    }

    private void StopDragging(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (selectedObject != null)
        {
            // Vérifie si l'objet est relâché sur une UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Objet relâché dans une UI !");
                InventoryUI.Instance.AddToInventory(selectedObject);
            }
            else
            {
                // Réactiver la physique si l'objet reste dans la scène
                if (selectedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                }

                // Réinitialiser l'objet
                selectedObject.transform.SetParent(null);
                Debug.Log($"Fin du drag de l'objet : {selectedObject.name}");
            }

            selectedObject = null;
            isDragging = false;
        }
    }

    private void MoveObjectWithMouse()
    {
        // Convertir la position de la souris en coordonnées du monde
        Vector3 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        mousePosition.z = 1f; // Distance arbitraire pour placer l'objet devant la caméra
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Déplacer l'objet de manière fluide
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, worldPosition, 0.2f);
    }
}
