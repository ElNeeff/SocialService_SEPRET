using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class ReceiptCounter
    {
        public long Rejected { get; set; }
        public long Pending { get; set; }
        public long Accepted { get; set; }
        public long Invoiced { get; set; }
        public long Deleted { get; set; }
        public long Total { get; set; }
    }
}