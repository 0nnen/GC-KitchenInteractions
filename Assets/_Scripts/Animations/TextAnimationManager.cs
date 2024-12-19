using UnityEngine;
using TMPro;

public class TextAnimationManager : MonoBehaviour
{
    [SerializeField] private RectTransform textTransform; // Le RectTransform du texte à animer
    [SerializeField] private float animationDuration = 1.0f; // Durée de l'animation
    [SerializeField] private float marginFromTop = 50f; // Marge depuis le haut de l'écran
    [SerializeField] private float delayBeforeFadeOut = 2.0f; // Délai avant la disparition

    private Vector2 startPosition; // Position de départ (hors écran)
    private Vector2 endPosition;   // Position d'arrivée (visible à l'écran)
    private Vector2 exitPosition;  // Position de sortie (hors écran en bas)
    private float timer = 0f;      // Temps écoulé pour l'animation
    private bool isAnimating = false;
    private bool isFadingOut = false;

    public void StartTextAnimation()
    {
        if (textTransform == null)
        {
            Debug.LogWarning("TextAnimationManager: Aucun RectTransform assigné.");
            return;
        }

        // Calculer les positions
        startPosition = new Vector2(0, Screen.height + textTransform.rect.height);
        endPosition = new Vector2(0, Screen.height - marginFromTop);
        exitPosition = new Vector2(0, -textTransform.rect.height); // Sortie par le bas

        // Initialiser l'état
        textTransform.anchoredPosition = startPosition;
        timer = 0f;
        isAnimating = true;
        isFadingOut = false;
    }

    private void Update()
    {
        if (!isAnimating) return;

        // Phase d'entrée
        if (!isFadingOut)
        {
            AnimateEntry();
        }
        // Phase de sortie
        else
        {
            AnimateExit();
        }
    }

    private void AnimateEntry()
    {
        // Calcul de la progression
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / animationDuration);

        // Application de l'animation easing
        float easedProgress = EaseOutExpo(progress);

        // Mise à jour de la position
        textTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedProgress);

        // Si l'animation d'entrée est terminée
        if (progress >= 1f)
        {
            isAnimating = false;
            Invoke(nameof(StartFadeOut), delayBeforeFadeOut); // Lancer la disparition après un délai
        }
    }

    private void StartFadeOut()
    {
        timer = 0f; // Réinitialiser le timer
        isAnimating = true;
        isFadingOut = true; // Passer à la phase de disparition
    }

    private void AnimateExit()
    {
        // Calcul de la progression
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / animationDuration);

        // Application de l'animation easing inversée
        float easedProgress = EaseOutExpo(progress);

        // Mise à jour de la position
        textTransform.anchoredPosition = Vector2.Lerp(endPosition, exitPosition, easedProgress);

        // Si l'animation de sortie est terminée
        if (progress >= 1f)
        {
            isAnimating = false;
            textTransform.gameObject.SetActive(false); // Désactiver l'objet une fois l'animation terminée
        }
    }

    private float EaseOutExpo(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }
}
