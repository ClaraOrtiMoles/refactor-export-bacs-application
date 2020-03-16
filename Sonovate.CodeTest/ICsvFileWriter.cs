namespace Sonovate.CodeTest
{
	using System.Collections.Generic;

	public interface ICsvFileWriter
	{
		public void WriteCsvFile<T>(string fileName, IEnumerable<T> records);
	}
}