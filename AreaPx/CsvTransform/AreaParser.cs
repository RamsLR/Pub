using System;
using System.Collections.Generic;
using Shapes.AreaCalculators;

namespace Shapes.AreaParser
{
	public class AreaResult
	{
		public string ShapeName;
		public double Area;
	}

	public class ParserOutput
	{
		public long StartLineNo;
		public List<AreaResult> OutputLines;
		public List<ParserErrorLog> Errors;
	}

	public class ParserErrorLog
	{
		public readonly long LineNumber;
		public readonly string InputLine;
		public readonly string ErrorMessage;

		public ParserErrorLog(long lineNum, string input, string error)
		{
			LineNumber = lineNum;
			InputLine = input;
			ErrorMessage = error;
		}
	}

	public static class AreaParser
	{
		private const char spaceDelim = ' ';
		private static readonly Dictionary<string, IShapeAreaCalculator> shapeAreaCalculators = null;

		static AreaParser()
		{
			shapeAreaCalculators = new Dictionary<string, IShapeAreaCalculator>();
			IShapeAreaCalculator c = new CircleAreaCalculator();
			shapeAreaCalculators.Add(c.ShapeName, c);
			IShapeAreaCalculator s = new SquareAreaCalculator();
			shapeAreaCalculators.Add(s.ShapeName, s);
			IShapeAreaCalculator r = new RectangleAreaCalculator();
			shapeAreaCalculators.Add(r.ShapeName, r);
			IShapeAreaCalculator t = new TriangleAreaCalculator();
			shapeAreaCalculators.Add(t.ShapeName, t);
		}
		public static ParserOutput ProcessLines(List<string> inputLines, long startLineNo)
		{
			// each block of lines processed here by noting the starting line no according to the input file..
			// the transformed output and error logs are stored in the TransformerOutput object

			ParserOutput result = new ParserOutput()
			{
				StartLineNo = startLineNo,
				OutputLines = new List<AreaResult>(),
				Errors = new List<ParserErrorLog>()
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
				AreaResult areaOutput = null;
				ParserErrorLog outputErrorLog = null;
				long currentLineNum = startLineNo + (long)index;

				try
				{
					areaOutput = ProcessLine(inputLine);
				}
				catch(Exception e)
				{
					outputErrorLog = outputErrorLog ?? new ParserErrorLog(currentLineNum, inputLine, e.Message);
				}

				System.Diagnostics.Debug.Assert(!(areaOutput == null && outputErrorLog == null));
				if (areaOutput != null)
				{
					result.OutputLines.Add(areaOutput);
				}
				else
				{
					result.Errors.Add(outputErrorLog);
				}
			} 
			// );

			// below block needed if parallel.for is used..
			// return the sorted list of output and errors by the line no.
			//result.OutputLines = loopOutputLines.Values.ToList();
			//result.Errors = result.Errors.OrderBy(e => e.LineNumber).ToList();

			return result;
		}

		private static AreaResult ProcessLine(string currentLine)
		{
			const int numDecimalDigits = 5;

			if (string.IsNullOrWhiteSpace(currentLine))
			{
				throw new ApplicationException("Input line is empty.");
			}

			string[] lineTokens = currentLine.Trim().Split(spaceDelim);
			if (lineTokens == null || lineTokens.Length < 2 || lineTokens.Length > 3)
			{
				throw new ApplicationException("Input line with invalid number of arguments.");
			}

			string inputShapeName = lineTokens[0];
			if (!shapeAreaCalculators.ContainsKey(inputShapeName))
			{
				throw new ApplicationException($"Invalid input shape name provided: [{inputShapeName}]");
			}

			double param1 = double.Parse(lineTokens[1]);
			double? param2 = null;
			if (lineTokens.Length > 2)
			{
				param2 = double.Parse(lineTokens[2]);
			}

			if (Math.Round(param1, numDecimalDigits) <= 0.0)
			{
				throw new ArgumentException($"Invalid 1st decimal parameter for shape: [{param1}]");
			}
			if (param2.HasValue && Math.Round(param2.Value, numDecimalDigits) <= 0.0)
			{
				throw new ArgumentException($"Invalid 2nd decimal parameter for shape: [{param2}]");
			}

			List<double> parameters = new List<double> { param1 };
			if (param2.HasValue) { parameters.Add(param2.Value); }
			AreaResult ar = new AreaResult()
			{
				ShapeName = inputShapeName,
				Area = shapeAreaCalculators[inputShapeName].ComputeArea(parameters.ToArray())
			};

			return ar;
		}
	}
}
