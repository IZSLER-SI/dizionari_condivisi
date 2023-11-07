
using System;
using System.Collections.Generic;

using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle.Models
{
    public class Dizionario
    {
        public string NomeDizionario { get; set; }
        public string Descrizione { get; set; }
        public string DataUltimaModifica { get; set; }
        public DateTime DataUltimaModifica2 { get; set; }
        public int NumeroRecord { get; set; }
        public bool? DictionarySuperUser { get; set; }
        public bool PortalAdmin { get; set; }
        public bool PortalSuperUser { get; set; }

        public Dizionario(string nome_dizionario, string descrizione, string data_ultima_modifica,
            int numero_record, bool? dictionarySuperUser, bool portalAdmin, bool portalSuperUser)
        {
            this.NomeDizionario = nome_dizionario;
            this.Descrizione = descrizione;
            this.DataUltimaModifica = data_ultima_modifica;
            this.NumeroRecord = numero_record;
            this.DictionarySuperUser = dictionarySuperUser;
            this.PortalAdmin = portalAdmin;
            this.PortalSuperUser = portalSuperUser;
        }

        public Dizionario(string nome_dizionario, string descrizione, string data_ultima_modifica,
            int numero_record, bool? dictionarySuperUser, bool portalAdmin, bool portalSuperUser,
            DateTime dataUltimaModifica2)
        {
            this.NomeDizionario = nome_dizionario;
            this.Descrizione = descrizione;
            this.DataUltimaModifica = data_ultima_modifica;
            this.NumeroRecord = numero_record;
            this.DictionarySuperUser = dictionarySuperUser;
            this.PortalAdmin = portalAdmin;
            this.PortalSuperUser = portalSuperUser;
            this.DataUltimaModifica2 = dataUltimaModifica2;
        }
    }
}