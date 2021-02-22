using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class PaymentDBVM
    {
        public long Rejected { get; set; }
        public long Pending { get; set; }
        public long Accepted { get; set; }
        public long Invoiced { get; set; }
        public long Total { get; set; }
        public decimal Progress { get; set; }
        public PaymentDBGeneral RejectedGeneral { get; set; }
        public PaymentDBGeneral PendingGeneral { get; set; }
        public PaymentDBGeneral AcceptedGeneral { get; set; }
        public PaymentDBGeneral InvoicedGeneral { get; set; }
    }
}