using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sonovate.Codetest.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Sonovate.CodeTest.Domain
{
	using System.Runtime.CompilerServices;

	
    internal class SupplierBacs
    {
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string SortCode { get; set; }
        public decimal PaymentAmount { get; set; }
        public string InvoiceReference { get; set; }
        public string PaymentReference { get; set; }
    }
}