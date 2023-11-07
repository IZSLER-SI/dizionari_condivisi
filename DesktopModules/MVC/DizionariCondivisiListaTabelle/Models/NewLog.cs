using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle.Models
{
    public class NewLog
    {
        public int Id { get; set; }
        public string Data_Operazione { get; set; }

        public NewLog(int id, string data_operazione)
        {
            this.Id = id;
            this.Data_Operazione = data_operazione;
        }

        public bool HasNewLog()
        {
            return this.Id > 0;
        }

        public string GetDataOperazione()
        {
            return this.Data_Operazione;
        }

        public void SaveNewLog(string dizionario)
        {
            string query = @"
                INSERT INTO log_stato_arte_email (
                    utente, dizionario, id_log_ultimo_invio,
                    time_modifica, record_attivo
                ) VALUES (
                    @Utente, @Dizionario, @IdLogUltimoInvio,
                    CURRENT_TIMESTAMP, 1
                )
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Utente", WebUtils.GetCurrentUsername());
                    command.Parameters.AddWithValue("@Dizionario", dizionario);
                    command.Parameters.AddWithValue("@IdLogUltimoInvio", this.Id);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
        }
    }
}