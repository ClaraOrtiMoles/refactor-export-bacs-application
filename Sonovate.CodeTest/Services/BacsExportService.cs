namespace Sonovate.CodeTest
{
	using System;
	using System.Threading.Tasks;
	using Configuration;
	using Domain;
	using Services;
	using Services.Writers;

	internal class BacsExportService
    {
        private IAgencyBacsService _agencyBacsService;
        private ICsvFileWriter _csvFileWriter;
        private ISupplierBacsService _supplierBacsService;
        private ISettings _settings;

        public BacsExportService()
        {
            SetSupplierBacsService(new SupplierBacsService());
	        SetAgencyBacsService(new AgencyBacsService());
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

        public void SetAgencyBacsService(IAgencyBacsService agencyBacsService)
        {
	        _agencyBacsService = agencyBacsService;
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
            if (!IsAgencyPaymentsEnabled())
		        return;
             
            var payments = await _agencyBacsService.GetAgencyBacsResult(startDate, endDate);
	        _csvFileWriter.WriteCsvFile(GetFileName(BacsExportType.Agency), payments);
        }

        private bool IsAgencyPaymentsEnabled()
        {
	        return bool.TryParse(_settings.GetSetting("EnableAgencyPayments"), out var enabledAgencyPayments) &&
	               enabledAgencyPayments;
        }

        private static string GetFileName(BacsExportType type)
        {
	        return $"{type}_BACSExport.csv";
        }

    }
}