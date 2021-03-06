using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTransform
{
	public static class FileTxManager
	{
		public static async void TransformCsvDataFileAsync(string sourcePath, string targetPath, 
			CsvDataFieldTransforms[] fieldTransformConfigs, string[] outputFieldNames)
		{
			if (!File.Exists(sourcePath)) throw new FileNotFoundException(sourcePath);

			CsvDataTransformer dataTransformer = new CsvDataTransformer(fieldTransformConfigs);

			// generated the task batch of processing the input file..

			var transformTasks = GenerateTransformTasks(sourcePath, dataTransformer);

			Task.WaitAny(transformTasks.ToArray());

			// process each completed task to get the output 

			var txOutputList = await GetTransformedOutput(transformTasks);

			// write the output and errors to the files..

			WriteTransformedOutput(targetPath, outputFieldNames, txOutputList);

		}

		private static void WriteTransformedOutput(string targetPath, string[] outputFieldNames, List<TransformerOutput> txOutputList)
		{
			using (StreamWriter ow = new StreamWriter(targetPath))
			using (StreamWriter lw = new StreamWriter(targetPath + ".log"))
			{
				ow.WriteLine(string.Join(",", outputFieldNames));
				foreach (var to in txOutputList)
				{
					to.OutputLines.ForEach(o => ow.WriteLine(o));
					to.Errors.ForEach(e => lw.WriteLine($"{e.LineNumber}|{e.ErrorMessage}|{e.InputLine}"));
				}
			}
		}

		private static async Task<List<TransformerOutput>> GetTransformedOutput(List<Task<TransformerOutput>> transformTasks)
		{
			List<TransformerOutput> txOutputList = new List<TransformerOutput>();

			while (transformTasks.Any())
			{
				Task<TransformerOutput> finishedTask = await Task.WhenAny(transformTasks);
				transformTasks.Remove(finishedTask);
				var txOutput = await finishedTask;
				txOutputList.Add(txOutput);
			}
			txOutputList = txOutputList.OrderBy(l => l.StartLineNo).ToList();
			return txOutputList;
		}

		private static List<Task<TransformerOutput>> GenerateTransformTasks(string sourcePath, CsvDataTransformer dataTransformer)
		{
			List<Task<TransformerOutput>> transformTasks = new List<Task<TransformerOutput>>();
			long currentLineNum = 0;

			foreach (var lines in ReadBlockOfLines(sourcePath, 10))
			{
				// create a task for each block of lines - configured as 10 lines for now (can be increased to int.MAXVALUE)
				var taskStartLineNum = currentLineNum;
				var task = Task<TransformerOutput>.Factory.StartNew(() => dataTransformer.Transform(lines, taskStartLineNum));
				transformTasks.Add(task);
				currentLineNum += lines.Count;
			}

			return transformTasks;
		}

		private static IEnumerable<List<string>> ReadBlockOfLines(string fileName, int maxLinesInBlock = 100)
		{
			using (var reader = new StreamReader(fileName))
			{
				while (!reader.EndOfStream)
				{
					var lines = new List<string>();
					for (var i = 0; i < maxLinesInBlock && !reader.EndOfStream; i++)
					{
						lines.Add(reader.ReadLine());
					}
					yield return lines;
				}
			}
		}
	}
}
