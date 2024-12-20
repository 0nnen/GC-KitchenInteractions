using UnityEngine;
using TMPro;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_Text popupTitleText;
    public TMP_Text popupIngredientsText;
    public TMP_Text popupDescriptionText;

    public void ShowPopup(Recipe recipe)
    {
        if (recipe != null)
        {
            popupTitleText.text = recipe.title;
            popupIngredientsText.text = string.Join("\n", recipe.ingredients);
            popupDescriptionText.text = recipe.description;
            popupPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
