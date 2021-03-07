using System;
using Shapes.FileParser;

namespace AreaProcessor
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length != 2)
			{
				Console.WriteLine("Usage syntax is - 'AreaPx.exe <inputFilePath> <outputFilePath>");
				System.Environment.Exit(0);
			}

			// Reads the app config section to get the list of shapes configured for Area computation

			Console.WriteLine("Process ingput file to compure area..");

			// below call handles all of reading input file and generating the transformed output file .

			FileProcessor.ProcessFileAsync(args[0], args[1]);

			Console.WriteLine("Done.");
		}
	}
}
