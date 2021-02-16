using NUnit.Framework;
using System.IO;
using CsvTransform;

namespace DataTxTest
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void FullScenarioOfDataTx_Test_Success()
		{
			var configuredTransforms = DataTx.CsvFieldConfigReader.ReadCsvDataFieldTransformConfigs();

			var outFile = @"..\..\..\ExpectedDataTxOutputFile.txt";
			FileTxManager.TransformCsvDataFileAsync(@"..\..\..\InputCsvFile.txt.csv", outFile, 
					configuredTransforms.FieldTransforms.ToArray(),
					configuredTransforms.OutputFieldNames.ToArray());

			string actual = File.ReadAllText(outFile);
			string expected = File.ReadAllText(@"..\..\..\ExpectedDataTxOutputFile.txt");

			Assert.True(string.CompareOrdinal(actual, expected) == 0);
		}
	}
}