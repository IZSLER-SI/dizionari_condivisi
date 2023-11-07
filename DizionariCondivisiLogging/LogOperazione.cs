using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System.Data;
using DotNetNuke.Instrumentation;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging
{
    public class LogOperazione : DatabaseField
    {
        public int id_log_operazione { get; set; }
        public int anno { get; set; }
        public string utente { get; set; }
        public string modulo { get; set; }
        public string operazione { get; set; }
        public string status { get; set; }
        public string dizionario { get; set; }
        public object dati { get; set; }
        public DateTime data_operazione { get; set; }
        
        public LogOperazione(int anno, string utente, string modulo,
            string operazione, string status, string dizionario,
            object dati, DateTime data_operazione,
            string extend, DateTime time_modifica, int record_attivo) :
            base(extend, time_modifica, record_attivo)
        {
            this.anno = anno;
            this.utente = utente;
            this.modulo = modulo;
            this.operazione = operazione;
            this.dizionario = dizionario;
            this.status = status;
            if (dati != null)
            {
                this.dati = JsonConvert.SerializeObject(dati);
            }
            else
            {
                this.dati = null;
            }
            this.data_operazione = data_operazione;
        }

        public int insert()
        {
            string query = @"
                INSERT INTO log_operazione (
                    anno, utente, modulo,
                    operazione, status, dati, data_operazione,
                    time_modifica, record_attivo, dizionario
                )
                VALUES (
                    @anno, @utente, @modulo,
                    @operazione, @status, @dati, @data_operazione,
                    @time_modifica, @record_attivo, @dizionario
                );
                SELECT IsNull(SCOPE_IDENTITY(), -1)
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValueOrNull("@anno", anno);
                    command.Parameters.AddWithValueOrNull("@utente", utente);
                    command.Parameters.AddWithValueOrNull("@modulo", modulo);
                    command.Parameters.AddWithValueOrNull("@operazione", operazione);
                    command.Parameters.AddWithValueOrNull("@status", status);
                    command.Parameters.AddWithValueOrNull("@dizionario", dizionario);
                    command.Parameters.AddWithValueOrNull("@dati", dati);
                    command.Parameters.AddWithValueOrNull("@data_operazione", data_operazione);
                    command.Parameters.AddWithValueOrNull("@time_modifica", time_modifica);
                    command.Parameters.AddWithValueOrNull("@record_attivo", record_attivo);
                    int id_log_operazione = command.ExecuteScalar().ToString().ToIntDef(-1);

                    if (id_log_operazione < 1)
                    {
                        Log.Error("Errore nell'inserimento in log_operazione: id_log_operazione=" + id_log_operazione);
                    }

                    return id_log_operazione;
                }
            }
        }
        
        //internal static log_operazione GetLogOperazione(int id)
        //{
        //    //TODO: implementare, se serve
        //}
    }
}
