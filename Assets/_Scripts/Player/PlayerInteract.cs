using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;

    [SerializeField] private LayerMask interactableLayer;
    private Camera playerCamera;
    private Interactable currentTarget;

    private void Awake()
    {
        playerCamera = Camera.main;
    }

    // Appelée par l'Input System lors de l'appui sur "Interact"
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        // Lancer un raycast vers l'objet devant le joueur
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable != currentTarget)
                {
                    // Changer de cible
                    if (currentTarget != null) currentTarget.OnFocusLost();
                    currentTarget = interactable;
                    currentTarget.OnFocused();
                }
                return;
            }
        }

        // Aucune cible trouvée
        if (currentTarget != null)
        {
            currentTarget.OnFocusLost();
            currentTarget = null;
        }
    }
}