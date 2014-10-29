using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using FiledRecipes.App.Controls;

namespace FiledRecipes.Views
{
    public class RecipeView : ViewBase, IRecipeView
    {
        /// <summary>
        /// Presents the specified recipie.
        /// </summary>
        /// <param name="recipe"></param>
        public void Show(IRecipe recipe) {
            
            //Sets the header of the view to the name of the recipie and displays it.
            this.Header = recipe.Name;
            ShowHeaderPanel();            
            //Loops through the ingredients of the recipie and displays them.
            Console.WriteLine("\nIngredienser\n------------");
            foreach (Ingredient ingred in recipe.Ingredients)
            {
                Console.WriteLine(string.Format("{0} {1} {2}", ingred.Amount, ingred.Measure, ingred.Name));            
            }
            //Loops through the instructions of the recipie and displays them.
            Console.WriteLine("\nInstruktioner\n------------\n");
            foreach (string str in recipe.Instructions)
            {
                Console.WriteLine(string.Format("{0}\n",str));
            }
            
        }
        /// <summary>
        /// Presents the ALL of the recipies.
        /// </summary>
        /// <param name="recipes"></param>
        public void Show(IEnumerable<IRecipe> recipes) {

            foreach (IRecipe recipe in recipes)
            {
                //Sets the header of the view to the name of the recipie and displays it.
                this.Header = recipe.Name;
                ShowHeaderPanel();
                //Loops through the ingredients of the recipie and displays them.
                Console.WriteLine("\nIngredienser\n------------\n");
                foreach (Ingredient ingred in recipe.Ingredients)
                {
                    Console.WriteLine(string.Format("{0} {1} {2}", ingred.Amount, ingred.Measure, ingred.Name));
                }
                //Loops through the instructions of the recipie and displays them.
                Console.WriteLine("\nInstruktioner\n------------");
                foreach (string str in recipe.Instructions)
                {
                    Console.WriteLine(string.Format("{0}\n", str));
                }
            }
            //Displays message & waits for the user to press to continue.
            ContinueOnKeyPressed();
        }
    }
}
