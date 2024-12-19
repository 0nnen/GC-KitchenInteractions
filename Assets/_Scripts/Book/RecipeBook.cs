using UnityEngine;
using UnityEngine.UI;

public class RecipeBook : MonoBehaviour
{
    public PageManager pageManager;
    public PopupManager popupManager;
    public RecipeCreationManager recipeCreationManager;

    public Button leftPageButton;
    public Button rightPageButton;
    public Button nextPageButton;
    public Button previousPageButton;
    public Button closePopupButton;
    public Button newRecipeButton;
    public Button saveButton;

    private Book book;

    void Start()
    {
        book = FindObjectOfType<Book>();

        pageManager.Initialize(book);
        recipeCreationManager.Initialize(book, null);

        leftPageButton.onClick.AddListener(() => popupManager.ShowPopup(book.GetRecipe(pageManager.GetCurrentLeftRecipeIndex())));
        rightPageButton.onClick.AddListener(() => popupManager.ShowPopup(book.GetRecipe(pageManager.GetCurrentRightRecipeIndex())));

        nextPageButton.onClick.AddListener(pageManager.NextPage);
        previousPageButton.onClick.AddListener(pageManager.PreviousPage);
        closePopupButton.onClick.AddListener(popupManager.ClosePopup);

        newRecipeButton.onClick.AddListener(recipeCreationManager.OpenNewRecipePanel);
        saveButton.onClick.AddListener(recipeCreationManager.SaveNewRecipe);
    }
}
