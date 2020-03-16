namespace Sonovate.Codetest.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutoFixture;
	using CodeTest;
	using CodeTest.Domain;
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
		private Mock<IInvoiceTransactionRepository> _transactionRespositoryMock;
		private Mock<ICandidateRepository> _candidateRespositoryMock;

		[SetUp]
		public void Setup()
		{
			_supplierBacsService = new SupplierBacsService();
			_startDate = _fixture.Create<DateTime>();
			_endDate = _fixture.Create<DateTime>();

			_transactionRespositoryMock = new Mock<IInvoiceTransactionRepository>();
			_candidateRespositoryMock = new Mock<ICandidateRepository>();

			_supplierBacsService.SetInvoiceTransactionRepository(_transactionRespositoryMock.Object);
			_supplierBacsService.SetCandidateRepository(_candidateRespositoryMock.Object);
		}

		[Test]
		public void ThrowInvalidOperationException_WhenGettingSupplierPayments_GivenNoInvoiceTransactionExistBetweenDates()
		{
			_transactionRespositoryMock.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
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
			_candidateRespositoryMock.Setup(x => x.GetById(supplierId)).Returns(default(Candidate));
			_transactionRespositoryMock.Setup(x => x.GetBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Returns(invoiceTransactions);
			
			Action action = () => _supplierBacsService.GetSupplierPayments(_startDate, _endDate);
			action.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($"Could not load candidate with Id {supplierId}");
		}
	}
}
