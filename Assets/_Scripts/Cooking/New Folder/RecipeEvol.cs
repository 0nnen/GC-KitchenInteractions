using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeEvol : ScriptableObject
{
    public string title;                     // Titre de la recette
    public List<string> ingredients;         // Liste des ingr�dients
    [TextArea] public string description;    // Description de la recette
}   
