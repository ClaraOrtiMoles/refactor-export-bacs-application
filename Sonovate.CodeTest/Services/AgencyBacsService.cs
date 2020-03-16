namespace Sonovate.CodeTest.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Domain;

	public class AgencyBacsService : IAgencyBacsService
	{
		private IAgencyRepository _agencyRepository;
		private IPaymentsRepository _paymentsRepository;

		public AgencyBacsService()
		{
			SetAgencyRepository(new AgencyRepository());
			SetPaymentsRepository(new PaymentsRepository());
		}

		public void SetAgencyRepository(IAgencyRepository agencyRepository)
		{
			_agencyRepository = agencyRepository;
		}

		internal void SetPaymentsRepository(IPaymentsRepository paymentsRepository)
		{
			_paymentsRepository = paymentsRepository;
		}

		public async Task<List<AgencyBacs>> GetAgencyBacsResult(DateTime startDate, DateTime endDate)
		{
			var payments = _paymentsRepository.GetBetweenDates(startDate, endDate);

			if (!payments.Any())
			{
				throw new InvalidOperationException($"No agency payments found between dates {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}");
			}

			var agencyIds = payments.Select(x => x.AgencyId).Distinct().ToList();
			var agencies = await _agencyRepository.GetAgencies(agencyIds);

			return BuildAgencyBacs(payments, agencies);
		}
		 
		private static List<AgencyBacs> BuildAgencyBacs(IEnumerable<Payment> payments, List<Agency> agencies)
		{
			return (from p in payments
				let agency = agencies.FirstOrDefault(x => x.Id == p.AgencyId)
				where agency?.BankDetails != null
				let bank = agency.BankDetails
				select new AgencyBacs
				{
					AccountName = bank.AccountName,
					AccountNumber = bank.AccountNumber,
					SortCode = bank.SortCode,
					Amount = p.Balance,
					Ref = $"SONOVATE{p.PaymentDate:ddMMyyyy}"
				}).ToList();
		}
	}
}