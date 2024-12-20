using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private Image fadePanel; // Panel pour le fade-out
    [SerializeField] private float fadeDuration = 1.0f; // Dur�e du fade-out
    [SerializeField] private Slider loadingSlider; // Slider pour le chargement

    private void Start()
    {
        // Initialisez le fade panel transparent au d�but
        if (fadePanel != null)
            fadePanel.color = new Color(0, 0, 0, 0);

        // Cachez le slider au d�but
        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(false);
    }

    /// <summary>
    /// Changer de sc�ne avec transition et chargement
    /// </summary>
    /// <param name="sceneName">Nom de la sc�ne � charger</param>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithTransition(sceneName));
    }

    /// <summary>
    /// Quitter le jeu
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitter le jeu !");
        Application.Quit(); // Fonctionne uniquement dans une build
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Affiche le slider de chargement
        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(true);
            loadingSlider.value = 0; // R�initialise le slider
        }

        // D�marre la transition de fade-out
        if (fadePanel != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / fadeDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // Commence � charger la sc�ne
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false; // Emp�che l'activation imm�diate de la sc�ne

        // Met � jour le slider pendant le chargement
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // Normalize la progression
            if (loadingSlider != null)
                loadingSlider.value = progress;

            // Activer la sc�ne une fois le chargement termin�
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Une fois charg�, effectue un fade-out
        if (fadePanel != null)
        {
            float timer = fadeDuration;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / fadeDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        // Cache le slider apr�s la transition
        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(false);
    }
}
