using System.Runtime.CompilerServices;

namespace Sonovate.CodeTest
{
	using System;
	using System.Collections.Generic;
	using Domain;

	internal interface IInvoiceTransactionRepository
	{
		List<InvoiceTransaction> GetBetweenDates(DateTime startDate, DateTime endDate);
	}
}