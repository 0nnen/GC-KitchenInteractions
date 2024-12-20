using UnityEngine;
using TMPro;

public class RecipeCreationManager : MonoBehaviour
{
    public GameObject newRecipePanel;
    public TMP_InputField titleInputField;
    public TMP_InputField ingredientsInputField;
    public TMP_InputField descriptionInputField;

    private Book book;
    private MonoBehaviour playerMovementScript;

    public void Initialize(Book book, MonoBehaviour playerMovementScript)
    {
        this.book = book;
        this.playerMovementScript = playerMovementScript;
    }

    public void OpenNewRecipePanel()
    {
        newRecipePanel.SetActive(true);
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
    }

    public void SaveNewRecipe()
    {
        string title = titleInputField.text;
        string ingredients = ingredientsInputField.text;
        string description = descriptionInputField.text;

        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(ingredients) && !string.IsNullOrEmpty(description))
        {
            Recipe newRecipe = ScriptableObject.CreateInstance<Recipe>();
            newRecipe.title = title;
            newRecipe.ingredients = ingredients.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            newRecipe.description = description;

            book.AddRecipe(newRecipe);

            titleInputField.text = "";
            ingredientsInputField.text = "";
            descriptionInputField.text = "";

            CloseNewRecipePanel();
        }
        else
        {
            Debug.LogError("Tous les champs doivent être remplis !");
        }
    }

    public void CloseNewRecipePanel()
    {
        newRecipePanel.SetActive(false);
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }
}
