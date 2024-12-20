using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RecipeBookManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI leftPageTitleText;
    [SerializeField] private TextMeshProUGUI leftPageIngredientsText;
    [SerializeField] private TextMeshProUGUI leftPageDescriptionText;
    [SerializeField] private TextMeshProUGUI rightPageTitleText;
    [SerializeField] private TextMeshProUGUI rightPageIngredientsText;
    [SerializeField] private TextMeshProUGUI rightPageDescriptionText;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button createRecipeButton;

    [Header("Popup")]
    [SerializeField] private GameObject recipePopup; // La popup pour créer une recette
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private TMP_InputField ingredientsInputField;
    [SerializeField] private TMP_InputField descriptionInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    [Header("Recipes")]
    [SerializeField] private List<RecipeEvol> recipes;

    private int currentPageIndex = 0;

    private void Start()
    {
        UpdatePages();

        previousPageButton.onClick.AddListener(GoToPreviousPage);
        nextPageButton.onClick.AddListener(GoToNextPage);
        createRecipeButton.onClick.AddListener(ShowPopup);

        confirmButton.onClick.AddListener(CreateNewRecipe);
        cancelButton.onClick.AddListener(HidePopup);

        recipePopup.SetActive(false); // Assure que la popup est initialement masquée
    }

    private void UpdatePages()
    {
        // Page gauche
        if (currentPageIndex < recipes.Count)
        {
            RecipeEvol leftPageRecipe = recipes[currentPageIndex];
            leftPageTitleText.text = leftPageRecipe.title;
            leftPageIngredientsText.text = string.Join("\n", leftPageRecipe.ingredients);
            leftPageDescriptionText.text = leftPageRecipe.description;
        }
        else
        {
            leftPageTitleText.text = "";
            leftPageIngredientsText.text = "";
            leftPageDescriptionText.text = "";
        }

        // Page droite
        int rightPageIndex = currentPageIndex + 1;
        if (rightPageIndex < recipes.Count)
        {
            RecipeEvol rightPageRecipe = recipes[rightPageIndex];
            rightPageTitleText.text = rightPageRecipe.title;
            rightPageIngredientsText.text = string.Join("\n", rightPageRecipe.ingredients);
            rightPageDescriptionText.text = rightPageRecipe.description;
        }
        else
        {
            rightPageTitleText.text = "";
            rightPageIngredientsText.text = "";
            rightPageDescriptionText.text = "";
        }

        previousPageButton.interactable = currentPageIndex > 0;
        nextPageButton.interactable = rightPageIndex < recipes.Count;
    }

    private void GoToPreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex -= 2; // Aller à la paire précédente
            UpdatePages();
        }
    }

    private void GoToNextPage()
    {
        if (currentPageIndex + 2 < recipes.Count)
        {
            currentPageIndex += 2; // Aller à la paire suivante
            UpdatePages();
        }
    }

    private void ShowPopup()
    {
        recipePopup.SetActive(true); // Affiche la popup
        titleInputField.text = "";
        ingredientsInputField.text = "";
        descriptionInputField.text = "";

        if (playerController != null)
        {
            playerController.SetMovementEnabled(false); // Désactiver les mouvements
        }
    }

    private void HidePopup()
    {
        recipePopup.SetActive(false); // Masque la popup

        if (playerController != null)
        {
            playerController.SetMovementEnabled(true); // Réactiver les mouvements
        }
    }

    private void CreateNewRecipe()
    {
        string title = titleInputField.text.Trim();
        string ingredientsRaw = ingredientsInputField.text.Trim();
        string description = descriptionInputField.text.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(ingredientsRaw) || string.IsNullOrEmpty(description))
        {
            Debug.LogWarning("Tous les champs doivent être remplis pour créer une recette.");
            return;
        }

        List<string> ingredients = new List<string>(ingredientsRaw.Split('\n'));

        RecipeEvol newRecipe = ScriptableObject.CreateInstance<RecipeEvol>();
        newRecipe.title = title;
        newRecipe.ingredients = ingredients;
        newRecipe.description = description;

        recipes.Add(newRecipe);

        Debug.Log($"Nouvelle recette ajoutée : {newRecipe.title}");
        UpdatePages();
        HidePopup(); // Masque la popup après ajout
    }
}
