using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Raven.Client.Documents;
using Sonovate.CodeTest.Domain;

namespace Sonovate.CodeTest
{
	using Configuration;

	public class BacsExportService
    {
        private const string NOT_AVAILABLE = "NOT AVAILABLE";

        private IAgencyPaymentService _agencyPaymentService;
        private ICsvFileWriter _csvFileWriter;

        private ISettings _settings;

        public BacsExportService()
        {
	        SetAgencyPaymentService(new AgencyPaymentService());
            SetSettings(new Settings());
            SetCsvFileWriter(new CsvFileWriter());
        }
         
        public void SetSettings(ISettings settings)
        {
	        _settings = settings;
        }

        public void SetCsvFileWriter(ICsvFileWriter csvFileWriter)
        {
	        _csvFileWriter = csvFileWriter;
        }

        public void SetAgencyPaymentService(IAgencyPaymentService agencyPaymentService)
        {
	        _agencyPaymentService = agencyPaymentService;
        }
        
        public async Task ExportZip(BacsExportType bacsExportType)
        {
            if (bacsExportType == BacsExportType.None)
            {
                throw new Exception("No export type provided.");
            }

           
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;

            try
            {
	            if (bacsExportType == BacsExportType.Agency && _settings.GetSetting("EnableAgencyPayments") == "true")
	            {
		            var payments = await _agencyPaymentService.GetAgencyBacsResult(startDate, endDate);
		            var filename = $"{bacsExportType}_BACSExport.csv";
		            _csvFileWriter.WriteCsvFile<BacsResult>(filename, payments);
                }

                switch (bacsExportType)
                {
                    case BacsExportType.Agency:
	                    break;
                    case BacsExportType.Supplier:
                        var supplierBacsExport = GetSupplierPayments(startDate, endDate);
                        SaveSupplierBacsExport(supplierBacsExport);
                        break;
                    default:
                        throw new Exception("Invalid BACS Export Type.");
                }

            }
            catch (InvalidOperationException inOpEx)
            {
                throw new Exception(inOpEx.Message);
            }
        }
        
        private SupplierBacsExport GetSupplierPayments(DateTime startDate, DateTime endDate)
        {
            var invoiceTransactions = new InvoiceTransactionRepository();
            var candidateInvoiceTransactions = invoiceTransactions.GetBetweenDates(startDate, endDate);

            if (!candidateInvoiceTransactions.Any())
            {
                throw new InvalidOperationException(string.Format("No supplier invoice transactions found between dates {0} to {1}", startDate, endDate));
            }

            var candidateBacsExport = CreateCandidateBacxExportFromSupplierPayments(candidateInvoiceTransactions);

            return candidateBacsExport;
        }
        private SupplierBacsExport CreateCandidateBacxExportFromSupplierPayments(IList<InvoiceTransaction> supplierPayments)
        {
            var candidateBacsExport = new SupplierBacsExport
            {
                SupplierPayment = new List<SupplierBacs>()
            };

            candidateBacsExport.SupplierPayment = BuildSupplierPayments(supplierPayments);
                
            return candidateBacsExport;
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
                var candidateRepository = new CandidateRepository();
                var candidate = candidateRepository.GetById(transactionGroup.Key.SupplierId);

                if (candidate == null)
                {
                    throw new InvalidOperationException(string.Format("Could not load candidate with Id {0}",
                        transactionGroup.Key.SupplierId));
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

        private void SaveSupplierBacsExport(SupplierBacsExport supplierBacsExport)
        {
            var fileName = string.Format("{0}_BACSExport.csv", BacsExportType.Supplier);

            using (var csv = new CsvWriter(new StreamWriter(new FileStream(fileName, FileMode.Create))))
            {
                csv.WriteRecords(supplierBacsExport.SupplierPayment);
            }
        }
    }
}