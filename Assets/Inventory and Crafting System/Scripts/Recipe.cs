using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



class RecipeException : System.Exception
{
    public RecipeException(string message) : base(message)
    {

    }
}

[Serializable]
public class GridPair
{
    public int count;
    public Item item;
}


[CreateAssetMenu(fileName = "Recipe", menuName = "ScriptableObjects/Recipe", order = 3)]
public class Recipe : ScriptableObject
{
    [SerializeField]
    private int recipeID;

    public int RecipeID
    {
        get { return recipeID; }
        set
        {
            recipeID = value;
            throw new RecipeException("You never should have come here!");
        }
    }

    [SerializeField]
    private Item outputItem;
    public Item OutputItem
    {
        get { return outputItem; }
        private set { }
    }


    [SerializeField]
    private GridPair[] gridItems = new GridPair[9];
    public GridPair[] GridItems
    {
        get { return gridItems; }
        private set { }
    }


}
