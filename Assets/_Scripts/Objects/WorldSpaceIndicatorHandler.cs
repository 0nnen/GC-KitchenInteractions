using UnityEngine;

public class WorldSpaceIndicatorHandler : MonoBehaviour
{
    [Header("R�f�rences")]
    [SerializeField] private Canvas worldSpaceCanvas; // Canvas en World Space
    [SerializeField] private RectTransform indicatorUI; // Indicateur � afficher au-dessus de l'objet interactable
    [SerializeField] private Camera mainCamera; // Cam�ra utilis�e pour la vue
    [SerializeField] private Transform playerTransform; // Transform du joueur

    [Header("R�glages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private float interactionRange = 3f; // Distance maximale pour afficher l'indicateur
    [SerializeField] private float heightOffset = 1.5f; // D�calage vertical au-dessus de l'objet interactable

    private Interactable currentTarget; // Objet interactable actuellement cibl�

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (worldSpaceCanvas == null)
        {
            Debug.LogError("Le Canvas World Space n'est pas assign� !");
        }

        if (indicatorUI == null)
        {
            Debug.LogError("L'indicateur UI n'est pas assign� !");
        }

        // D�sactiver l'indicateur au d�marrage
        indicatorUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        DetectInteractable();

        if (currentTarget != null)
        {
            UpdateIndicatorPosition();
        }
    }

    private void DetectInteractable()
    {
        // Lancer un raycast depuis le centre de la cam�ra
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // V�rifier si l'objet est interactif et dans la port�e
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                float distanceToPlayer = Vector3.Distance(playerTransform.position, hit.collider.transform.position);
                if (distanceToPlayer <= interactionRange)
                {
                    if (currentTarget != interactable)
                    {
                        // Changer de cible
                        currentTarget?.OnFocusLost();
                        currentTarget = interactable;
                        currentTarget.OnFocused();
                        indicatorUI.gameObject.SetActive(true); // Activer l'indicateur
                    }
                }
                return;
            }
        }

        // Aucune cible trouv�e
        if (currentTarget != null)
        {
            currentTarget.OnFocusLost();
            currentTarget = null;
            indicatorUI.gameObject.SetActive(false); // D�sactiver l'indicateur
        }
    }

    private void UpdateIndicatorPosition()
    {
        if (currentTarget == null) return;

        // Position r�elle de l'objet avec d�calage vertical
        Vector3 targetPosition = currentTarget.transform.position + Vector3.up * heightOffset;

        // Placer l'indicateur dans le monde
        indicatorUI.position = targetPosition;

        // Faire face � la cam�ra (si n�cessaire pour une lisibilit� optimale)
        indicatorUI.LookAt(mainCamera.transform);
        indicatorUI.Rotate(0, 180, 0); // Corriger l'orientation si n�cessaire
    }

    // M�thode pour ajuster dynamiquement la hauteur
    public void SetHeightOffset(float newHeightOffset)
    {
        heightOffset = newHeightOffset;
    }
}
