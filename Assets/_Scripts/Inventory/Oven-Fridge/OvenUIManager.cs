using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OvenSliderUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas sliderCanvas; // Canvas contenant le slider
    [SerializeField] private Slider fireSlider; // Slider de contrôle
    [SerializeField] private TMP_Text valueText; // Texte affichant la valeur du slider
    [SerializeField] private Transform sliderAnchor; // Objet 3D représentant le centre du slider

    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem smokeParticles; // Système de particules à ajuster
    [SerializeField] private float minStartSize = 0.5f; // Taille minimale des particules
    [SerializeField] private float maxStartSize = 3.0f; // Taille maximale des particules
    [SerializeField] private float sliderMinValue = 150f; // Valeur minimale du slider
    [SerializeField] private float sliderMaxValue = 900f; // Valeur maximale du slider

    [Header("Settings")]
    [SerializeField] private float interactionRange = 3.0f; // Distance pour afficher le slider
    [SerializeField] private Transform playerTransform; // Transform du joueur
    [SerializeField] private Vector3 canvasOffset = new Vector3(0, 0.5f, 0.5f); // Décalage du Canvas depuis l'ancre
    [SerializeField] private Vector3 fixedCanvasRotation = new Vector3(0, 0, 0); // Rotation fixe du Canvas
    [SerializeField] private bool dynamicOrientation = false; // Activer/désactiver la rotation dynamique vers le joueur

    private void Start()
    {
        // Désactiver le Canvas au départ
        if (sliderCanvas != null)
            sliderCanvas.gameObject.SetActive(false);

        // Assurez-vous que les particules jouent
        if (smokeParticles != null && !smokeParticles.isPlaying)
        {
            smokeParticles.Play();
        }

        // Initialisation des paramètres
        UpdateSliderValue();
        UpdateParticleSize();
    }

    private void Update()
    {
        HandleSliderVisibility();
        UpdateSliderValue();
        UpdateParticleSize();
    }

    private void HandleSliderVisibility()
    {
        if (sliderCanvas == null || sliderAnchor == null || playerTransform == null) return;

        // Calcul de la distance au joueur
        float distanceToPlayer = Vector3.Distance(playerTransform.position, sliderAnchor.position);

        // Gestion de la visibilité du slider
        if (distanceToPlayer <= interactionRange)
        {
            if (!sliderCanvas.gameObject.activeSelf)
            {
                sliderCanvas.gameObject.SetActive(true);
            }

            // Position et orientation du Canvas
            sliderCanvas.transform.position = sliderAnchor.position + canvasOffset;
            if (dynamicOrientation)
            {
                Vector3 directionToPlayer = playerTransform.position - sliderCanvas.transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer.normalized);
                sliderCanvas.transform.rotation = lookRotation;
            }
            else
            {
                sliderCanvas.transform.eulerAngles = fixedCanvasRotation;
            }
        }
        else
        {
            if (sliderCanvas.gameObject.activeSelf)
            {
                sliderCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSliderValue()
    {
        if (valueText == null || fireSlider == null) return;

        // Met à jour le texte avec la valeur du slider
        valueText.text = Mathf.RoundToInt(fireSlider.value).ToString();
    }

    private void UpdateParticleSize()
    {
        if (smokeParticles == null || fireSlider == null) return;

        // Normaliser la valeur du slider dans la plage [0, 1]
        float normalizedValue = (fireSlider.value - sliderMinValue) / (sliderMaxValue - sliderMinValue);

        // Calculer la nouvelle taille minimale en fonction du slider
        float newStartSize = Mathf.Lerp(minStartSize, maxStartSize, normalizedValue);

        // Appliquer la nouvelle valeur au startSize
        var mainModule = smokeParticles.main;
        var currentStartSize = mainModule.startSize;

        if (currentStartSize.mode == ParticleSystemCurveMode.TwoConstants)
        {
            mainModule.startSize = new ParticleSystem.MinMaxCurve(newStartSize, currentStartSize.constantMax);
        }
        else
        {
            mainModule.startSize = newStartSize;
        }
    }
}
