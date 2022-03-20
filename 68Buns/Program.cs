using _68Buns.Handlers;
using System;
using System.IO;

namespace _68Buns
{
	class Program
	{
		static void Main(string[] args)
		{
			// first cmd arg is input
			var inputFolderPath = args[0];
			// second cmd arg is output
			var outputFolderPath = args[1];

			// create the recipe converter obj
			var recipeConverter = new RecipeConverter();

			// loop through all files in the input folder
			foreach (var file in Directory.GetFiles(inputFolderPath))
			{
				// generate the recipe
				_ = recipeConverter.GenerateRecipe(file, outputFolderPath);
			}

			Console.ReadLine();
		}
	}
}
