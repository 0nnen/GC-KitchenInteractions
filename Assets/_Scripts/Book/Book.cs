using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Book : MonoBehaviour
{
    public List<Recipe> recipes;


    public void AddRecipe(Recipe newRecipe)
    {
        recipes.Add(newRecipe);
    }

    public Recipe GetRecipe(int index)
    {
        if (index >= 0 && index < recipes.Count)
        {
            return recipes[index];
        }
        return null;
    }
}
