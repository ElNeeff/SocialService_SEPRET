using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEPRET.Models.Custom
{
    public class PDFGeneratorVM
    {
        public string Asesor { get; set; }
        public List<PersonVM> Residentes { get; set; }
        public ProjectVM Proyect { get; set; }
        public string Periodo { get; set; }
        public CompanyVM Empresa { get; set; }
        public PersonVM JefedeDepartamento { get; set; }
        public string Fecha { get; set; }
    }
}