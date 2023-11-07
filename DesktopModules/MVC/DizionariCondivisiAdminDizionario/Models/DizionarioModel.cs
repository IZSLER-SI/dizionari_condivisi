using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Models
{
    public class DizionarioModel
    {
        public DizionarioModel(string nomeDizionario, string descrizione, string chiave, int numeroRecord)
        {
            NomeDizionario = nomeDizionario;
            Descrizione = descrizione;
            Chiave = chiave;
            NumeroRecord = numeroRecord;
        }

        public string NomeDizionario { get; set; }
        public string Descrizione { get; set; }
        public string Chiave { get; set; }
        public int NumeroRecord { get; set; }
    }
}