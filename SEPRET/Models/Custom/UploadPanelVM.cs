using System.Collections.Generic;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class UploadPanelVM
    {
        public HttpPostedFileBase File { get; set; }
        public List<MyFileVM> MyFileVMs { get; set; }
    }
}