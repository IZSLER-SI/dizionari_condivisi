/*
' Copyright (c) 2020 invisiblefarm SRL
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Web.Mvc;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using DotNetNuke.Instrumentation;
using System.Collections.Generic;
using System.Data.SqlClient;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using DotNetNuke.Services.Mail;
using System.Configuration;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiListaTabelle.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult loaddata()
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start");

            List<Dizionario> listaDizionario = new List<Dizionario>();
            bool isPortalSuperUser = WebUtils.UserIsSuperUser();
            bool isPortalAdmin = WebUtils.UserHasRole(UserRoles.ROLE_AMMINISTRATORE);
            string username = WebUtils.GetCurrentUsername();
            string join_ruolo = "";
            string select_ruolo = ", CAST(0 AS BIT)";

            if (!isPortalSuperUser)
            {
                join_ruolo = @"
                    LEFT JOIN (
	                    SELECT rd.dizionario, rd.nome_utente, rd.super_user
	                    FROM ruolo_dizionario rd
	                    WHERE rd.record_attivo = 1
		                    AND rd.super_user IS NOT NULL
		                    AND rd.nome_utente = @Username
                    ) rd ON (ed.dizionario = rd.dizionario)
                ";

                select_ruolo = ", rd.super_user";
            }

            try
            {
                string query = @"
                    SELECT ed.Dizionario, ed.Descrizione, ed.Data_ultima_modifica, 
                        ed.num_record" + select_ruolo + @"
                    FROM ELENCO_DIZIONARI ed " + join_ruolo + @"
                    WHERE ed.record_attivo = 1
                    ORDER BY ed.Dizionario
                ";

                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (!isPortalSuperUser)
                        {
                            command.Parameters.AddWithValue("@Username", username);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string row_nome_dizionario = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                string row_descrizione = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                string row_data_ultima_modifica = reader.IsDBNull(2) 
                                    ? "" 
                                    : DateExtensions.OnlyDate(reader.GetDateTime(2), "dd-MM-yyyy HH:mm:ss");
                                DateTime row_data_ultima_modifica2 = reader.IsDBNull(2) 
                                    ? DateTime.MinValue
                                    : reader.GetDateTime(2);
                                int row_numero_record = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                                bool? row_isDictionarySuperUser = null; //Nessun ruolo definito QUINDI non devo vederlo
                                if (!reader.IsDBNull(4))
                                {
                                    row_isDictionarySuperUser = reader.GetBoolean(4);
                                }

                                if (row_isDictionarySuperUser.HasValue || isPortalSuperUser || isPortalAdmin)
                                {
                                    Dizionario d = new Dizionario(
                                        row_nome_dizionario, row_descrizione,
                                        row_data_ultima_modifica, row_numero_record,
                                        row_isDictionarySuperUser.HasValue ? row_isDictionarySuperUser.Value : false,
                                        isPortalAdmin, isPortalSuperUser, row_data_ultima_modifica2
                                    );

                                    listaDizionario.Add(d);
                                }
                            }
                        }
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { result = listaDizionario });

                return Json(listaDizionario, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e });
                
                throw e; // Così viene generato un errore catturato da datatables
            }
        }

        private bool IncludeField(string fieldName) => !fieldName.Equals("id") && !fieldName.Equals("chiave") && !fieldName.Equals("contatore");

        public ActionResult DownloadDizionario(string dizionario)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null, 
                new { dizionario = dizionario });

            string s = "";
            string query = $"SELECT * FROM {dizionario} ORDER BY id";
            ARowDizionarioModel dizionarioModel = new RowDizionarioModel(dizionario);
            List<string> tmp = new List<string>();
            List<List<string>> result = new List<List<string>>();
            foreach (FieldInformation field in dizionarioModel.fields)
            {
                if (IncludeField(field.name))
                {
                    tmp.Add($"\"{field.name}\"");
                }
            }
            result.Add(tmp);
            tmp = new List<string>();

            try
            {
                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (IncludeField(reader.GetName(i)))
                                    {
                                        string row = !reader.IsDBNull(i) ? DbExtensions.GetValueOrNull<string>(reader, reader.GetName(i), "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss") : "";
                                        tmp.Add($"\"{row}\"");
                                    }
                                }
                                result.Add(tmp);
                                tmp = new List<string>();
                            }
                        }
                    }
                }

                foreach (List<string> a in result)
                {
                    s += String.Join(";", a);
                    s += "\r\n";
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { result = s });

                return Json(new { s = s }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e });

                throw e; // Così viene generato un errore catturato da datatables
            }
        }

        public ActionResult InviaEmail(string dizionario)
        {
            NewLog nl = new NewLog(0, "");
            DateTime dt_last_invio = DateTime.MinValue;
            string result = "";
            int id_last_invio = 0;

            string query_last_invio = @"
                SELECT l.id_log_ultimo_invio, l.time_modifica
                FROM log_stato_arte_email l
	                INNER JOIN (
		                SELECT l.dizionario, MAX(l.time_modifica) maxtm
		                FROM log_stato_arte_email l
                        WHERE l.record_attivo = 1
                            AND l.dizionario = @Dizionario
		                GROUP BY l.dizionario
	                ) maxtm ON (
		                l.dizionario = maxtm.dizionario AND
		                l.time_modifica = maxtm.maxtm
	                )
                WHERE l.record_attivo = 1
                    AND l.dizionario = @Dizionario
                ORDER BY l.id DESC
            ";
            
            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query_last_invio, connection))
                {
                    command.Parameters.AddWithValue("@Dizionario", dizionario);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            id_last_invio = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            dt_last_invio = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                        }
                    }
                }
            }

            string query_seach_new_log = @"
                SELECT TOP 1 lo.id, lo.data_operazione
                FROM log_operazione lo
                WHERE lo.dizionario IS NOT NULL AND
	                lo.operazione IN ('Create', 'Edit') AND
	                lo.id > @IdLastInvio AND
	                lo.dizionario = @Dizionario
                ORDER BY lo.data_operazione DESC
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query_seach_new_log, connection))
                {
                    command.Parameters.AddWithValue("@Dizionario", dizionario);
                    command.Parameters.AddWithValue("@IdLastInvio", id_last_invio);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nl = new NewLog(
                                reader.IsDBNull(0) ? 0 : reader.GetInt32(0), 
                                reader.IsDBNull(1) ? "" : DateExtensions.OnlyDate(reader.GetDateTime(1), "dd-MM-yyyy HH:mm:ss")
                            );
                        }
                    }
                }
            }
            
            if (nl.HasNewLog())
            {
                Dictionary<string, string> utenti_dizionario_email = new Dictionary<string, string>();
                string sender = ConfigurationManager.AppSettings["DizionariCondivisi.ListaTabelle.SMTP.Sender"];

                string query = @"
                    SELECT rd.nome_utente
                    FROM ruolo_dizionario rd
                    WHERE rd.record_attivo = 1
	                    AND rd.super_user IS NOT NULL
                        AND rd.dizionario = @Dizionario
                ";

                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Dizionario", dizionario);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string row_utente = reader.GetString(0);
                                string user_email = WebUtils.GetUserEmailByUsername(row_utente);

                                utenti_dizionario_email.Add(row_utente, user_email);
                            }
                        }
                    }
                }

                try
                {
                    foreach (KeyValuePair<string, string> item in utenti_dizionario_email)
                    {
                        string mail_receiver = item.Value;

                        Mail.SendMail(sender, mail_receiver, "", "", MailPriority.Normal,
                            $"Aggiornamento dizionario {dizionario}", MailFormat.Text, System.Text.Encoding.Default,
                            "Gentile Utente, " + System.Environment.NewLine + $"si comunica che il dizionario {dizionario} è stato aggiornato in data {nl.GetDataOperazione()}", "",
                            "", "", "", ""
                        );

                        if (result.Length > 0)
                        {
                            result += "<br />";
                        }

                        result += $"Email inviata con successo a: {mail_receiver}";
                    }

                    nl.SaveNewLog(dizionario);
                }
                catch (Exception e)
                {
                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                        "Error", null, new { msg = e.Message, e = e });

                    result = $"Si è verificato un errore durante l'invio dell'email. Errore: {e.Message}";
                }
            }
            else
            {
                result = $"Non è stata inviata alcuna email perché non sono stati apportati aggiornamenti al dizionario dall'ultimo invio, avvenuto il giorno {DateExtensions.OnlyDate(dt_last_invio, "dd-MM-yyyy HH:mm:ss")}, ad oggi ";
            }

            return Json(new { r = result }, JsonRequestBehavior.AllowGet);
        }
    }
}