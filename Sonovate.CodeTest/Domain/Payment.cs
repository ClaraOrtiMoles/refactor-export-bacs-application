using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Sonovate.Codetest.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Sonovate.CodeTest.Domain
{
    internal class Payment
    {
        public string AgencyId { get; set; }
        public decimal Balance { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}