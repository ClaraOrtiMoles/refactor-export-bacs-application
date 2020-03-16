using System;
using System.Collections.Generic;
using System.Text;

namespace Sonovate.Codetest.UnitTests
{
	using System.Linq;
	using System.Threading.Tasks;
	using AutoFixture;
	using CodeTest;
	using CodeTest.Domain;
	using FluentAssertions;
	using Moq;
	using NUnit.Framework;

	public class AgencyPaymentServiceShould
	{
		private readonly Fixture _fixture = new Fixture();
		private AgencyPaymentService _agencyPaymentService;
		private Mock<IAgencyRepository> _agencyRepositoryMock;
		private Mock<IPaymentsRepository> _paymentsRepositoryMock;
		private DateTime _startDate;
		private DateTime _endDate;

		[SetUp]
		public void Setup()
		{
			_agencyPaymentService = new AgencyPaymentService();
			_agencyRepositoryMock = new Mock<IAgencyRepository>();
			_paymentsRepositoryMock = new Mock<IPaymentsRepository>();
			_agencyPaymentService.SetAgencyRepository(_agencyRepositoryMock.Object);
			_agencyPaymentService.SetPaymentsRepository(_paymentsRepositoryMock.Object);
			_startDate = _fixture.Create<DateTime>();
			_endDate = _fixture.Create<DateTime>();
		}

		[Test]
		public void ThrowInvalidOperationException_WhenGettingBacsResult_GivenNoPaymentsExistBetweenDates()
		{
			_paymentsRepositoryMock
				.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(new List<Payment>());


			Func<Task<List<BacsResult>>> action = () =>
				_agencyPaymentService.GetAgencyBacsResult(_startDate, _endDate);

			action.Should()
				.ThrowAsync<InvalidOperationException>()
				.WithMessage($"No agency payments found between dates {_startDate:dd/MM/yyyy} to {_endDate:dd/MM/yyyy}");
		}

		[Test]
		public void GetDistinctAgenciesAssociatedToPayments_WhenGettingBacsResult()
		{
			var listPayments = _fixture.Create<List<Payment>>();
			listPayments.Add(listPayments.First());
			_paymentsRepositoryMock
				.Setup(x => x.GetBetweenDates(_startDate, _endDate))
				.Returns(listPayments);
			_agencyRepositoryMock
				.Setup(x => x.GetAgencies(It.IsAny<List<string>>()))
				.ReturnsAsync(new List<Agency>());
			var agencyIds = listPayments.Select(x => x.AgencyId).Distinct().ToList();


			var result = _agencyPaymentService.GetAgencyBacsResult(_startDate, _endDate);

			_agencyRepositoryMock.Verify(x => x.GetAgencies(agencyIds), Times.Once);
		}

		[Test]
		public async Task ReturnBacsResults()
		{
			var listPayments = _fixture.Create<List<Payment>>();
			var listAgencies = new List<Agency>();
			listPayments
				.ForEach(payment =>
					listAgencies
						.Add(_fixture.Build<Agency>()
						.With(agency => agency.Id, payment.AgencyId)
						.Create()));

			_paymentsRepositoryMock
				.Setup(x => x.GetBetweenDates(_startDate, _endDate))
				.Returns(listPayments);
			_agencyRepositoryMock
				.Setup(x => x.GetAgencies(It.IsAny<List<string>>()))
				.ReturnsAsync(listAgencies);

			var result = await _agencyPaymentService.GetAgencyBacsResult(_startDate, _endDate);

			foreach (var agency in listAgencies)
			{
				var payment = listPayments.First(x => x.AgencyId == agency.Id);
				var bacsResult = new BacsResult()
				{
					AccountName = agency.BankDetails.AccountName,
					AccountNumber = agency.BankDetails.AccountNumber,
					SortCode = agency.BankDetails.SortCode,
					Amount = payment.Balance,
					Ref = $"SONOVATE{payment.PaymentDate:ddMMyyyy}"
				};
				result.Should().ContainEquivalentOf(bacsResult);
			}
			result.Count().Should().Be(listAgencies.Count);
		}

		[Test]
		public async Task ReturnBacsResults_GivenAgenciesContainsBankDetails()
		{
			var agencyWithBankDetailsEmpty =
				_fixture.Build<Agency>()
					.With(x => x.Id, "ID")
					.With(x => x.BankDetails, default(BankDetails))
					.Create();
			var paymentAssociatedToAgencyWithBankDetailsEmpty = _fixture.Build<Payment>()
				.With(x => x.AgencyId, agencyWithBankDetailsEmpty.Id)
				.Create();

			var listPayments = _fixture.Create<List<Payment>>();
			var listAgencies = new List<Agency>();

			listPayments
				.ForEach(payment =>
					listAgencies
						.Add(_fixture.Build<Agency>()
							.With(agency => agency.Id, payment.AgencyId)
							.Create()));

			listAgencies.Add(agencyWithBankDetailsEmpty);
			listPayments.Add(paymentAssociatedToAgencyWithBankDetailsEmpty);
			
			_paymentsRepositoryMock
				.Setup(x => x.GetBetweenDates(_startDate, _endDate))
				.Returns(listPayments);
			_agencyRepositoryMock
				.Setup(x => x.GetAgencies(It.IsAny<List<string>>()))
				.ReturnsAsync(listAgencies);

			var result = await _agencyPaymentService.GetAgencyBacsResult(_startDate, _endDate);

			result.Count().Should().Be(listPayments.Count - 1);
		}
	}
}
