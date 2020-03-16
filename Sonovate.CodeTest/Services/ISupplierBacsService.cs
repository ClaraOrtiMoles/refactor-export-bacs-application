namespace Sonovate.CodeTest.Services
{
	using System;
	using System.Collections.Generic;
	using Domain;

	internal interface ISupplierBacsService
	{
		List<SupplierBacs> GetSupplierPayments(DateTime startDate, DateTime endDate);
	}
}