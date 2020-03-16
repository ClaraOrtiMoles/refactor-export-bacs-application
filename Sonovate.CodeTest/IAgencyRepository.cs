namespace Sonovate.CodeTest
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Domain;

	public interface IAgencyRepository
	{
		Task<List<Agency>> GetAgencies(List<string> agenciesIds);
	}
}
