using CsvTransform;
using System;

namespace DataTx
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length != 2)
			{
				Console.WriteLine("Usage syntax is - 'DataTx.exe <inputFilePath> <outputFilePath>");
				System.Environment.Exit(0);
			}

			// Reads the app config section to get the list of Field transforms..

			var configuredTransforms = CsvFieldConfigReader.ReadCsvDataFieldTransformConfigs();
			Console.WriteLine("Finished reading the file transform configurations.\r\n");

			Console.WriteLine("Proceeding with converting input csv file to output csv file now.");

			// below call handles all of reading input file and generating the transformed output file .

			FileTxManager.TransformCsvDataFileAsync(args[0], args[1], 
				configuredTransforms.FieldTransforms.ToArray(), 
				configuredTransforms.OutputFieldNames.ToArray());

			Console.WriteLine("Done.");
		}
	}
}
