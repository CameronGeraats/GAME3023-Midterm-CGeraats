using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe Table", menuName = "ScriptableObjects/RecipeTable", order = 4)]
public class RecipeTable : ScriptableObject
{
    [SerializeField]
    private Recipe[] recipes;
    public Recipe GetRecipe(int id)
    {
        return recipes[id];
    }
    public void AssignRecipeIDs()
    {
        for (int i = 0; i < recipes.Length; i++)
        {
            try
            {
                recipes[i].RecipeID = i;
            }
            catch (RecipeException)
            {
                // this is fine
            }
        }
    }
    public int GetRecipeTableSize()
    {
        return recipes.Length;
    }
}

