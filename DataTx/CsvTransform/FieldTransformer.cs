/**
 * This file contains classes to transform a input string format to another based on an interface pattern
 * The consumer needs to use the factory class to pick the required implementation class of that interface.
 * The implementation class will perform both the required input data validation and transformation and 
 * will give appropriate exceptions/errors when it is unsuccessful.
 * **/

using System;
using System.Globalization;
using System.Linq;

namespace CsvTransform
{
	public interface IFieldTransform
	{
		bool NeedsInputFields { get; }

		string TransformField(params string[] inputFields);
	}

	public static class FieldTransformFactory
	{
		public static IFieldTransform GetFieldTransformer(string name)
		{
			switch (name)
			{
				case "Number": return new NumberFieldTransformer();

				case "DateTime": return new DateTimeFieldTransformer();

				case "ProductId": return new ProductIdFieldTransformer();

				case "ProductName": return new ProductNameFieldTransformer();

				case "Quantity": return new QuantityFieldTransformer();

				case "KgUnit": return new KgUnitFieldTransformer();

				default: throw new NotImplementedException();
			}
		}
	}

	//public class ConstantFieldTransformer : IFieldTransform
	//{

	//	public string TransformField(params string[] inputFields)
	//	{
	//		return string.Join(",", inputFields);
	//	}
	//}

	public class KgUnitFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => false;

		public string TransformField(params string[] inputFields)
		{
			return string.Join(",", inputFields);
		}
	}

	public class NumberFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => true;

		public string TransformField(params string[] inputFields)
		{
			if (inputFields == null || inputFields.Length != 1 || string.IsNullOrEmpty(inputFields[0]))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			var result = inputFields[0].Trim();
			if (string.IsNullOrEmpty(result) || !Char.IsDigit(result[0]))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			return ulong.TryParse(inputFields[0], out _) ? result : throw new ArgumentException(nameof(inputFields));
		}
	}

	public class DateTimeFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => true;

		public string TransformField(params string[] inputFields)
		{
			if (inputFields == null || inputFields.Length != 3)
			{
				throw new ArgumentException(nameof(inputFields));
			}

			try
			{
				var result = new DateTime(int.Parse(inputFields[0]), int.Parse(inputFields[1]), int.Parse(inputFields[2]));
				return result.ToString("d");
			}
			catch (Exception e)
			{
				throw new ArgumentException($"Invalid input date field - {e} ", nameof(inputFields));
			}
		}
	}

	public class ProductIdFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => true;

		public string TransformField(params string[] inputFields)
		{
			if (inputFields == null || inputFields.Length != 1 || string.IsNullOrEmpty(inputFields[0]))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			var result = inputFields[0].Trim();
			if (string.IsNullOrEmpty(result) || result.All(c => Char.IsLetterOrDigit(c)))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			return result;
		}
	}

	public class ProductNameFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => true;

		public string TransformField(params string[] inputFields)
		{
			if (inputFields == null || inputFields.Length != 1 || string.IsNullOrEmpty(inputFields[0]))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			var result = inputFields[0].Trim();
			result = new CultureInfo("en-US", false).TextInfo.ToTitleCase(result);

			return result;
		}
	}

	public class QuantityFieldTransformer : IFieldTransform
	{
		public bool NeedsInputFields => true;

		public string TransformField(params string[] inputFields)
		{
			if (inputFields == null || inputFields.Length != 1 || string.IsNullOrEmpty(inputFields[0]))
			{
				throw new ArgumentException(nameof(inputFields));
			}

			try
			{
				var result = decimal.Parse(inputFields[0]);
				return string.Format("{0:.##}", result);
			}
			catch (Exception e)
			{
				throw new ArgumentException($"Invalid input quantity field - {e} ", nameof(inputFields));
			}
		}
	}
}
