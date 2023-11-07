using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiEliminaUtente.Models
{
    public class PortalUser
    {
        public int UserId { get; private set; }
        public string Nome { get; private set; }
        public string Cognome { get; private set; }
        public string Email { get; private set; }
        public string DataCreazione { get; private set; }
        public bool IsAutorizzato { get; private set; }
        public string Utente { get; private set; }
        public string DataOperazione { get; private set; }

        public PortalUser(int userId, string nome, string cognome, string email, string dataCreazione, 
            bool isAutorizzato, string utente, string dataOperazione)
        {
            UserId = userId;
            Nome = nome;
            Cognome = cognome;
            Email = email;
            DataCreazione = dataCreazione;
            IsAutorizzato = isAutorizzato;
            Utente = utente;
            DataOperazione = dataOperazione;
        }
    }
}