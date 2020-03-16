namespace Sonovate.CodeTest
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Domain;

	public interface IAgencyPaymentService
	{
		Task<List<BacsResult>> GetAgencyBacsResult(DateTime startDate, DateTime endDate);
	}
}