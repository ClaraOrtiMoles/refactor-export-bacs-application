namespace Sonovate.CodeTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Domain;

	public class AgencyPaymentService : IAgencyPaymentService
	{
		private IAgencyRepository _agencyRepository;
		private IPaymentsRepository _paymentsRepository;

		public AgencyPaymentService()
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

		public async Task<List<BacsResult>> GetAgencyBacsResult(DateTime startDate, DateTime endDate)
		{
			var payments = _paymentsRepository.GetBetweenDates(startDate, endDate);

			if (!payments.Any())
			{
				throw new InvalidOperationException($"No agency payments found between dates {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}");
			}

			var agencyIds = payments.Select(x => x.AgencyId).Distinct().ToList();
			var agencies = await _agencyRepository.GetAgencies(agencyIds);

			return BuildAgencyPayments(payments, agencies);
		}
		 
		private static List<BacsResult> BuildAgencyPayments(IEnumerable<Payment> payments, List<Agency> agencies)
		{
			return (from p in payments
				let agency = agencies.FirstOrDefault(x => x.Id == p.AgencyId)
				where agency?.BankDetails != null
				let bank = agency.BankDetails
				select new BacsResult
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