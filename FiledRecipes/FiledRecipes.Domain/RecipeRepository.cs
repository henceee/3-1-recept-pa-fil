using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        /// <summary>
        /// Loads recipes and formats them into sections of
        /// Ingredients, and Instructions by assigning references to
        /// Recipe and Ingredient objects.
        /// </summary>

        public void Load() {
                    
            //strings representing the current line, and the next line to be read.
            string line = null;
            string nextline = null;

            using (StreamReader Listreader = new StreamReader(_path)) {

                line = Listreader.ReadLine();
                nextline = Listreader.ReadLine();
                /*  reference to an Recipe obj. to be assigned, using the 
                *   variable name as argument to Recipe
                */
                Recipe Recip = null;
                string name = null;
                //Reference to an IIngredient
                Ingredient Ingred;    
                //Temporary storage of Ingrediences and Instructions
                List<string> Ingrediences = new List<string>();
                List<string> Instructions = new List<string>();

                while (line != null)
                {
                    if (line.Contains(string.Empty))
                    {
                        //Skips the empty string.
                        continue;    
                    }
                    else if (line.Contains(SectionRecipe))
                    {
                        //The name of the Recipe is set to the value of the next line.
                        name = nextline;
                    }

                    if (line.Contains(SectionIngredients))
                    {
                        /*  The nexcoming lines, until the next section are stored in 
                         *  the temporary list for now.
                         */
                        while (line != SectionInstructions) {

                            Ingrediences.Add(nextline);
                        }
                    }
                    if (line.Contains(SectionInstructions))
                    {
                        /*  The nexcoming lines, until the next section are stored in 
                         *  the temporary list for now.
                         */
                        while (line != SectionRecipe)
                        {
                            Instructions.Add(nextline);
                        }
                    }
                    else if (line == name)
                    {
                        //Creates a new Recipe object, using the name aquired previously, stored in variable name.
                        Recip = new Recipe(name);
                    }
                    else if (Ingrediences.Contains(line))
                    {
                            /*  Splits the string into what should be 3 parts
                             *  however, if that is not the case, an exception is thrown.
                             */
                            string[] IngredienceArr = line.Split(';');

                            if (IngredienceArr.Length > 3)
                            {
                                throw new FileFormatException();
                            }
                            //Sets each part to respective property of the Ingredient-obj. (Amount, Measure & Name)
                            Ingred = new Ingredient();
                            Ingred.Amount = IngredienceArr[0];
                            Ingred.Measure = IngredienceArr[1];
                            Ingred.Name = IngredienceArr[2];
                    }
                    else if (Instructions.Contains(line)) {
                        /*  Tries to an line of the Instructions to the recipe, 
                         *  however if the procedure fails, an exception is thrown.
                         */
                        try
                        {
                            Recip.Add(line);
                        }
                        catch (Exception)
                        {                            
                            throw new FileFormatException();
                        }
                    }
                    
                     //Adds the recepie to the list & sorts it.                      
                    _recipes.Add(Recip);
                    _recipes.Sort();
                    IsModified = false;
                    OnRecipesChanged(EventArgs.Empty);                    
                }
            }       
        }

        /// <summary>
        /// Saves the recipe.
        /// </summary>
        public void Save()
        {
            using (StreamWriter Sr = new StreamWriter(_path))
            {


            }

        }
       
    }
}
