using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CsvTransform
{
	public class CsvDataFieldTransforms
	{
		public int[] CsvFieldPositions;
		public IFieldTransform FieldTransformer;

		public CsvDataFieldTransforms(int[] fieldPositions, IFieldTransform fieldTransformer)
		{
			if (fieldTransformer.NeedsInputFields && 
				(fieldPositions == null || fieldPositions.Length == 0)) 
			{ 
				throw new ArgumentException(nameof(fieldPositions));
			}
			CsvFieldPositions = fieldPositions;
			FieldTransformer = fieldTransformer;
		}
	}

	public class TransformLog
	{
		public readonly long LineNumber;
		public readonly string InputLine;
		public readonly string ErrorMessage;

		public TransformLog(long lineNum, string input, string error)
		{
			LineNumber = lineNum;
			InputLine = input;
			ErrorMessage = error;
		}
	}

	public class TransformerOutput
	{
		public long StartLineNo;
		public List<string> OutputLines;
		public List<TransformLog> Errors;
	}

	public class CsvDataTransformer
	{
		private const char csvDelim = ',';
		private CsvDataFieldTransforms[] outputFieldTransforms;
		public CsvDataTransformer(CsvDataFieldTransforms[] dataFieldTransforms)
		{
			if (dataFieldTransforms == null || dataFieldTransforms.Length == 0) { throw new ArgumentException(nameof(dataFieldTransforms)); }

			this.outputFieldTransforms = dataFieldTransforms;
		}

		public TransformerOutput Transform(List<string> inputLines, long startLineNo)
		{
			// each block of lines processed here by noting the starting line no according to the input file..
			// the transformed output and error logs are stored in the TransformerOutput object

			TransformerOutput result = new TransformerOutput()
			{
				StartLineNo = startLineNo,
				OutputLines = new List<string>(),
				Errors = new List<TransformLog>()
			};

			// each line can be processed in parallel as there are no thread dependency between the lines
			// but I am finding a bug in inserting the output lines correctly in their sorted order.. 
			// as I am running out of time - I am switching this to a regular for loop but 
			// the bug should be easy to find and fix ..  
			//
			//SortedDictionary<int, string> loopOutputLines = new SortedDictionary<int, string>();
			//
			//Parallel.For(0, inputLines.Count, (index) =>
			for(int index = 0; index < inputLines.Count; ++index)
			{
				string inputLine = inputLines[index];
				string outputLine = string.Empty;
				TransformLog logOutput = null;
				long currentLineNum = startLineNo + (long)index;

				try
				{
					outputLine = TransformInputLine(currentLineNum, inputLine, out logOutput);
				}
				catch(Exception e)
				{
					logOutput = logOutput ?? new TransformLog(currentLineNum, inputLine, e.Message); //e.ToString());
				}

				result.OutputLines.Add(outputLine); //loopOutputLines[index] = outputLine;
				if (logOutput != null) { result.Errors.Add(logOutput); }
			} 
			// );

			// below block needed if parallel.for is used..
			// return the sorted list of output and errors by the line no.
			//result.OutputLines = loopOutputLines.Values.ToList();
			//result.Errors = result.Errors.OrderBy(e => e.LineNumber).ToList();

			return result;
		}

		private string TransformInputLine(long lineNum, string currentLine, out TransformLog logOutput)
		{
			List<string> outputFields = new List<string>();
			logOutput = null;

			if (string.IsNullOrWhiteSpace(currentLine)) 
			{
				logOutput = new TransformLog(lineNum, currentLine, "Found line with no valid input fields.");
				return string.Empty;
			}

			string[] fields = SplitEscapedFieldsInLine(currentLine); // this is used to handle escaped csv fields like for quantity - > "2,500.25" which will be output as "25000.25,kg"
			for(int i = 0; i < outputFieldTransforms.Length; ++i)
			{
				var inputFields = outputFieldTransforms[i].FieldTransformer.NeedsInputFields
					? outputFieldTransforms[i].CsvFieldPositions.Select(p => fields[p]).ToArray()
					: null;

				var transformedField = outputFieldTransforms[i].FieldTransformer.TransformField(inputFields);
				outputFields.Add(transformedField);
			}

			return string.Join(csvDelim.ToString(), outputFields);
		}

		private string[] SplitEscapedFieldsInLine(string currentLine)
		{
			const char quoteChar = '"';
			var fields = new List<string>();
			fields.AddRange(currentLine.Split(csvDelim));

			for(var i = 0; i < fields.Count; ++i)
			{
				var field = fields[i];
				if (field.Length > 0 && field[0] == quoteChar)
				{
					var k = i;
					fields[i] = fields[i].Substring(1);
					for (int j = i++; i < fields.Count; ++i)
					{
						var tempField = fields[i];
						if (tempField.Length > 0 && tempField[tempField.Length - 1] == quoteChar)
						{
							fields[i] = fields[i].Substring(0, tempField.Length - 1);
							fields[j] = string.Join(csvDelim.ToString(), fields.Skip(j).Take(i - j + 1).ToArray());
							fields.RemoveRange(j + 1, i - j);
							break;
						}
					}
					i = k;  // restore outer loop back to the saved index value k 
				}
			}

			return fields.ToArray();
		}
	}
}
