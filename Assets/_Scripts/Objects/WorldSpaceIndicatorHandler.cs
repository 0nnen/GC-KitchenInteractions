using UnityEngine;

public class WorldSpaceIndicatorHandler : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Canvas worldSpaceCanvas; // Canvas en World Space
    [SerializeField] private RectTransform indicatorUI; // Indicateur à afficher au-dessus de l'objet interactable
    [SerializeField] private Camera mainCamera; // Caméra utilisée pour la vue
    [SerializeField] private Transform playerTransform; // Transform du joueur

    [Header("Réglages")]
    [SerializeField] private LayerMask interactableLayer; // Layer des objets interactifs
    [SerializeField] private float interactionRange = 3f; // Distance maximale pour afficher l'indicateur
    [SerializeField] private float heightOffset = 1.5f; // Décalage vertical au-dessus de l'objet interactable

    private Interactable currentTarget; // Objet interactable actuellement ciblé

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (worldSpaceCanvas == null)
        {
            Debug.LogError("Le Canvas World Space n'est pas assigné !");
        }

        if (indicatorUI == null)
        {
            Debug.LogError("L'indicateur UI n'est pas assigné !");
        }

        // Désactiver l'indicateur au démarrage
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
        // Lancer un raycast depuis le centre de la caméra
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayer))
        {
            // Vérifier si l'objet est interactif et dans la portée
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

        // Aucune cible trouvée
        if (currentTarget != null)
        {
            currentTarget.OnFocusLost();
            currentTarget = null;
            indicatorUI.gameObject.SetActive(false); // Désactiver l'indicateur
        }
    }

    private void UpdateIndicatorPosition()
    {
        if (currentTarget == null) return;

        // Position réelle de l'objet avec décalage vertical
        Vector3 targetPosition = currentTarget.transform.position + Vector3.up * heightOffset;

        // Placer l'indicateur dans le monde
        indicatorUI.position = targetPosition;

        // Faire face à la caméra (si nécessaire pour une lisibilité optimale)
        indicatorUI.LookAt(mainCamera.transform);
        indicatorUI.Rotate(0, 180, 0); // Corriger l'orientation si nécessaire
    }

    // Méthode pour ajuster dynamiquement la hauteur
    public void SetHeightOffset(float newHeightOffset)
    {
        heightOffset = newHeightOffset;
    }
}
