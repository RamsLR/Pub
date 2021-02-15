using CsvTransform;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace DataTx
{
	public static class CsvFieldConfigReader
	{
		public static List<CsvDataFieldTransforms> ReadCsvDataFieldTransformConfigs()
		{
			const string fieldConfigKeyPrefix = "OutputField";
			var configuredTransforms = new List<CsvDataFieldTransforms>();
			List<string> outputFieldNames = new List<string>();
			int index = 0;

			string fieldConfigValue;
			do
			{
				fieldConfigValue = ConfigurationManager.AppSettings[$"{fieldConfigKeyPrefix}{index}"];
				if (string.IsNullOrEmpty(fieldConfigValue)) break;

				var configAttributes = fieldConfigValue.Split(":");
				if (configAttributes.Length < 3)
				{
					throw new ApplicationException("Invalid field config attribute : {fieldConfigValue}");
				}

				var positionStrings = configAttributes[0].Split(",");
				List<int> inputFieldPositions = new List<int>();
				foreach (string fieldPos in positionStrings)
				{
					inputFieldPositions.Add(int.Parse(fieldPos));
				}

				outputFieldNames.Add(configAttributes[1]); // output field name

				var fieldTransformer = FieldTransformFactory.GetFieldTransformer(configAttributes[2]);
				var csvDataFieldTransform = new CsvDataFieldTransforms(inputFieldPositions.ToArray(), fieldTransformer);
				configuredTransforms.Add(csvDataFieldTransform);
			}
			while (fieldConfigValue.Length > 0);

			return configuredTransforms;
		}
	}
}
