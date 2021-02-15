using CsvTransform;
using System;

namespace DataTx
{
	class Program
	{
		static void Main(string[] args)
		{
			//if (args == null || args.Length != 2)
			//{
			//	Console.WriteLine("Usage syntax is - 'DataTx.exe <inputFilePath> <outputFilePath>");
			//	System.Environment.Exit(0);
			//}

			var configuredTransforms = CsvFieldConfigReader.ReadCsvDataFieldTransformConfigs();

			Console.WriteLine("Finished reading the file transform configurations.\r\n");
			Console.WriteLine("Proceeding with converting input csv file to output csv file now.");

			FileTxManager.TransformCsvDataFileAsync(args[0], args[1], 
				configuredTransforms.FieldTransforms.ToArray(), 
				configuredTransforms.OutputFieldNames.ToArray());

			Console.WriteLine("Done.");
		}
	}
}
