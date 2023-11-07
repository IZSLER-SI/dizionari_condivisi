namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiImpostaAdmin.Models
{
    public class PortalUser
    {
        public int UserId { get; private set; }
        public string Nome { get; private set; }
        public string Cognome { get; private set; }
        public string Email { get; private set; }
        public string DataCreazione { get; private set; }
        public bool IsAutorizzato { get; private set; }
        public bool IsAdmin { get; private set; }

        public PortalUser(int userId, string nome, string cognome, string email, string dataCreazione, 
            bool isAutorizzato, bool isAdmin)
        {
            UserId = userId;
            Nome = nome;
            Cognome = cognome;
            Email = email;
            DataCreazione = dataCreazione;
            IsAutorizzato = isAutorizzato;
            IsAdmin = isAdmin;
        }
    }
}