using UnityEngine;
using TMPro;
using System.Collections;

public class FridgeInteract : MonoBehaviour
{
    [Header("UI Interaction")]
    [Tooltip("Canvas en World Space pour afficher le texte.")]
    [SerializeField] private Canvas interactionCanvas; // Canvas World Space pour l'interaction

    [Tooltip("TextMeshPro pour afficher le message.")]
    [SerializeField] private TMP_Text interactionText; // Texte pour afficher le message

    [Tooltip("Ancre où sera positionné le Canvas.")]
    [SerializeField] private Transform canvasAnchor;   // Ancre où sera positionné le Canvas

    [Tooltip("Transform du joueur.")]
    [SerializeField] private Transform playerTransform;     // Transform du joueur

    [Tooltip("Décalage du Canvas.")]
    [SerializeField] private Vector3 canvasOffset = new Vector3(0, 1f, 0); // Décalage du Canvas

    [Tooltip("Oriente le Canvas vers le joueur.")]
    [SerializeField] private bool dynamicOrientation = true; // Oriente le Canvas vers le joueur

    [Space(10)]
    [Header("UI du frigo")]
    [Tooltip("UI principale du frigo.")]
    [SerializeField] private GameObject fridgeUI;

    [Tooltip("RectTransform du UI Fridge pour animation.")]
    [SerializeField] private RectTransform fridgeUIRect;

    [Header("Paramètres")]
    [Tooltip("Distance maximale pour interagir et afficher le Canvas.")]
    [SerializeField] private float interactionRange = 3.0f;

    [Tooltip("Durée de l'animation en secondes.")]
    [SerializeField] private float animationDuration = 1f;

    private bool isPlayerInRange = false;
    private bool isFridgeOpen = false;
    private bool isAnimating = false; // Indique si une animation est en cours
    private Vector2 fridgeClosedPosition = new Vector2(0, -500); // Hors écran
    private Vector2 fridgeOpenPosition = new Vector2(0, 0);      // Position visible

    private void Start()
    {
        if (interactionCanvas != null)
            interactionCanvas.gameObject.SetActive(false);

        fridgeUI.SetActive(false);

        // Initialiser la position du frigo fermé
        if (fridgeUIRect != null)
        {
            fridgeUIRect.anchoredPosition = fridgeClosedPosition;
        }
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        HandleCanvasVisibility();

        if (distanceToPlayer <= interactionRange)
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                InteractionMessageHandler.Instance.ShowMessage("Appuyez sur E pour ouvrir le frigo", transform);
            }

            if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
            {
                ToggleFridgeUI();
            }
        }
        else
        {
            if (isPlayerInRange)
            {
                isPlayerInRange = false;
                InteractionMessageHandler.Instance.HideMessage();
            }

            if (isFridgeOpen)
            {
                ToggleFridgeUI();
            }
        }
    }

    private void HandleCanvasVisibility()
    {
        if (interactionCanvas == null || canvasAnchor == null || playerTransform == null) return;

        // Calcul de la distance entre le joueur et l'ancre du Canvas
        float distanceToPlayer = Vector3.Distance(playerTransform.position, canvasAnchor.position);

        // Affiche ou cache le Canvas en fonction de la distance
        if (distanceToPlayer <= interactionRange)
        {
            if (!interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(true);
            }

            // Positionner le Canvas
            interactionCanvas.transform.position = canvasAnchor.position + canvasOffset;

            // Oriente le Canvas vers le joueur si l'orientation dynamique est activée
            if (dynamicOrientation)
            {
                Vector3 directionToPlayer = playerTransform.position - interactionCanvas.transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer.normalized);
                interactionCanvas.transform.rotation = lookRotation;
            }
        }
        else
        {
            if (interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void ToggleFridgeUI()
    {
        if (isAnimating) return; // Bloque si une animation est en cours

        isAnimating = true; // Début de l'animation
        isFridgeOpen = !isFridgeOpen;

        if (isFridgeOpen)
        {
            interactionCanvas.gameObject.SetActive(false);
            fridgeUI.SetActive(true);
            StartCoroutine(AnimateFridgeUI(fridgeClosedPosition, fridgeOpenPosition, () =>
            {
                isAnimating = false; // Fin de l'animation
            }));
        }
        else
        {
            StartCoroutine(AnimateFridgeUI(fridgeOpenPosition, fridgeClosedPosition, () =>
            {
                fridgeUI.SetActive(false);
                isAnimating = false; // Fin de l'animation
            }));
        }

        Debug.Log(isFridgeOpen ? "Frigo ouvert" : "Frigo fermé");
    }

    private IEnumerator AnimateFridgeUI(Vector2 startPos, Vector2 endPos, System.Action onAnimationComplete)
    {
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationDuration); // Normalise entre 0 et 1
            float easedT = EaseOutElastic(t);                  // Utilise l'easing pour l'animation

            fridgeUIRect.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        fridgeUIRect.anchoredPosition = endPos;

        onAnimationComplete?.Invoke();
    }

    private float EaseOutElastic(float x)
    {
        const float c4 = (2 * Mathf.PI) / 3;

        if (x == 0)
            return 0;
        if (x == 1)
            return 1;

        return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }
}
