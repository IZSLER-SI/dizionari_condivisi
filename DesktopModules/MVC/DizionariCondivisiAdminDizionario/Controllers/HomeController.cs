/*
' Copyright (c) 2020 Invisiblefarm SRL
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
using System.Linq;
using System.Web.Mvc;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using DotNetNuke.Collections;
using System.Collections;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            string adminAction = ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("DizionariCondivisiAdminDizionario_AdminAction", "");

            if (String.IsNullOrWhiteSpace(adminAction))
            {
                throw new Exception("Module setting AdminAction NON impostato");
            }

            AdminDizionarioModel model = null;
            string dizionario = this.Request.Params["diz"];
            int step = Convert.ToInt32(this.Request.Params["step"]);
            int diznumfield = this.Request.Params["diznumfield"].ToIntDef(0);

            if (adminAction.Equals("create"))
            {
                model = new AdminDizionarioModel(
                    step,
                    dizionario, this.Request.Params["dizkey"],
                    diznumfield,
                    this.Request.Params["dizdesc"],
                    adminAction
                );
            }
            else if (adminAction.Equals("modify"))
            {
                if (String.IsNullOrEmpty(dizionario))
                {
                    model = new AdminDizionarioModel(step, dizionario, adminAction);
                }
                else
                {
                    RowDizionarioModel rdm = new RowDizionarioModel(dizionario, 
                        DbExtensions.GetValueInTable(dizionario, "elenco_dizionari", "dizionario", "campi_modificabili")
                    );
                    int numFixedFields = 9;
                    model = new AdminDizionarioModel(
                        step,
                        dizionario,
                        DbExtensions.GetValueInTable(dizionario, "elenco_dizionari", "dizionario", "chiave", true),
                        diznumfield + (rdm.fields.Count - numFixedFields),
                        DbExtensions.GetValueInTable(dizionario, "elenco_dizionari", "dizionario", "descrizione", true),
                        adminAction,
                        rdm.fields.GetRange(numFixedFields, rdm.fields.Count - numFixedFields)
                    );
                }
            }
            else
            {
                throw new Exception("Module setting AdminAction NON valorizzato correttamente");
            }

            return View(model);
        }

        public ActionResult ValidateInput(string name, string key)
        {
            // Check name
            string result = DbExtensions.GetValueInTable(name, "elenco_dizionari", "dizionario", "dizionario");
            bool name_exist = !String.IsNullOrWhiteSpace(result);

            // Check key. Before flush list
            result = DbExtensions.GetValueInTable(key, "elenco_dizionari", "chiave", "chiave");
            bool key_exist = !String.IsNullOrWhiteSpace(result);

            return Json(new { name_exist = name_exist, key_exist = key_exist });
        }
        
        private bool CheckRegExp(string txt)
        {
            Regex regex = new Regex(DatabaseTableModel.PatternCampo);
            MatchCollection matches = regex.Matches(txt);

            return matches.Count > 0;
        }

        private List<T> DecodeFields<T>(string fields)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            List<T> decoded_tables = js.Deserialize<List<T>>(fields);

            return decoded_tables;
        }

        public ActionResult CreateTable(string dizionario, string descrizione, string chiave, string fields)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                   "Start", null, new { dizionario = dizionario, descrizione = descrizione, chiave = chiave, fields = fields });

            bool status = true;
            string msg = "";
            string result = DbExtensions.GetValueInTable(dizionario, "elenco_dizionari", "dizionario", "dizionario");
            bool name_exist = !String.IsNullOrWhiteSpace(result);
            result = DbExtensions.GetValueInTable(chiave, "elenco_dizionari", "chiave", "chiave");
            bool key_exist = !String.IsNullOrWhiteSpace(result);

            string campiModificabili = "";

            if (name_exist || key_exist)
            {
                status = false;
                if (name_exist)
                {
                    msg = $"Dizionario scelto ({dizionario}) già in uso";
                }
                if (key_exist)
                {
                    if (msg.Trim().Length > 0)
                    {
                        msg += "<br />";
                    }
                    msg += $"Chiave scelta ({chiave}) già in uso";
                }
            }

            if (status)
            {
                if (!CheckRegExp(dizionario))
                {
                    status = false;
                    msg = $"Campo <b>Dizionario</b> non rispetta il formalismo {DatabaseTableModel.PatternCampo}";
                }
            }

            if (status)
            {
                int j = 0;
                msg = $"Dizionario <b>{dizionario}</b>: ";
                List<DatabaseTableModel> decoded_tables = DecodeFields<DatabaseTableModel>(fields);

                #region Creo la query di CREATE
                string sql_header = $"CREATE TABLE {dizionario} (";
                string sql = null;
                foreach (DatabaseTableModel item in decoded_tables)
                {
                    if (!CheckRegExp(item.Campo))
                    {
                        status = false;
                        msg = $"Campo {j}: <b>campo</b> non rispetta il formalismo {DatabaseTableModel.PatternCampo}";
                        break;
                    }

                    if (!String.IsNullOrWhiteSpace(sql))
                    {
                        sql += ", ";
                    }

                    sql += item.Campo + @" " + item.Tipo + ""
                            + (item.Dimensione.HasValue ? "(" + item.Dimensione.Value + ") " : " ")
                            + (item.IsPrimaryKey() ? "PRIMARY KEY IDENTITY(1, 1)" : " ")
                            + (item.IsNullable() ? "NULL" : "NOT NULL")
                            + (item.HasDefaultValue() ? (" DEFAULT '" + item.DefaultValue + "'") : "");

                    j += 1;

                    if (item.IsModificabile())
                    {
                        if (campiModificabili.Length > 0)
                        {
                            campiModificabili += "%";
                        }

                        campiModificabili += item.Campo;
                    }
                }
                string sql_footer = ");";
                string create_table = String.Concat(sql_header, sql, sql_footer);
                #endregion

                #region Creo la query per il trigger
                string sql_trigger = $@"
                    CREATE TRIGGER dbo.trg_{dizionario}_last_update 
                        ON dbo.{dizionario} AFTER INSERT, UPDATE
                    AS 
                    BEGIN
	                    -- SET NOCOUNT ON added to prevent extra result sets from
	                    -- interfering with SELECT statements.
	                    SET NOCOUNT ON;

	                    UPDATE d
	                    SET contatore = t.RowNo
	                    FROM {dizionario} d 
                            INNER JOIN (
		                        SELECT d.id, ROW_NUMBER() OVER (ORDER BY d.id) as RowNo
		                        FROM {dizionario} d
	                        ) AS t ON d.id = t.id
	                    WHERE d.contatore IS NULL;

                        UPDATE t
                        SET last_update = GETDATE(),
                            chiave_composta = t.chiave + RIGHT('00000' + CAST(t.contatore AS nvarchar(5)), 5)
                        FROM dbo.{dizionario} AS t
                        INNER JOIN inserted AS i
                            ON t.id = i.id;
                    END
                ";
                #endregion

                if (status) { 
                    string define_dizionario = @"
                        INSERT INTO elenco_dizionari (
                            dizionario, descrizione, chiave, num_record,
                            utente_creazione, time_modifica, record_attivo, campi_modificabili
                        ) VALUES (
                            @Dizionario, @Descrizione, @Chiave, 0,
                            @Utente, CURRENT_TIMESTAMP, 1, @CampiModificabili
                        )
                    ";

                    #region Recupero tutti gli utenti autorizzati
                    ModuleBase mb = new ModuleBase();
                    ArrayList u = WebUtils.GetUsers(mb.PortalId);

                    List<string> users = new List<string>();
                    foreach (UserInfo info in u) { 
                        if (info.Membership.Approved)
                        {
                            users.Add(info.Username);
                        }
                    }

                    string define_ruolo = @"
                        INSERT INTO ruolo_dizionario (
                            nome_utente, dizionario, time_modifica, record_attivo
                        ) VALUES (
                            @Utente, @Dizionario, CURRENT_TIMESTAMP, 1
                        )
                    ";
                    #endregion

                    #region Eseguo le query
                    using (SqlConnection connection = DbExtensions.GetSqlConnection())
                    {
                        connection.Open();

                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            using (SqlCommand command = new SqlCommand(create_table, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = new SqlCommand(sql_trigger, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            using (SqlCommand command = new SqlCommand(define_dizionario, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Dizionario", dizionario);
                                command.Parameters.AddWithValue("@Descrizione", descrizione);
                                command.Parameters.AddWithValue("@Chiave", chiave);
                                command.Parameters.AddWithValue("@Utente", WebUtils.GetCurrentUsername());
                                command.Parameters.AddWithValue("@CampiModificabili", campiModificabili);

                                command.ExecuteNonQuery();
                            }

                            foreach (string user in users)
                            {
                                using (SqlCommand command = new SqlCommand(define_ruolo, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@Utente", user);
                                    command.Parameters.AddWithValue("@Dizionario", dizionario);

                                    command.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            status = true;
                            msg += "creato con successo. <br />Verrai re-indirizzato automaticamente " +
                                "alla pagina di gestione dei ruoli";
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();

                            status = false;
                            msg += ("ERRORE: " + e.Message); 
                            
                            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                                "Error", null, new { status = status, msg = msg });
                        }
                    }
                    #endregion
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                   "End", null, new { status = status, msg = msg });

            return Json(new { status = status, msg = msg});
        }

        public ActionResult LoadElencoDizionario()
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start");

            List<DizionarioModel> listaDizionario = new List<DizionarioModel>();

            string query = @"
                SELECT ed.dizionario, ed.descrizione, ed.chiave, ed.num_record
                FROM elenco_dizionari ed
                WHERE ed.record_attivo = 1
                ORDER BY ed.dizionario
            ";

            try {

                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (var command = new SqlCommand(query, connection)) 
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string row_nome_dizionario = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                string row_descrizione = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                string row_chiave = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                int row_numero_record = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);

                                DizionarioModel d = new DizionarioModel(
                                    row_nome_dizionario, row_descrizione,
                                    row_chiave, row_numero_record
                                );

                                listaDizionario.Add(d);
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

        public ActionResult ModifyTable(string dizionario, string fields)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "Start", null, new { dizionario = dizionario, fields = fields });

            int j = 0;
            bool status = true;
            string currentModificabili = "";
            string originalCampiModificabili = DbExtensions.GetValueInTable(dizionario, "elenco_dizionari", "dizionario", "campi_modificabili");
            bool originalCampiModificabiliHasValue = !String.IsNullOrWhiteSpace(originalCampiModificabili);
            string msg = $"Dizionario <b>{dizionario}</b>: ";
            List<DatabaseTableModel> decoded_tables = DecodeFields<DatabaseTableModel>(fields);

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    foreach (DatabaseTableModel item in decoded_tables)
                    {
                        if (!CheckRegExp(item.Campo))
                        {
                            status = false;
                            msg = "Campo " + j + ": <b>campo</b> non rispetta il formalismo " + DatabaseTableModel.PatternCampo;
                            break;
                        }

                        if (!item.IsNullable() && !item.HasDefaultValue())
                        {
                            status = false;
                            msg = $"Campo {j}: NON è consentito aggiungere una colonna obbligatoria priva di valore di default";
                            break;
                        }

                        string query_field = "ALTER TABLE " + dizionario + item.GetAlterOperation() + item.Campo + " " + item.Tipo
                                + (item.Dimensione.HasValue ? "(" + item.Dimensione.Value + ") " : " ")
                                + (item.IsNullable() ? "NULL" : "NOT NULL")
                                /*+ (item.HasDefaultValue() && item.IsNewField() 
                                    ? (" CONSTRAINT " + item.GetDefaultConstraintName(dizionario) + " DEFAULT " + item.GetDefaultValue()) 
                                    : ""
                                )*/;

                        using (SqlCommand command = new SqlCommand(query_field, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        if (!item.IsNewField())
                        {
                            // Se ha già un default value, lo elimino e poi lo ricreo
                            string defaultName = null;
                            string query_check_df_constraint = @"
                                SELECT d.name
                                FROM sys.default_constraints AS d  
	                                INNER JOIN sys.columns AS c ON (
		                                d.parent_object_id = c.object_id AND 
		                                d.parent_column_id = c.column_id AND
		                                c.name = @Colonna
	                                )
                                WHERE d.parent_object_id = OBJECT_ID(@Dizionario, 'U') 
                            ";

                            using (SqlCommand command = new SqlCommand(query_check_df_constraint, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Dizionario", dizionario);
                                command.Parameters.AddWithValue("@Colonna", item.Campo);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        defaultName = reader.IsDBNull(0) ? null : reader.GetString(0);
                                    }
                                }
                            }

                            if (!String.IsNullOrWhiteSpace(defaultName))
                            {
                                string query_delete_df = $"ALTER TABLE {dizionario} DROP CONSTRAINT {defaultName}";

                                using (SqlCommand command = new SqlCommand(query_delete_df, connection, transaction))
                                {
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        if (item.HasDefaultValue())
                        {
                            string query_add_df = $@"ALTER TABLE {dizionario} 
                                ADD CONSTRAINT {item.GetDefaultConstraintName(dizionario)} 
                                    DEFAULT {item.GetDefaultValue()} FOR {item.Campo}
                            ";

                            using (SqlCommand command = new SqlCommand(query_add_df, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        if (item.IsModificabile())
                        {
                            if (!originalCampiModificabiliHasValue || !originalCampiModificabili.Contains(item.Campo))
                            {
                                if (currentModificabili.Length > 0)
                                {
                                    currentModificabili += "%";
                                }

                                currentModificabili += item.Campo;
                            }
                        }
                        else
                        {
                            if (originalCampiModificabiliHasValue)
                            {
                                originalCampiModificabili = originalCampiModificabili.Replace(item.Campo, "").Replace("%%", "%");
                            }
                        }

                        status = true;
                        msg += "<br />Campo " + item.Campo + (item.IsNewField() ? " aggiunto" : " aggiornato") + " con successo";

                        j += 1;
                    }

                    while (originalCampiModificabiliHasValue && originalCampiModificabili.StartsWith("%"))
                    {
                        originalCampiModificabili = originalCampiModificabili.Remove(0, 1);
                    }

                    // Non controllo la variabile originalCampiModificabiliHasValue perché
                    // sono interessato allo stato attuale, dopo le modifiche ai vari campi
                    string campiModificabili = String.IsNullOrWhiteSpace(originalCampiModificabili)
                        ? currentModificabili
                        : $"{originalCampiModificabili}%{currentModificabili}";
                    // Se lo stato attuale è solo %, inizilizzo a zero
                    if (campiModificabili.Length == 1 && campiModificabili.Equals("%"))
                    {
                        campiModificabili = campiModificabili.Replace("%", "");
                    }

                    string queryUpdateCampiModificabili = @"
                        UPDATE elenco_dizionari
                        SET campi_modificabili = @CampiModificabili,
                            time_modifica = CURRENT_TIMESTAMP
                        WHERE dizionario = @Dizionario
                    ";

                    using (SqlCommand command = new SqlCommand(queryUpdateCampiModificabili, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Dizionario", dizionario);
                        if (String.IsNullOrWhiteSpace(campiModificabili))
                        {
                            command.Parameters.AddWithValue("@CampiModificabili", DBNull.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@CampiModificabili", campiModificabili);
                        }

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    status = false;
                    msg = ("ERRORE: " + e.Message);

                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                        "Error", null, new { status = status, msg = msg });
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                   "End", null, new { status = status, msg = msg });

            return Json(new { status = status, msg = msg });
        }
        
        public ActionResult GetTableStructure(string dizionario)
        {
            RowDizionarioModel rdm = new RowDizionarioModel(dizionario);
            
            return Json(new { response = rdm.fields });
        }
    }
}
