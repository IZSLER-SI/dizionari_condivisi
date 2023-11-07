using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiConsultaLog.Models
{
    public class LogModel
    {
        public int Id { get; set; }
        public int Anno { get; set; }
        public string Utente { get; set; }
        public string Modulo { get; set; }
        public string Dizionario { get; set; }
        public string Data_Operazione { get; set; }

        public LogModel(int id, int anno, string utente, string modulo, string dizionario, string data_operazione)
        {
            this.Id = id;
            this.Anno = anno;
            this.Utente = utente;
            this.Modulo = modulo;
            this.Dizionario = dizionario;
            this.Data_Operazione = data_operazione;
        }
    }
}