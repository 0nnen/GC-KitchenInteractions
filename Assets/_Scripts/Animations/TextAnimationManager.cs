using UnityEngine;
using TMPro;

public class TextAnimationManager : MonoBehaviour
{
    [SerializeField] private RectTransform textTransform; // Le RectTransform du texte � animer
    [SerializeField] private float animationDuration = 1.0f; // Dur�e de l'animation
    [SerializeField] private float marginFromTop = 50f; // Marge depuis le haut de l'�cran
    [SerializeField] private float delayBeforeFadeOut = 2.0f; // D�lai avant la disparition

    private Vector2 startPosition; // Position de d�part (hors �cran)
    private Vector2 endPosition;   // Position d'arriv�e (visible � l'�cran)
    private Vector2 exitPosition;  // Position de sortie (hors �cran en bas)
    private float timer = 0f;      // Temps �coul� pour l'animation
    private bool isAnimating = false;
    private bool isFadingOut = false;

    public void StartTextAnimation()
    {
        if (textTransform == null)
        {
            Debug.LogWarning("TextAnimationManager: Aucun RectTransform assign�.");
            return;
        }

        // Calculer les positions
        startPosition = new Vector2(0, Screen.height + textTransform.rect.height);
        endPosition = new Vector2(0, Screen.height - marginFromTop);
        exitPosition = new Vector2(0, -textTransform.rect.height); // Sortie par le bas

        // Initialiser l'�tat
        textTransform.anchoredPosition = startPosition;
        timer = 0f;
        isAnimating = true;
        isFadingOut = false;
    }

    private void Update()
    {
        if (!isAnimating) return;

        // Phase d'entr�e
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

        // Mise � jour de la position
        textTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedProgress);

        // Si l'animation d'entr�e est termin�e
        if (progress >= 1f)
        {
            isAnimating = false;
            Invoke(nameof(StartFadeOut), delayBeforeFadeOut); // Lancer la disparition apr�s un d�lai
        }
    }

    private void StartFadeOut()
    {
        timer = 0f; // R�initialiser le timer
        isAnimating = true;
        isFadingOut = true; // Passer � la phase de disparition
    }

    private void AnimateExit()
    {
        // Calcul de la progression
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / animationDuration);

        // Application de l'animation easing invers�e
        float easedProgress = EaseOutExpo(progress);

        // Mise � jour de la position
        textTransform.anchoredPosition = Vector2.Lerp(endPosition, exitPosition, easedProgress);

        // Si l'animation de sortie est termin�e
        if (progress >= 1f)
        {
            isAnimating = false;
            textTransform.gameObject.SetActive(false); // D�sactiver l'objet une fois l'animation termin�e
        }
    }

    private float EaseOutExpo(float x)
    {
        return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
    }
}
