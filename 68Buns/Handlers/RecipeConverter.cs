using _68Buns.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace _68Buns.Handlers
{
	public class RecipeConverter
	{
		private static readonly Dictionary<string, string> UNIT_LOOKUP = new()
		{
			// TODO: add more if needed
			{ "g", "grams" },
		};

		private readonly string[] InvalidCharacters = new string[] { "\r\n", "\n", "\r" };

		private string RawFileContent { get; set; }

		public RecipeConverter() { }

		/// <summary>
		/// Converts a text based recipe to an xml recipe.
		/// </summary>
		/// <param name="inputFilePath"></param>
		/// <param name="outputFilePath"></param>
		/// <returns>A converted Recipe object</returns>
		/// <exception cref="InvalidDataException"></exception>
		public Recipe GenerateRecipe(string inputFilePath, string outputFilePath)
		{
			try
			{
				Console.WriteLine($"Loading file: {inputFilePath}");
				// read the file contents
				this.RawFileContent = File.ReadAllText(inputFilePath);

				if (this.RawFileContent.Length == 0)
				{
					// file is empty, throw exception
					throw new InvalidDataException("File content cannot be empty.");
				}

				var splitData = this.RawFileContent.Split('\n');

				// get the metadata line from the file content
				// ? we assume that the data is only on the first line
				var metaDataLine = splitData[0];
				// generate the metadata section of the xml
				var metaData = GetRecipeMetaData(metaDataLine, out var id);

				// get each section of the file
				var sections = this.GetRecipeSections(this.RawFileContent);

				// generate the content section of the xml
				var content = this.GetRecipeContent(sections);

				// combine all sections
				var recipe = new Recipe
				{
					Id = id,
					Metadata = metaData,
					Content = content
				};

				// serialise the recipe obj to the output xml file
				WriteToOutputPath(recipe, outputFilePath);

				return recipe;
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine($"Cannot find file: ${inputFilePath}");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return null;
		}

		/// <summary>
		/// Returns the ingredients and method section of a recipe
		/// </summary>
		/// <param name="recipeData"></param>
		/// <returns></returns>
		private Dictionary<string, IEnumerable<string>> GetRecipeSections(string recipeData)
		{
			try
			{
				// we're assuming that the lead will only take up 1 line here
				var lead = recipeData.Split('\n').Skip(2).Take(1);
				// get the ingredient section of the file
				var ingredientSection = this.GetIngredientSection(recipeData);
				// get the method section of the file
				var methodSection = this.GetMethodSection(recipeData);

				// return a new dictionary with the section key and values
				return new Dictionary<string, IEnumerable<string>>
				{
					{ "Lead", lead },
					{ "Ingredients", ingredientSection },
					{ "Method", methodSection }
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return null;
		}

		/// <summary>
		/// Returns the ingredient section of the recipe
		/// </summary>
		/// <param name="recipeData"></param>
		/// <returns></returns>
		/// <exception cref="InvalidDataException"></exception>
		private IEnumerable<string> GetIngredientSection(string recipeData)
		{
			// TODO: tweak this regex/come up with another method to catch all variations of user input
			//			as this fails on some of the recipes
			var ingredientPattern = @"ingredients[:{0,}](.*\n|\r\n?)+(\n|\r\n?)method[:{0,}]";
			var ingredientMatches = Regex.Matches(recipeData, ingredientPattern, RegexOptions.IgnoreCase);

			if (ingredientMatches.Count == 0)
			{
				throw new InvalidDataException("Unable to parse ingredient section");
			}

			return this.GetSectionLines(ingredientMatches);
		}

		/// <summary>
		/// Returns the method section of the recipe
		/// </summary>
		/// <param name="recipeData"></param>
		/// <returns></returns>
		/// <exception cref="InvalidDataException"></exception>
		private IEnumerable<string> GetMethodSection(string recipeData)
		{
			var methodPattern = @"method[:{0,}](.*\n?|\r\n?)+";
			var methodMatches = Regex.Matches(recipeData, methodPattern, RegexOptions.IgnoreCase);

			if (methodMatches.Count == 0)
			{
				throw new InvalidDataException("Unable to parse method section");
			}

			return this.GetSectionLines(methodMatches);
		}

		/// <summary>
		/// Returns the lines of the recipe from the section's matches
		/// </summary>
		/// <param name="matches"></param>
		/// <returns></returns>
		private IEnumerable<string> GetSectionLines(MatchCollection matches)
		{
			var result = new List<string>();
			// start at i = 1 and j = 1 to skip blank lines that were captured
			// TODO: more testing on this area as it can be the biggest breaking point
			for (var i = 1; i < matches[0].Groups.Count; i++)
			{
				var group = matches[0].Groups[i];
				//Console.WriteLine($"{i} | {matches[0].Groups[i].Value}");
				for (var j = 1; j < group.Captures.Count; j++)
				{
					var capture = group.Captures[j].Value;
					if (string.IsNullOrEmpty(capture) || this.InvalidCharacters.Contains(capture))
					{
						continue;
					}

					// add to list and remove line break characters
					result.Add(capture.Replace("\r", "").Replace("\n", ""));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the Metadata section of the recipe
		/// <br />
		/// Author, Title, Created
		/// </summary>
		/// <param name="line"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <exception cref="InvalidDataException"></exception>
		private static Metadata GetRecipeMetaData(string line, out int id)
		{
			var pattern = @"(\d+)\. (\w.+){1,} \[(\d.*)\]";
			var matches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);
			if (matches.Count == 0)
			{
				throw new InvalidDataException("Invalid metadata line.");
			}

			// ? we can assume that every first line of the recipe contains:
			// id, title, author and created date
			id = int.Parse(matches[0].Groups[1].Value);

			var titleAndAuthor = matches[0].Groups[2].Value;
			// TODO: possible issue if someone's name contains ' by ' - although unlikely
			var spl = titleAndAuthor.Split(" by ");
			var title = GetRecipeTitle(spl);
			var author = GetRecipeAuthor(spl);
			var createdDate = GetRecipeCreatedDate(matches);

			return new Metadata
			{
				Author = author,
				Title = title,
				Created = DateTime.Parse(createdDate)
			};
		}

		/// <summary>
		/// Returns the created date of the recipe
		/// </summary>
		/// <param name="matches"></param>
		/// <returns></returns>
		private static string GetRecipeCreatedDate(MatchCollection matches)
		{
			return matches[0].Groups[3].Value;
		}

		/// <summary>
		/// Returns the author of the recipe
		/// </summary>
		/// <param name="titleString"></param>
		/// <returns></returns>
		private static string GetRecipeAuthor(string[] titleString)
		{
			return titleString.Length > 1 ? titleString[1] : "";
		}

		/// <summary>
		/// Returns the title of the recipe
		/// </summary>
		/// <param name="titleString"></param>
		/// <returns></returns>
		private static string GetRecipeTitle(string[] titleString)
		{
			return titleString.Length > 1 ? titleString[0] : string.Join("", titleString);
		}

		/// <summary>
		/// Returns the lead of the recipe
		/// </summary>
		/// <param name="leadSection"></param>
		/// <returns></returns>
		private static string GetRecipeLead(IEnumerable<string> leadSection)
		{
			// we are assuming this only takes up 1 line - could break here if lead takes up more lines.
			return leadSection.FirstOrDefault().Replace("\n", "").Replace("\r", "");
		}

		/// <summary>
		/// Returns the content section of the recipe
		/// <br />
		/// Lead, Ingredients, Method
		/// </summary>
		/// <param name="contentSections"></param>
		/// <returns></returns>
		private Content GetRecipeContent(Dictionary<string, IEnumerable<string>> contentSections)
		{
			// get lead
			var lead = GetRecipeLead(contentSections["Lead"]);

			// get ingredients list - skip 3 as 0 = lead, 1 = blank line, 2 = "ingredients"
			var ingredients = this.GetRecipeIngredientList(contentSections["Ingredients"].ToArray());
			var method = this.GetMethodList(contentSections["Method"].ToArray());

			return new Content
			{
				Lead = lead,
				Ingredients = ingredients,
				Method = method
			};
		}

		/// <summary>
		/// Returns the full method list
		/// </summary>
		/// <param name="methodLines"></param>
		/// <returns></returns>
		private Method GetMethodList(string[] methodLines)
		{
			var method = new Method
			{
				Step = new List<string>()
			};

			for (var i = 0; i < methodLines.Length; i++)
			{
				var m = methodLines[i];

				if (string.IsNullOrEmpty(m) || this.InvalidCharacters.Contains(m))
				{
					continue;
				}

				// TODO: strip the step number -> 1. prepare the ingredients -> prepare the ingredients
				method.Step.Add(m);
			}

			return method;
		}

		/// <summary>
		/// Returns the full list of ingredients
		/// </summary>
		/// <param name="ingredientLines"></param>
		/// <returns></returns>
		private Ingredients GetRecipeIngredientList(string[] ingredientLines)
		{
			var ingredients = new Ingredients
			{
				Ingredient = new List<Ingredient>()
			};

			for (var i = 0; i < ingredientLines.Length; i++)
			{
				var line = ingredientLines[i];

				if (string.IsNullOrEmpty(line) || this.InvalidCharacters.Contains(line))
				{
					continue;
				}

				var ingredient = GetRecipeIngredient(line);
				ingredients.Ingredient.Add(ingredient);
			}

			return ingredients;
		}

		/// <summary>
		/// returns the Ingredient object from the line
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static Ingredient GetRecipeIngredient(string line)
		{
			var amount = 0;
			var unit = string.Empty;
			var item = string.Empty;

			// check if there is an ammount present
			// we can check if the first character is a letter or not
			if (!char.IsLetter(line[0]))
			{
				// regex for amount, unit and item
				// TODO: Breaks on 1/2 and symbols
				var pattern = @"(\d+)(\w{0,})\s(.*)";
				var matches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);

				if (matches.Count == 0)
				{
					// unable to parse correctly, so return everything as the item
					return new Ingredient { Item = line };
				}

				if (!int.TryParse(matches[0].Groups[1].Value, out amount))
				{
					throw new InvalidDataException("Invalid amount");
				}

				var rawUnit = matches[0].Groups[2].Value;
				unit = UNIT_LOOKUP.GetValueOrDefault(rawUnit, rawUnit);

				item = matches[0].Groups[3].Value;
			}

			return new Ingredient
			{
				Amount = amount == 0 ? null : amount,
				// this produces <Unit xsi:nil="true" /> if null
				Unit = unit == string.Empty ? null : unit,
				Item = item
			};
		}

		/// <summary>
		/// Serialises and writes the Recipe object to the outputFilePath
		/// </summary>
		/// <param name="recipe"></param>
		/// <param name="outputFolder"></param>
		private static void WriteToOutputPath(Recipe recipe, string outputFolder)
		{
			var serializer = new XmlSerializer(typeof(Recipe));
			var streamWriter = new StreamWriter(Path.Combine(outputFolder, $"{recipe.Id}.xml"));
			serializer.Serialize(streamWriter, recipe);
			streamWriter.Close();
			Console.WriteLine($"Sucessfully wrote to file: {outputFolder}");
		}
	}
}
