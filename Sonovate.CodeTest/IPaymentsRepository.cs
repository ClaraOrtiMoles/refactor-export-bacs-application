using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sonovate.Codetest.UnitTests")]
namespace Sonovate.CodeTest
{
	using System;
	using System.Collections.Generic;
	using Domain;

	internal interface IPaymentsRepository
	{
		IList<Payment> GetBetweenDates(DateTime start, DateTime end);
	}
}