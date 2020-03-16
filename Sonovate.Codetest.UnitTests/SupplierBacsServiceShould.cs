namespace Sonovate.Codetest.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutoFixture;
	using CodeTest;
	using CodeTest.Domain;
	using CodeTest.Services;
	using FluentAssertions;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class SupplierBacsServiceShould
	{
		private readonly Fixture _fixture = new Fixture();
		private SupplierBacsService _supplierBacsService;
		private DateTime _startDate;
		private DateTime _endDate;
		private Mock<IInvoiceTransactionRepository> _transactionRepositoryMock;
		private Mock<ICandidateRepository> _candidateRepositoryMock;

		[SetUp]
		public void Setup()
		{
			_supplierBacsService = new SupplierBacsService();
			_startDate = _fixture.Create<DateTime>();
			_endDate = _fixture.Create<DateTime>();

			_transactionRepositoryMock = new Mock<IInvoiceTransactionRepository>();
			_candidateRepositoryMock = new Mock<ICandidateRepository>();

			_supplierBacsService.SetInvoiceTransactionRepository(_transactionRepositoryMock.Object);
			_supplierBacsService.SetCandidateRepository(_candidateRepositoryMock.Object);
		}

		[Test]
		public void ThrowInvalidOperationException_WhenGettingSupplierPayments_GivenNoInvoiceTransactionExistBetweenDates()
		{
			_transactionRepositoryMock.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(new List<InvoiceTransaction>());

			Action action = () => _supplierBacsService.GetSupplierPayments(_startDate, _endDate);
			action.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"No supplier invoice transactions found between dates {_startDate} to {_endDate}");
		}


		[Test]
		public void ThrowInvalidOperationException_WhenGettingSupplierPayments_GivenCandidateIdCanNotBeLoaded()
		{
			var invoiceTransactions = _fixture.Create<List<InvoiceTransaction>>();
			var supplierId = invoiceTransactions.First().SupplierId;
			_candidateRepositoryMock.Setup(x => x.GetById(supplierId)).Returns(default(Candidate));
			_transactionRepositoryMock.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(invoiceTransactions);
			
			Action action = () => _supplierBacsService.GetSupplierPayments(_startDate, _endDate);
			action.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"Could not load candidate with Id {supplierId}");
		}


		[Test]
		public void GetSupplierBacsPayments()
		{
			var candidateInvoiceTransaction = new List<InvoiceTransaction>
			{
				GetInvoiceTransaction("ID1", "Supplier1", "Ref1", 10),
				GetInvoiceTransaction("ID1", "Supplier1", "Ref2", 20),
				GetInvoiceTransaction("ID2", "Supplier2", "Ref1", 10),
				GetInvoiceTransaction("ID3", "Supplier2", "", 10)
			};

			_transactionRepositoryMock
				.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(candidateInvoiceTransaction);

			var candidate1 = _fixture.Build<Candidate>().Create();
			var candidate2 = _fixture.Build<Candidate>().Create();
			_candidateRepositoryMock.Setup(x => x.GetById("Supplier1")).Returns(candidate1);
			_candidateRepositoryMock.Setup(x => x.GetById("Supplier2")).Returns(candidate2);

			var result = _supplierBacsService.GetSupplierPayments(_startDate, _endDate);

			var expectedResult = new List<SupplierBacs>
			{
				GetSupplierBacs(candidate1.BankDetails, 30, "Ref1"),
				GetSupplierBacs(candidate2.BankDetails, 10, "Ref1"),
				GetSupplierBacs(candidate2.BankDetails, 10, "NOT AVAILABLE")
			};

			result.Should().BeEquivalentTo(expectedResult);
		}

		private SupplierBacs GetSupplierBacs(BankDetails candidateBankDetails, int sumGross, string reference)
		{
			return new SupplierBacs()
			{
				AccountName = candidateBankDetails.AccountName,
				AccountNumber = candidateBankDetails.AccountNumber,
				PaymentAmount = sumGross,
				InvoiceReference = reference,
				SortCode = candidateBankDetails.SortCode,
				PaymentReference = $"SONOVATE{DateTime.Today:ddMMyyyy}"
			};
		}

		private static InvoiceTransaction GetInvoiceTransaction(string invoiceId, string supplierId, string invoiceRef, decimal gross)
		{
			return new InvoiceTransaction()
			{
				Gross = gross,
				InvoiceRef = invoiceRef,
				SupplierId = supplierId,
				InvoiceId = invoiceId,
				InvoiceDate = DateTime.Now
			};
		}
	}
}
