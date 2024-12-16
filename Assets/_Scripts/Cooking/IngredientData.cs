using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Cooking/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Informations")]
    public string ingredientName;        // Nom de l'ingrédient
    public Sprite ingredientSprite;      // Texture ou icône
    [TextArea] public string description; // Description de l'ingrédient

    [Header("Cuisson")]
    public float cookingTime; // Temps de cuisson en secondes
}
