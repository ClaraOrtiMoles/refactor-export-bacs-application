namespace Sonovate.CodeTest
{
    using System;
    using System.Threading.Tasks;
    using Configuration;
    using Domain;

    internal class BacsExportService
    {
        private IAgencyPaymentService _agencyPaymentService;
        private ICsvFileWriter _csvFileWriter;
        private ISupplierBacsService _supplierBacsService;
        private ISettings _settings;

        public BacsExportService()
        {
            SetSupplierBacsService(new SupplierBacsService());
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

        public void SetSupplierBacsService(ISupplierBacsService supplierBacsService)
        {
	        _supplierBacsService = supplierBacsService;
        }

        public async Task ExportZip(BacsExportType bacsExportType)
        {
            if (NoExportTypeProvided(bacsExportType))
            {
                throw new Exception("No export type provided.");
            }
             
            var startDate = DateTime.Now.AddMonths(-1);
            var endDate = DateTime.Now;
            
            try
            {
                switch (bacsExportType)
                {
                    case BacsExportType.Agency:
	                    await ExportAgencyBacs(startDate, endDate);
	                    break;
                    case BacsExportType.Supplier:
                        ExportSupplierBacs(startDate, endDate);
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

        private static bool NoExportTypeProvided(BacsExportType bacsExportType)
        {
	        return bacsExportType == BacsExportType.None;
        }

        private void ExportSupplierBacs(DateTime startDate, DateTime endDate)
        {
            var paymentsSuppliers = _supplierBacsService.GetSupplierPayments(startDate, endDate);
	        _csvFileWriter.WriteCsvFile(GetFileName(BacsExportType.Supplier), paymentsSuppliers);
        }
       
        private async Task ExportAgencyBacs(DateTime startDate, DateTime endDate)
        {
            if (_settings.GetSetting("EnableAgencyPayments") != "true")
		        return;
             
            var payments = await _agencyPaymentService.GetAgencyBacsResult(startDate, endDate);
	        _csvFileWriter.WriteCsvFile(GetFileName(BacsExportType.Agency), payments);
        }

        private static string GetFileName(BacsExportType type)
        {
	        return $"{type}_BACSExport.csv";
        }

    }
}