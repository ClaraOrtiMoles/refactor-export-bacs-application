namespace Sonovate.CodeTest
{
	using System.Linq;
	using Domain;
	using System;
	using System.Collections.Generic;

    internal class SupplierBacsService : ISupplierBacsService
	{
		private const string NOT_AVAILABLE = "NOT AVAILABLE";
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
            var results = new List<SupplierBacs>();

            var transactionsByCandidateAndInvoiceId = invoiceTransactions.GroupBy(transaction => new
            {
                transaction.InvoiceId,
                transaction.SupplierId
            });

            foreach (var transactionGroup in transactionsByCandidateAndInvoiceId)
            {
                var candidate = _candidateRepository.GetById(transactionGroup.Key.SupplierId);

                if (candidate == null)
                {
                    throw new InvalidOperationException($"Could not load candidate with Id {transactionGroup.Key.SupplierId}");
                }

                var result = new SupplierBacs();

                var bank = candidate.BankDetails;

                result.AccountName = bank.AccountName;
                result.AccountNumber = bank.AccountNumber;
                result.SortCode = bank.SortCode;
                result.PaymentAmount = transactionGroup.Sum(invoiceTransaction => invoiceTransaction.Gross);
                result.InvoiceReference = string.IsNullOrEmpty(transactionGroup.First().InvoiceRef)
                    ? NOT_AVAILABLE
                    : transactionGroup.First().InvoiceRef;
                result.PaymentReference = string.Format("SONOVATE{0}",
                    transactionGroup.First().InvoiceDate.GetValueOrDefault().ToString("ddMMyyyy"));

                results.Add(result);
            }

            return results;
        }

       
    }
}
