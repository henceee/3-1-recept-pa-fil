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

        public void Load()
        {
            
            using (StreamReader Listreader = new StreamReader(_path))
            {
                Recipe Recip = null;
                IIngredient Ingred;
                //Variable that sets the status of the nextcoming row(s)
                RecipeReadStatus status = RecipeReadStatus.Indefinite;
                string line;

                while ((line = Listreader.ReadLine()) != null)
                {
                    //if the line is empty, reader keeps on reading.
                    if(line.Trim().Length == 0){
                        continue;
                    }
                    else
                    {   /*  If the section divider indicates a new recipe,
                         *  the status of the next rows are set to New.
                         */
                        if (line.Contains(SectionRecipe))
                        {
                            status = RecipeReadStatus.New;
                        }
                        /*  If the section divider indicates that ingrediences are to
                         *  follow, the status of the next rows are set to Ingredient.
                         */
                        else if (line.Contains(SectionIngredients))
                        {
                            status = RecipeReadStatus.Ingredient;
                        }
                        /*  If the section divider indicates that instructions are to
                        *  follow, the status of the next rows are set to Instructions.
                        */
                        else if (line.Contains(SectionInstructions))
                        {
                            status = RecipeReadStatus.Instruction;
                        } 
                        //Checks the status of the current line.
                        if (status == RecipeReadStatus.New || status ==RecipeReadStatus.Ingredient ||
                            status == RecipeReadStatus.Instruction)
                        {
                            /*  If the status is set to New, and is the next line
                             *  after the section divider, that means it is the name
                             *  of the recipe. A new instance of Recipe is created
                             *  using that line.
                             */
                            if (status == RecipeReadStatus.New && line != SectionRecipe)
                            {
                                Recip = new Recipe(line);
                            }
                            /*  If the status is set to Ingredient, and is the next line
                            *   after the section divider, that means it is an ingredient..
                            */
                            else if (status == RecipeReadStatus.Ingredient && line != SectionIngredients)
                            {
                                /*  The line is split into what should be 3 parts,
                                 *  otherwise an exception is thrown.
                                 */
                                string[] ingredienceArr = line.Split(new char[] { ';' });
                                if (ingredienceArr.Length < 3)
                                {
                                    throw new FileFormatException();
                                }
                                /*  Sets each part to respective property of the 
                                 *  Ingredient-obj.(Amount, Measure & Name)
                                 *  and adds the ingredient to the recipe.
                                 */
                                Ingred = new Ingredient();
                                Ingred.Amount = ingredienceArr[0];
                                Ingred.Measure = ingredienceArr[1];
                                Ingred.Name = ingredienceArr[2];
                                Recip.Add(Ingred);
                            }
                            /*  If the status is set to Instruction, and is the next line
                            *  after the section divider, that means it is an instruction..
                            */
                            else if (status == RecipeReadStatus.Instruction && line != SectionInstructions)
                            {
                                /*  .. and should be able to be added to the recipe.
                                 *  otherwise the format is incorrect and an exception is thrown. 
                                 */
                                try
                                {
                                    Recip.Add(line);
                                }
                                catch (Exception)
                                {
                                    throw new FileFormatException();
                                }
                                /*  If the recipe allready exists in the list of recipes
                                 *  it is deleted to prevent doubles. The recipe is then added
                                 *  to the list of recipes and sorted.
                                 */
                                if (_recipes.Contains(Recip))
                                {
                                    _recipes.Remove(Recip);
                                    
                                }
                                 _recipes.Add(Recip);                                
                                _recipes.Sort();
                                //The recipe is unmodified.
                                IsModified = false;
                                OnRecipesChanged(EventArgs.Empty);
                            } 
                        }
                    }                    
                }                
            }
            
        }

        /// <summary>
        /// Saves the recipes using all the recipies in the list _recipes
        /// in the correct formatting.
        /// </summary>
        public void Save()
        {
            using (StreamWriter Sr = new StreamWriter(_path))
            {
                foreach (Recipe recip in _recipes)
                {
                    /*Writes the section divider for new recipe,
                    /*the name of the recipe & the section
                     * for ingredients to the file.
                     */
                    Sr.Write(SectionRecipe);
                    Sr.Write(recip.Name);
                    Sr.Write(SectionIngredients);
                    //Writes the amout, measure and name of each ingredient of the recipe to the file.
                    foreach (Ingredient ingred in recip.Ingredients) {
                        Sr.Write(string.Format("{0};{1};{2}",ingred.Amount, ingred.Measure, ingred.Name));
                    }
                    //Writes the section divider for instructions, and all the instructions to the file.
                    Sr.Write(SectionInstructions);
                    Sr.Write(recip.Instructions);
                }
            }
            //The recipe is unmodified.
            IsModified = false;
            OnRecipesChanged(EventArgs.Empty);
        }
       
    }
}
