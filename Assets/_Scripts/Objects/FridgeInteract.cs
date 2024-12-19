using UnityEngine;
using TMPro;
using System.Collections;

public class FridgeInteract : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Canvas en World Space pour afficher le texte.")]
    [SerializeField] private GameObject interactCanvas;

    [Tooltip("TextMeshPro pour afficher le message.")]
    [SerializeField] private TMP_Text interactText;

    [Tooltip("UI principale du frigo.")]
    [SerializeField] private GameObject fridgeUI;

    [Tooltip("RectTransform du UI Fridge pour animation.")]
    [SerializeField] private RectTransform fridgeUIRect;

    [Tooltip("Transform du joueur.")]
    [SerializeField] private Transform playerTransform;

    [Header("Paramètres")]
    [Tooltip("Distance maximale pour interagir.")]
    [SerializeField] private float interactionRange = 3f;

    [Tooltip("Durée de l'animation en secondes.")]
    [SerializeField] private float animationDuration = 1f;

    private bool isPlayerInRange = false;
    private bool isFridgeOpen = false;
    private Vector2 fridgeClosedPosition = new Vector2(0, -500); // Hors écran
    private Vector2 fridgeOpenPosition = new Vector2(0, 0);      // Position visible

    private void Start()
    {
        interactCanvas.SetActive(false);
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

        if (distanceToPlayer <= interactionRange)
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                InteractionMessageHandler.Instance.ShowMessage("Appuyez sur E pour ouvrir le frigo", transform);
            }

            if (Input.GetKeyDown(KeyCode.E))
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

    private void ShowInteractionMessage(string message)
    {
        interactText.text = message;
        interactCanvas.SetActive(true);
    }

    private void HideInteractionMessage()
    {
        interactCanvas.SetActive(false);
    }

    private void ToggleFridgeUI()
    {
        isFridgeOpen = !isFridgeOpen;

        if (isFridgeOpen)
        {
            interactCanvas.SetActive(false);
            fridgeUI.SetActive(true);
            StartCoroutine(AnimateFridgeUI(fridgeClosedPosition, fridgeOpenPosition));
        }
        else
        {
            StartCoroutine(AnimateFridgeUI(fridgeOpenPosition, fridgeClosedPosition, onAnimationComplete: () =>
            {
                fridgeUI.SetActive(false);
            }));
        }

        Debug.Log(isFridgeOpen ? "Frigo ouvert" : "Frigo fermé");
    }

    private IEnumerator AnimateFridgeUI(Vector2 startPos, Vector2 endPos, System.Action onAnimationComplete = null)
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
