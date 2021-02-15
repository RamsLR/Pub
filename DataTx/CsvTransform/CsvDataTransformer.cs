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
			if (fieldPositions == null || fieldPositions.Length == 0) { throw new ArgumentException(nameof(fieldPositions));  }
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
			TransformerOutput result = new TransformerOutput()
			{
				StartLineNo = startLineNo,
				OutputLines = new List<string>(),
				Errors = new List<TransformLog>()
			};

			Parallel.For(0, inputLines.Count, (index) =>
			{
				TransformLog logOutput;
				var output = TransformInputLine(startLineNo + (long )index, inputLines[index], out logOutput);

				result.OutputLines.Add(output);
				if (logOutput != null) { result.Errors.Add(logOutput); }
			});

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

			string[] fields = currentLine.Split(csvDelim);
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
	}
}
