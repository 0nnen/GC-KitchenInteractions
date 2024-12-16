using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FridgeUIManager : MonoBehaviour
{
    [Header("UI Références")]
    [SerializeField] private GameObject fridgeUI;       // Canvas de l'interface du frigo
    [SerializeField] private Transform slotGrid;        // Grille des slots pour afficher les ingrédients
    [SerializeField] private GameObject slotPrefab;     // Prefab pour un slot
    [SerializeField] private GameObject tabButtonPrefab; // Prefab pour un bouton d'onglet
    [SerializeField] private Transform tabParent;       // Parent des onglets
    [SerializeField] private TMP_Text descriptionText;      // Texte de description
    [SerializeField] private Image ingredientImage;     // Image pour l'aperçu

    [Header("Paramètres")]
    public int maxSlotsPerTab = 9;  // Nombre de slots par onglet
    public int maxVisibleTabs = 3; // Nombre d'onglets visibles
    private List<List<IngredientData>> tabs = new List<List<IngredientData>>();
    private int currentTab = 0;

    private void Awake()
    {
        fridgeUI.SetActive(false); // Désactiver l'UI au démarrage
    }

    public void OpenFridgeUI(List<IngredientData> ingredients)
    {
        // Remplir les onglets
        OrganizeTabs(ingredients);

        // Afficher le premier onglet
        ShowTab(0);

        fridgeUI.SetActive(true);
    }

    public void CloseFridgeUI()
    {
        fridgeUI.SetActive(false);
    }

    private void OrganizeTabs(List<IngredientData> ingredients)
    {
        tabs.Clear();

        // Créer les onglets
        int slotCount = 0;
        List<IngredientData> currentTabItems = new List<IngredientData>();
        foreach (var ingredient in ingredients)
        {
            currentTabItems.Add(ingredient);
            slotCount++;

            if (slotCount >= maxSlotsPerTab)
            {
                tabs.Add(new List<IngredientData>(currentTabItems));
                currentTabItems.Clear();
                slotCount = 0;
            }
        }

        if (currentTabItems.Count > 0)
        {
            tabs.Add(currentTabItems);
        }

        // Créer les boutons d'onglet
        foreach (Transform child in tabParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < tabs.Count; i++)
        {
            GameObject tabButton = Instantiate(tabButtonPrefab, tabParent);
            tabButton.GetComponentInChildren<Text>().text = $"Onglet {i + 1}";
            int tabIndex = i;
            tabButton.GetComponent<Button>().onClick.AddListener(() => ShowTab(tabIndex));
        }
    }

    private void ShowTab(int tabIndex)
    {
        currentTab = tabIndex;

        foreach (Transform child in slotGrid)
        {
            Destroy(child.gameObject);
        }

        foreach (var ingredient in tabs[tabIndex])
        {
            GameObject slot = Instantiate(slotPrefab, slotGrid);
            slot.GetComponentInChildren<Image>().sprite = ingredient.ingredientSprite;
            slot.GetComponent<Button>().onClick.AddListener(() => ShowIngredientDetails(ingredient));
        }
    }

    private void ShowIngredientDetails(IngredientData ingredient)
    {
        descriptionText.text = ingredient.description;
        ingredientImage.sprite = ingredient.ingredientSprite;
    }
}
