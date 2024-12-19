using UnityEngine;
using TMPro;

public class PageManager : MonoBehaviour
{
    public TMP_Text leftPageTitle;
    public TMP_Text leftPageIngredients;
    public TMP_Text leftPageDescription;

    public TMP_Text rightPageTitle;
    public TMP_Text rightPageIngredients;
    public TMP_Text rightPageDescription;

    private int currentLeftRecipeIndex = 0;
    private int currentRightRecipeIndex = 1;
    private Book book;

    public void Initialize(Book book)
    {
        this.book = book;
        UpdatePages();
    }
    public int GetCurrentLeftRecipeIndex()
    {
        return currentLeftRecipeIndex;
    }

    public int GetCurrentRightRecipeIndex()
    {
        return currentRightRecipeIndex;
    }

    public void NextPage()
    {
        currentLeftRecipeIndex = Mathf.Min(currentLeftRecipeIndex + 2, book.recipes.Count - 2);
        currentRightRecipeIndex = Mathf.Min(currentRightRecipeIndex + 2, book.recipes.Count - 1);
        UpdatePages();
    }

    public void PreviousPage()
    {
        currentLeftRecipeIndex = Mathf.Max(currentLeftRecipeIndex - 2, 0);
        currentRightRecipeIndex = Mathf.Max(currentRightRecipeIndex - 2, 1);
        UpdatePages();
    }

    private void UpdatePages()
    {
        if (book != null)
        {
            Recipe leftRecipe = book.GetRecipe(currentLeftRecipeIndex);
            if (leftRecipe != null)
            {
                leftRecipe.DisplayRecipe(leftPageTitle, leftPageIngredients, leftPageDescription);
            }
            else
            {
                ClearPage(leftPageTitle, leftPageIngredients, leftPageDescription);
            }

            Recipe rightRecipe = book.GetRecipe(currentRightRecipeIndex);
            if (rightRecipe != null)
            {
                rightRecipe.DisplayRecipe(rightPageTitle, rightPageIngredients, rightPageDescription);
            }
            else
            {
                ClearPage(rightPageTitle, rightPageIngredients, rightPageDescription);
            }
        }
    }

    private void ClearPage(TMP_Text title, TMP_Text ingredients, TMP_Text description)
    {
        title.text = "";
        ingredients.text = "";
        description.text = "";
    }
}
