namespace Sonovate.CodeTest.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Domain;

	public interface IAgencyBacsService
	{
		Task<List<AgencyBacs>> GetAgencyBacsResult(DateTime startDate, DateTime endDate);
	}
}