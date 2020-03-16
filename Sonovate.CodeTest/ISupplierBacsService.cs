namespace Sonovate.CodeTest
{
	using System;
	using Domain;

	internal interface ISupplierBacsService
	{
		SupplierBacsExport GetSupplierPayments(DateTime startDate, DateTime endDate);
	}
}