using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OvenUIManager : MonoBehaviour
{
    [Header("UI R�f�rences")]
    [SerializeField] private GameObject ovenUI;           // Canvas pour l'interface compl�te du four
    [SerializeField] private Canvas interactionCanvas;    // Canvas pour l'indication "Appuyez sur E"
    [SerializeField] private Slider fireSlider;           // Slider de contr�le du feu
    [SerializeField] private TMP_Text temperatureText;    // Texte pour afficher la temp�rature
    [SerializeField] private Transform slotGrid;          // Grille pour les slots
    [SerializeField] private GameObject slotPrefab;       // Prefab pour les slots
    [SerializeField] private Transform playerTransform;   // Transform du joueur

    [Header("Particules")]
    [SerializeField] private ParticleSystem fireParticles; // Syst�me de particules du feu
    [SerializeField] private float maxEmissionRate = 100f; // Taux d'�mission maximal des particules

    [Header("Param�tres")]
    [SerializeField] private float interactionRange = 3.0f; // Distance pour afficher le Canvas d'interaction
    [SerializeField] private int maxSlots = 2;             // Nombre maximum de slots dans le four

    private IngredientData[] cookingSlots;                // Slots de cuisson
    private bool isUIOpen = false;

    private void Awake()
    {
        cookingSlots = new IngredientData[maxSlots];
        ovenUI.SetActive(false); // D�sactive l'interface du four par d�faut
        if (interactionCanvas != null) interactionCanvas.gameObject.SetActive(false); // D�sactiver le Canvas "E"

        // Initialisation des valeurs du Slider
        if (fireSlider != null)
        {
            fireSlider.onValueChanged.AddListener(delegate { AdjustFire(); });
            AdjustFire(); // Met � jour la temp�rature d�s le d�but
        }
    }

    private void Update()
    {
        HandleInteractionCanvas();

        // Ouvrir ou fermer l'UI avec "E"
        if (Input.GetKeyDown(KeyCode.E) && IsPlayerInRange())
        {
            if (isUIOpen) CloseOvenUI();
            else OpenOvenUI();
        }

        // Toujours orienter le Slider et la temp�rature vers le joueur
        OrientSliderAndTemperature();

        // Met � jour les particules en fonction du slider
        UpdateFireParticles();
    }

    private void HandleInteractionCanvas()
    {
        if (interactionCanvas == null || playerTransform == null) return;

        if (IsPlayerInRange())
        {
            if (!interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(true);
            }

            // Oriente le Canvas vers le joueur
            Vector3 lookDirection = (playerTransform.position - interactionCanvas.transform.position).normalized;
            interactionCanvas.transform.forward = -lookDirection;
        }
        else
        {
            if (interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void OrientSliderAndTemperature()
    {
        if (fireSlider == null || temperatureText == null || playerTransform == null) return;

        // Oriente le Slider vers le joueur
        Vector3 lookDirection = (playerTransform.position - fireSlider.transform.position).normalized;
        fireSlider.transform.forward = -lookDirection;

        // Oriente le Texte de la temp�rature
        temperatureText.transform.forward = -lookDirection;
    }

    private bool IsPlayerInRange()
    {
        return Vector3.Distance(playerTransform.position, transform.position) <= interactionRange;
    }

    public void OpenOvenUI()
    {
        ovenUI.SetActive(true);
        isUIOpen = true;
        UpdateSlots();
    }

    public void CloseOvenUI()
    {
        ovenUI.SetActive(false);
        isUIOpen = false;
    }

    public void AdjustFire()
    {
        if (fireSlider == null || temperatureText == null) return;

        // Ajuste la temp�rature et le texte
        float fireLevel = fireSlider.value * maxEmissionRate;
        temperatureText.text = $"{Mathf.RoundToInt(fireSlider.value * 100)}�"; // Afficher la temp�rature
    }

    private void UpdateFireParticles()
    {
        if (fireParticles == null) return;

        var emission = fireParticles.emission;
        emission.rateOverTime = fireSlider.value * maxEmissionRate;

        var main = fireParticles.main;
        main.simulationSpeed = 0.5f + fireSlider.value; // Ajuste la vitesse des particules
    }

    public void AddToSlot(int slotIndex, IngredientData ingredient)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;

        cookingSlots[slotIndex] = ingredient;
        UpdateSlots();
    }

    private void UpdateSlots()
    {
        foreach (Transform child in slotGrid)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cookingSlots.Length; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotGrid);
            if (cookingSlots[i] != null)
            {
                slot.GetComponentInChildren<Image>().sprite = cookingSlots[i].ingredientSprite;
            }
        }
    }
}
