namespace Sonovate.CodeTest
{
	using System.IO;
	using System.Collections.Generic;
	using CsvHelper;

	public class CsvFileWriter : ICsvFileWriter
	{
		public void WriteCsvFile<T>(string fileName, IEnumerable<T> records)
		{
			using var csv = new CsvWriter(new StreamWriter(new FileStream(fileName, FileMode.Create)));
			csv.WriteRecords(records);
		}
	}
}
