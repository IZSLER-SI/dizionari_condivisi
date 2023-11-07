using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAutorizzaUtente.Models
{
    public class UnauthorizedUser
    {
        public int UserId { get; private set; }
        public string Nome { get; private set; }
        public string Cognome { get; private set; }
        public string Email { get; private set; }
        public string DataCreazione { get; private set; }

        public UnauthorizedUser(int userId, string nome, string cognome, string email, string dataCreazione)
        {
            UserId = userId;
            Nome = nome;
            Cognome = cognome;
            Email = email;
            DataCreazione = dataCreazione;
        }
    }
}