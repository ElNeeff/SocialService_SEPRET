using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class PaymentVM
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Account { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRIBUTES
        public string PriceFormatted { get; set; }
        public string TimeCreatedFormatted { get; set; }
    }
}