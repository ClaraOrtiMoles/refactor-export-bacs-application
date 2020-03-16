namespace Sonovate.CodeTest.Services.Writers
{
	using System.Collections.Generic;

	public interface ICsvFileWriter
	{
		public void WriteCsvFile<T>(string fileName, IEnumerable<T> records);
	}
}