using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class ReceiptVM
    {

        public long Id { get; set; }
        public long PersonId { get; set; }
        public long PaymentId { get; set; }
        public long MethodId { get; set; }
        public long PhaseId { get; set; }
        public string Voucher { get; set; }
        public string Image { get; set; }
        public string Invoice { get; set; }
        public Nullable<long> InvoicedBy { get; set; }
        public bool Active { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeUpdated { get; set; }

        // CUSTOM ATTRBUTES

        #region Bitacora
        public string PersonName { get; set; }
        public string Enrollment { get; set; }
        public string Career { get; set; }
        public string Reference { get; set; }
        public string Email { get; set; }
        public string PaymentName { get; set; }
        public string MethodName { get; set; }
        public string PhaseName { get; set; }
        public string PriceFormatted { get; set; }
        public string RejectDescription{ get; set; }
        #endregion

        public string RelativePath { get; set; }
        public HttpPostedFileBase File { get; set; }
        public string TimeCreatedFormatted { get; set; }
        public long TotalRecords { get; set; }
    }
}