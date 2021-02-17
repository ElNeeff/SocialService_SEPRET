using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class VoucherVM
    {
        public List<PaymentVM> Payments { get; set; }
        public List<PersonVM> People { get; set; }

        public string Reference { get; set; }
    }
}