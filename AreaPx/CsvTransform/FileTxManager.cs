using Shapes.AreaParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shapes.FileParser
{
	public static class FileProcessor
	{
		private class FileProcessorOutput
		{
			public List<AreaResult> AreaResults;
			public List<ParserErrorLog> ErrorLines;
		}

		public static async void ProcessFileAsync(string sourcePath, string targetPath)
		{
			if (!File.Exists(sourcePath)) throw new FileNotFoundException(sourcePath);

			// generated the task batch of processing the input file..

			var areaProcessorTasks = GenerateAreaProcessorTasks(sourcePath);

			Task.WaitAny(areaProcessorTasks.ToArray());

			// process each completed task to get the output 

			var fpOutput = await GetFileProcessorOutput(areaProcessorTasks);

			// write the output and errors to the files..

			WriteAreaResultsOutput(targetPath, fpOutput);

		}

		private static void WriteAreaResultsOutput(string targetPath, FileProcessorOutput fpOutput)
		{
			using (StreamWriter ow = new StreamWriter(targetPath))
			using (StreamWriter lw = new StreamWriter(targetPath + ".log"))
			{
				ow.WriteLine("ShapeName, Area");
				fpOutput.AreaResults.ForEach(ar => ow.WriteLine(string.Join(" ", ar.ShapeName, ar.Area.ToString())));

				fpOutput.ErrorLines.ForEach(e => lw.WriteLine($"{e.LineNumber}|{e.ErrorMessage}|{e.InputLine}"));
			}
		}

		private static async Task<FileProcessorOutput> GetFileProcessorOutput(List<Task<ParserOutput>> areaProcessorTasks)
		{
			FileProcessorOutput fpOutput = new FileProcessorOutput { AreaResults = new List<AreaResult>(), ErrorLines = new List<ParserErrorLog>() };

			while (areaProcessorTasks.Any())
			{
				Task<ParserOutput> finishedTask = await Task.WhenAny(areaProcessorTasks);
				areaProcessorTasks.Remove(finishedTask);
				var parserOutput = await finishedTask;
				fpOutput.AreaResults.AddRange(parserOutput.OutputLines);
				fpOutput.ErrorLines.AddRange(parserOutput.Errors);
			}
			fpOutput.AreaResults = fpOutput.AreaResults.OrderByDescending(ar => ar.Area).ToList();
			return fpOutput;
		}

		private static List<Task<ParserOutput>> GenerateAreaProcessorTasks(string sourcePath)
		{
			List<Task<ParserOutput>> areaParserTasks = new List<Task<ParserOutput>>();
			long currentLineNum = 0;

			foreach (var lines in ReadBlockOfLines(sourcePath, 5))
			{
				// create a task for each block of lines - configured as 10 lines for now (can be increased to int.MAXVALUE)
				var taskStartLineNum = currentLineNum;
				var task = Task<ParserOutput>.Factory.StartNew(() => AreaParser.AreaParser.ProcessLines(lines, currentLineNum));
				areaParserTasks.Add(task);
				currentLineNum += lines.Count;
			}

			return areaParserTasks;
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
