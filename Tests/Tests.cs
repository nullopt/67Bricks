using _68Buns.Handlers;
using _68Buns.Models;
using NUnit.Framework;
using System.IO;

namespace Tests
{
	[TestFixture]
	public class Tests
	{
		private const string RECIPE_PATH_RAW = @"C:\TA\68Buns\Input\1_LemonCake.txt";
		private const string RECIPE_PATH_XML = @"C:\TA\68Buns\Example Output\1.xml";

		private Recipe InputXmlRecipe { get; set; }
		private Recipe OutputXmlRecipe { get; set; }

		[OneTimeSetUp]
		public void Setup()
		{
			// load the 1st recipe (raw and xml) as it's the example given
			this.OutputXmlRecipe = XmlReader.ReadXml<Recipe>(RECIPE_PATH_XML);

			var recipeConverter = new RecipeConverter();

			// convert the raw recipe to an xml formatted recipe file
			this.InputXmlRecipe = recipeConverter.GenerateRecipe(RECIPE_PATH_RAW, RECIPE_PATH_XML);
		}

		/// <summary>
		/// Test to check that the recipe id has been correctly implemented
		/// </summary>
		[Test]
		public void CheckRecipeId()
		{
			Assert.AreEqual(this.OutputXmlRecipe.Id, this.InputXmlRecipe.Id);
		}

		[Test]
		public void CheckTitle()
		{
			Assert.AreEqual(this.OutputXmlRecipe.Metadata.Title, this.InputXmlRecipe.Metadata.Title);
		}

		[Test]
		public void CheckAuthor()
		{
			Assert.AreEqual(this.OutputXmlRecipe.Metadata.Author, this.InputXmlRecipe.Metadata.Author);
		}

		[Test]
		public void CheckCreated()
		{
			Assert.AreEqual(this.OutputXmlRecipe.Metadata.Created, this.InputXmlRecipe.Metadata.Created);
		}

		[Test]
		public void CheckLead()
		{
			Assert.AreEqual(this.OutputXmlRecipe.Content.Lead, this.InputXmlRecipe.Content.Lead);
		}

		[Test]
		public void CheckIngredients()
		{
			Assert.That(this.InputXmlRecipe.Content.Ingredients.Ingredient, Is.EqualTo(this.OutputXmlRecipe.Content.Ingredients.Ingredient));
		}

		[Test]
		public void CheckMethod()
		{
			Assert.That(this.InputXmlRecipe.Content.Method.Step, Is.EqualTo(this.OutputXmlRecipe.Content.Method.Step));
		}
	}
}