using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecipeTable))]
public class RecipeTableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RecipeTable recipeTable = (RecipeTable)target;
        if (recipeTable)
        {
            if (GUILayout.Button("Assign recipe IDs"))
            {
                recipeTable.AssignRecipeIDs();
            }
        }
    }
}
