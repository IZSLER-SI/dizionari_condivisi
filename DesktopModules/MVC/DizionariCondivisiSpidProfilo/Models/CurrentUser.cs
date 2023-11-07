using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidProfilo.Models
{
    public class CurrentUser
    {
        public bool IsSuperUser { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSpid { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Nickname { get; set; }
        public string CodiceFiscale { get; set; }
        public string EmailLavoro { get; set; }
        public string EnteAppartenenza { get; set; }
        public string IndirizzoLavoro { get; set; }
        public string TelefonoLavoro { get; set; }
    }
}