
using System;
using System.Collections.Generic;

using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli.Models
{
    public class RuoloModel
    {
        public string Nome;
        public string Cognome;
        public string Username;
        public string Dizionario;
        public Boolean? SuperUser; //0: user 1: superuser NULL: nessun legame

        public RuoloModel() { }

        public RuoloModel(string Nome,string Cognome,string Username, string Dizionario, Boolean? SuperUser)
        {
            this.Nome = Nome;
            this.Cognome = Cognome;
            this.Username = Username;
            this.Dizionario = Dizionario;
            this.SuperUser = SuperUser;
        }

        public bool GetSuperUserBit()
        {
            if (this.SuperUser == false)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}