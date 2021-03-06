/**
 * This file contains classes to calculate area based on a shapes interface pattern
 * Based on the list of IShapeAreaCalculator implemented here - area computations 
 * can be invoked, computed and the result is sent back to the caller.
 * **/

using System;
using System.Linq;

namespace Shapes.AreaCalculators
{
	public interface IShapeAreaCalculator
	{
		string ShapeName { get; }

		double ComputeArea(params double[] inputFields);
	}

	//public static class ShapeAreaCalculatorFactory
	//{
	//	public static IShapeAreaCalculator ShapeAreaCalculator(string name)
	//	{
	//		switch (name)
	//		{
	//			case "circle": return new CircleAreaCalculator();
	//			case "square": return new SquareAreaCalculator();
	//			case "rectangle": return new RectangleAreaCalculator();
	//			case "triangle": return new TriangleAreaCalculator();

	//			default: throw new NotImplementedException();
	//		}
	//	}
	//}

	public class CircleAreaCalculator : IShapeAreaCalculator
	{
		public string ShapeName => "circle";

		public double ComputeArea(params double[] inputParams)
		{
			if (inputParams.Length != 1)
			{
				throw new ArgumentException("invalid number of arguments provided for circle area calculation.");
			}

			double radius = inputParams[0] / 2.0;
			return System.Math.PI * radius * radius;
		}
	}


	public class SquareAreaCalculator : IShapeAreaCalculator
	{
		public string ShapeName => "square";

		public double ComputeArea(params double[] inputParams)
		{
			if (inputParams.Length != 1)
			{
				throw new ArgumentException("invalid number of arguments provided for square area calculation.");
			}

			return inputParams[0] * inputParams[0];
		}
	}

	public class RectangleAreaCalculator : IShapeAreaCalculator
	{
		public string ShapeName => "rectangle";

		public double ComputeArea(params double[] inputParams)
		{
			if (inputParams.Length != 2)
			{
				throw new ArgumentException("invalid number of arguments provided for rectangle area calculation.");
			}

			return inputParams[0] * inputParams[1];
		}
	}

	public class TriangleAreaCalculator : IShapeAreaCalculator
	{
		public string ShapeName => "triangle";

		public double ComputeArea(params double[] inputParams)
		{
			if (inputParams.Length != 2)
			{
				throw new ArgumentException("invalid number of arguments provided for triangle area calculation.");
			}

			return inputParams[0] * inputParams[1] * 0.5;
		}
	}

}
