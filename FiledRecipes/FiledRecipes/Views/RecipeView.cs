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
            
        }
        public void Show(IEnumerable<IRecipe> recipes) {

            foreach (IRecipe recip in recipes)
            {
                throw new NotImplementedException();
            }
        }
    }
}
