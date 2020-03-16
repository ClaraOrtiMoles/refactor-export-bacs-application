namespace Sonovate.CodeTest
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Domain;
	using Raven.Client.Documents;

	public class AgencyRepository : IAgencyRepository
	{
		private readonly IDocumentStore _documentStore;
		public AgencyRepository()
		{
			_documentStore = new DocumentStore { Urls = new[] { "http://localhost" }, Database = "Export" };
			_documentStore.Initialize();
		}

		public async Task<List<Agency>> GetAgencies(List<string> agenciesIds)
		{
			using var session = _documentStore.OpenAsyncSession();
			return (await session.LoadAsync<Agency>(agenciesIds)).Values.ToList();
		}
	}
}
