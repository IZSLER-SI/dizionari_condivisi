using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System;
using System.Collections.Generic;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneDizionario.Models
{
    public class GestioneDizionarioModel
    {
        public RowDizionarioModel dizionarioModel;
        public bool DictionarySuperUser;
        public bool IsError;
        public bool HasCampiModificabili;

        public GestioneDizionarioModel(string tbl, bool isError)
        {
            IsError = isError;

            if (!isError)
            {
                dizionarioModel = new RowDizionarioModel(tbl);
                
                string username = WebUtils.GetCurrentUsername();
                Dictionary<string, string> role_dictionary = DbExtensions.GetTableAsDictionary(
                    "ruolo_dizionario", "nome_utente",
                    "CAST (super_user AS NCHAR(1))", "nome_utente",
                    new Dictionary<string, string>() { { "nome_utente", username }, { "dizionario", tbl } }
                );

                if (role_dictionary.ContainsKey(username) && role_dictionary[username].Equals("1"))
                {
                    DictionarySuperUser = true;
                }
                else
                {
                    DictionarySuperUser = false;
                }

                string campiModificabili = DbExtensions.GetValueInTable(tbl, "elenco_dizionari", "dizionario", "campi_modificabili", true);
                HasCampiModificabili = String.IsNullOrWhiteSpace(campiModificabili) ? false : true;

                foreach (FieldInformation field in dizionarioModel.fields)
                {
                    field.can_edit = HasCampiModificabili ? campiModificabili.Contains(field.name) : false;
                }
            }
        }
    }
}