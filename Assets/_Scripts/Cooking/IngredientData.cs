using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Cooking/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Informations")]
    public string ingredientName;        // Nom de l'ingr�dient
    public Sprite ingredientSprite;      // Texture ou ic�ne
    [TextArea] public string description; // Description de l'ingr�dient

    [Header("Cuisson")]
    public float cookingTime; // Temps de cuisson en secondes
}
