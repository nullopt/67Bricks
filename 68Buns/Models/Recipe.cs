using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace _68Buns.Models
{
	// used https://json2csharp.com/xml-to-csharp to quickly convert to a class

	[XmlRoot(ElementName = "Metadata")]
	public class Metadata
	{

		[XmlElement(ElementName = "Created")]
		public DateTime Created { get; set; }

		[XmlElement(ElementName = "Author")]
		public string Author { get; set; }

		[XmlElement(ElementName = "Title")]
		public string Title { get; set; }
	}

	[XmlRoot(ElementName = "Ingredient")]
	public class Ingredient
	{

		[XmlElement(ElementName = "Amount", IsNullable = true)]
		public int? Amount { get; set; }

		[XmlElement(ElementName = "Unit", IsNullable = true)]
		public string? Unit { get; set; }

		[XmlElement(ElementName = "Item")]
		public string Item { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Ingredient ingredient &&
				   Amount == ingredient.Amount &&
				   Unit == ingredient.Unit &&
				   Item == ingredient.Item;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Amount, Unit, Item);
		}
	}

	[XmlRoot(ElementName = "Ingredients")]
	public class Ingredients
	{

		[XmlElement(ElementName = "Ingredient")]
		public List<Ingredient> Ingredient { get; set; }
	}

	[XmlRoot(ElementName = "Method")]
	public class Method
	{

		[XmlElement(ElementName = "Step")]
		public List<string> Step { get; set; }
	}

	[XmlRoot(ElementName = "Content")]
	public class Content
	{

		[XmlElement(ElementName = "Lead")]
		public string Lead { get; set; }

		[XmlElement(ElementName = "Ingredients")]
		public Ingredients Ingredients { get; set; }

		[XmlElement(ElementName = "Method")]
		public Method Method { get; set; }
	}

	[XmlRoot(ElementName = "Recipe")]
	public class Recipe
	{

		[XmlElement(ElementName = "Metadata")]
		public Metadata Metadata { get; set; }

		[XmlElement(ElementName = "Content")]
		public Content Content { get; set; }

		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }
	}
}
