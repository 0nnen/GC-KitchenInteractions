using UnityEngine;

public class IngredientComponent : MonoBehaviour
{
    private IngredientData ingredientData;

    public void SetIngredientData(IngredientData data)
    {
        ingredientData = data;
    }

    public IngredientData GetIngredientData()
    {
        return ingredientData;
    }
}
