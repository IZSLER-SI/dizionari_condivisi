using System;
using System.Collections.Generic;

using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli.Models
{
    public class UserModel
    {
        public string Username;
        public string Nome;
        public string Cognome;

        public UserModel() { }

        public UserModel(string Username,string Nome,string Cognome)
        {
            this.Username = Username;
            this.Nome = Nome;
            this.Cognome = Cognome;
        }
    }
}