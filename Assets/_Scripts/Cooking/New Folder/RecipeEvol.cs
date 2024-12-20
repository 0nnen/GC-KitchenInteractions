using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeEvol : ScriptableObject
{
    public string title;                     // Titre de la recette
    public List<string> ingredients;         // Liste des ingrédients
    [TextArea] public string description;    // Description de la recette
}   
