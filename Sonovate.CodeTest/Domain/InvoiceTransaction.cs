using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sonovate.Codetest.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Sonovate.CodeTest.Domain
{
    internal class InvoiceTransaction
    {
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceId { get; set; }
        public string SupplierId { get; set; }
        public decimal Gross { get; set; }
        public string InvoiceRef { get; set; }
    }
}