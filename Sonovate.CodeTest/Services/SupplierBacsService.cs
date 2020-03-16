namespace Sonovate.CodeTest.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Domain;

	internal class SupplierBacsService : ISupplierBacsService
	{
		private const string NotAvailable = "NOT AVAILABLE";
		private IInvoiceTransactionRepository _invoiceTransactionRepository;
		private ICandidateRepository _candidateRepository;

        public SupplierBacsService()
		{
			SetInvoiceTransactionRepository(new InvoiceTransactionRepository());
			SetCandidateRepository(new CandidateRepository());
		}

        public void SetCandidateRepository(ICandidateRepository candidateRepository)
        {
	        _candidateRepository = candidateRepository;
        }

        public void SetInvoiceTransactionRepository(IInvoiceTransactionRepository invoiceTransactionRepository)
		{
			_invoiceTransactionRepository = invoiceTransactionRepository;
		}

		public List<SupplierBacs> GetSupplierPayments(DateTime startDate, DateTime endDate)
        { 
            var candidateInvoiceTransactions = _invoiceTransactionRepository.GetBetweenDates(startDate, endDate);

            if (!candidateInvoiceTransactions.Any())
            {
                throw new InvalidOperationException($"No supplier invoice transactions found between dates {startDate} to {endDate}");
            }

            return BuildSupplierPayments(candidateInvoiceTransactions);
        }
      
        private List<SupplierBacs> BuildSupplierPayments(IEnumerable<InvoiceTransaction> invoiceTransactions)
        {
			var transactionsByCandidateAndInvoiceId = invoiceTransactions
				.GroupBy(transaction => new
				{
					transaction.InvoiceId,
					Candidate = _candidateRepository.GetById(transaction.SupplierId) ?? throw new InvalidOperationException($"Could not load candidate with Id {transaction.SupplierId}"),
				});

			return (from transactionGroup in transactionsByCandidateAndInvoiceId
				let bank = transactionGroup.Key.Candidate.BankDetails
				let firstTransaction = transactionGroup.First()
				select new SupplierBacs()
				{
					AccountName = bank.AccountName,
					AccountNumber = bank.AccountNumber,
					SortCode = bank.SortCode,
					PaymentAmount = transactionGroup.Sum(invoiceTransaction => invoiceTransaction.Gross),
					InvoiceReference =  string.IsNullOrWhiteSpace(firstTransaction.InvoiceRef) ? NotAvailable: firstTransaction.InvoiceRef,
					PaymentReference = $"SONOVATE{firstTransaction.InvoiceDate.GetValueOrDefault():ddMMyyyy}"
				}).ToList();
        }
       
    }
}
