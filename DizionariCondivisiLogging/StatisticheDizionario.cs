using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging
{
    public class StatisticheDizionario : DatabaseField
    {
        public string dizionario { get; set; }
        public string descrizione_dizionario { get; set; }
        public string chiave_dizionario { get; set; }
        public int numero_record { get; set; }
        public string utente { get; set; }

        public StatisticheDizionario(string dizionario, string descrizione_dizionario, string chiave_dizionario,
            int numero_record, string utente,
            string extend, DateTime time_modifica, int record_attivo
        ) : base(extend, time_modifica, record_attivo)
        {
            this.dizionario = dizionario;
            this.descrizione_dizionario = descrizione_dizionario;
            this.chiave_dizionario = chiave_dizionario;
            this.numero_record = numero_record;
            this.utente = utente;
        }

        public StatisticheDizionario(string dizionario, int numero_record, string utente,
            string extend, DateTime time_modifica, int record_attivo
        ) : base(extend, time_modifica, record_attivo)
        {
            this.dizionario = dizionario;
            this.numero_record = numero_record;
            this.utente = utente;
        }

        public void InsertStatisticheDizionario()
        {
            string query = @"
                INSERT INTO elenco_dizionari (
                    dizionario, descrizione, chiave,
                    num_record, utente_creazione,
                    time_modifica, record_attivo
                ) VALUES (
	                @Dizionario, @Descrizione, @Chiave,
                    @NumeroRecord, @UtenteCreazione,
                    CURRENT_TIMESTAMP, 1
                )
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValueOrNull("@Dizionario", dizionario);
                    command.Parameters.AddWithValueOrNull("@Descrizione", descrizione_dizionario);
                    command.Parameters.AddWithValueOrNull("@Chiave", chiave_dizionario);
                    command.Parameters.AddWithValueOrNull("@NumeroRecord", numero_record);
                    command.Parameters.AddWithValueOrNull("@UtenteCreazione", utente);

                    command.ExecuteScalar();
                }
            }
        }

        public void UpdateStatisticheDizionario()
        {
            string query = @"
                UPDATE elenco_dizionari
                SET data_ultima_modifica = CURRENT_TIMESTAMP,
	                num_record = @NumeroRecord,
	                utente_ultima_modifica = @Utente,
	                time_modifica = CURRENT_TIMESTAMP,
                    record_attivo = 1
                WHERE dizionario = @Dizionario
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValueOrNull("@Dizionario", dizionario);
                    command.Parameters.AddWithValueOrNull("@NumeroRecord", numero_record);
                    command.Parameters.AddWithValueOrNull("@Utente", utente);

                    command.ExecuteScalar();
                }
            }
        }
    }
}
