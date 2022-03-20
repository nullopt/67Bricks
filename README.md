# 68 Buns Recipe Converter


## Overview
This program takes an directory of plain text recipes and converts them to `.xml` files.  

## Time Taken
This took 2:14hr over the course of 2 days - slightly over to finish the documentation.

## Tests
In the `68Buns\Tests\Tests.cs` file, update the `RECIPE_PATH_RAW` and `RECIPE_PATH_XML` variables to match your input and outfile files. You should then be able to run the `NUnit` tests.

## Thought and implementation process
I did some background research on XML de/serialisation as it's not something I've done a lot of work on before. The syntax for doing so in .NET is very similar to handling JSON files, so this felt familiar.

I decided to follow a TDD approach as I was provided with an example input and example output. From here I can just input this data into my `NUnit` test cases and develop my code around the success of the tests.  
I wrote a test for each section of `.xml` output and a very quick deserialiser for the example XML file; while
using [Json2CSharp - XML to C#](https://json2csharp.com/xml-to-csharp), I instantly generated the models I needed.

From here, I began working top down; implementing each function that would generate each section of the desired output.  
This way for coding made it very easy for me to create a working demo. This could then be worked on by another developer to fix the small issues I noted in the code and [Challenges and Issues](#challenges-and-issues).

## Challenges and issues
The biggest challenge I faced was trying to create a Regex pattern that would match all variations of user input. This might be an fruitless task, and perhaps there is a much simpler solution that I'm missing.

There were also a few formatting issues using the serialiser, but maybe these could be fixed with a few tweaks of the options or the models themselves.

## TODO:
- Fix the `RecipeConverter::GetIngredientSection` regex pattern as it breaks on some of the recipes.
- Add more tests for the individual functions, rather than just the outputs.
- Add support for `1/2`, `½` etc.
- Strip the step numbers from the Method section.