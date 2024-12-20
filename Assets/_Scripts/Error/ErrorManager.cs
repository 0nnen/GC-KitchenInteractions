using UnityEngine;
using TMPro;
using System.Collections;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager Instance;

    [Header("UI Error Display")]
    [SerializeField] private TMP_Text errorText;            // Texte d'erreur
    [SerializeField] private CanvasGroup errorCanvasGroup;  // CanvasGroup pour gérer l'opacité
    [SerializeField] private float fadeDuration = 0.5f;     // Durée de l'animation (en secondes)
    [SerializeField] private float displayTime = 2f;        // Temps d'affichage du texte

    private bool isErrorMessageDisplayed = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowErrorMessage(string message)
    {
        if (isErrorMessageDisplayed) return; // Ignore si un message est déjà affiché

        isErrorMessageDisplayed = true;
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        if (errorCanvasGroup != null)
        {
            StartCoroutine(FadeInAndOut());
        }
    }

    private IEnumerator FadeInAndOut()
    {
        // Apparition (Fade-In)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float easedT = EaseInOutExpo(timer / fadeDuration); // Utilisation d'EaseInOutExpo
            errorCanvasGroup.alpha = Mathf.Lerp(0f, 1f, easedT);
            yield return null;
        }

        // Temps d'affichage
        yield return new WaitForSeconds(displayTime);

        // Disparition (Fade-Out)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float easedT = EaseInOutExpo(timer / fadeDuration); // Utilisation d'EaseInOutExpo
            errorCanvasGroup.alpha = Mathf.Lerp(1f, 0f, easedT);
            yield return null;
        }

        // Réinitialisation
        errorText.gameObject.SetActive(false);
        isErrorMessageDisplayed = false;
    }

    private float EaseInOutExpo(float x)
    {
        if (x == 0)
            return 0;
        if (x == 1)
            return 1;
        if (x < 0.5f)
            return Mathf.Pow(2f, 20f * x - 10f) / 2f;
        return (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
    }
}
