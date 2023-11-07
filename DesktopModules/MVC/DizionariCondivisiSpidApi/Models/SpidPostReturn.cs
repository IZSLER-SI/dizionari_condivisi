using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Models
{
    public class SpidPostReturn
    {
        public string FiscalNumber { get; set; }
        public char Created { get; set; }
        public bool ExistAAD { get; set; }
    }
}