using UnityEngine;
using UnityEngine.UI;

public class OvenUIManager : MonoBehaviour
{
    [Header("UI Références")]
    [SerializeField] private GameObject ovenUI;          // Canvas de l'interface du four
    [SerializeField] private Transform slotGrid;         // Grille des 3 slots
    [SerializeField] private GameObject slotPrefab;      // Prefab pour un slot
    [SerializeField] private Slider fireSlider;          // Slider pour contrôler le feu

    [Header("Cuisson")]
    [SerializeField] private float maxCookingTime = 300f; // Temps max de cuisson

    private IngredientData[] cookingSlots = new IngredientData[3];

    private void Awake()
    {
        ovenUI.SetActive(false); // Désactiver l'UI au démarrage
    }

    public void OpenOvenUI()
    {
        ovenUI.SetActive(true);
        UpdateSlots();
    }

    public void CloseOvenUI()
    {
        ovenUI.SetActive(false);
    }

    public void AddToSlot(int slotIndex, IngredientData ingredient)
    {
        if (slotIndex < 0 || slotIndex >= cookingSlots.Length) return;

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

    public void AdjustFire()
    {
        float fireLevel = fireSlider.value * maxCookingTime;
        Debug.Log($"Niveau de feu réglé sur : {fireLevel}");
    }
}
