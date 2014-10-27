using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using FiledRecipes.App.Controls;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe) {
            
            this.Header = recipe.Name;
            ShowHeaderPanel();
            Console.WriteLine("\nIngredienser\n------------");
            foreach (Ingredient ingred in recipe.Ingredients)
            {
                Console.WriteLine(string.Format("{0,-3} {1,-3} {2,-3}", ingred.Amount, ingred.Measure, ingred.Name));            
            }
            Console.WriteLine("\nInstruktioner\n------------\n");
            foreach (string str in recipe.Instructions)
            {
                Console.WriteLine(string.Format("{0}\n",str));
            }
            
        }
        public void Show(IEnumerable<IRecipe> recipes) {

            foreach (IRecipe recipe in recipes)
            {
                this.Header = recipe.Name;
                ShowHeaderPanel();
                Console.WriteLine("\nIngredienser\n------------\n");
                foreach (Ingredient ingred in recipe.Ingredients)
                {
                    Console.WriteLine(string.Format("{0}\t{1}\t{2}", ingred.Amount, ingred.Measure, ingred.Name));
                }
                Console.WriteLine("\nInstruktioner\n------------");
                foreach (string str in recipe.Instructions)
                {
                    Console.WriteLine(string.Format("{0}\n", str));
                }
            }
            ContinueOnKeyPressed();
        }
    }
}
