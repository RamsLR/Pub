using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTransform
{
	public static class FileTxManager
	{
		public static void TransformCsvDataFile(string sourcePath, string targetPath, CsvDataFieldTransforms[] fieldTransformConfigs)
		{
			long currentLineNum = 0;

			if (!File.Exists(sourcePath)) throw new FileNotFoundException(sourcePath);

			CsvDataTransformer dataTransformer = new CsvDataTransformer(fieldTransformConfigs);
			List<Task<TransformerOutput>> transformTasks = new List<Task<TransformerOutput>>();

			foreach (var lines in ReadBlockOfLines(sourcePath, 100))
			{
				var task = Task<TransformerOutput>.Factory.StartNew(() => dataTransformer.Transform(lines, currentLineNum));
				transformTasks.Add(task);
				currentLineNum += lines.Count;
			}

			var targetStream = File.CreateText(targetPath);

			Task.WaitAny(transformTasks.ToArray());

			for


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
