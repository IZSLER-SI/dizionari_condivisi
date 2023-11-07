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
using System.Web.Mvc;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiConsultaLog.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiConsultaLog.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            ConsultaLogModel model = new ConsultaLogModel();
            return View(model);
        }
        public ActionResult LoadLog()
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start");

            try
            {
                string query = @"
                    select lo.id, lo.anno, lo.utente, lo.modulo, lo.dizionario, lo.data_operazione
                    from ruolo_dizionario rd
	                    inner join log_operazione lo on (
		                    rd.dizionario = lo.dizionario and
		                    lo.operazione IN ('Create', 'Edit')
	                    )
	                    inner join (
		                    select lo.dizionario, MAX(lo.data_operazione) as last_data
		                    from log_operazione lo
                            where lo.dizionario IS NOT NULL AND
			                    lo.operazione IN ('Create', 'Edit')
		                    group by lo.dizionario
	                    ) last_data ON (
		                    lo.dizionario = last_data.dizionario AND
		                    lo.data_operazione = last_data.last_data
	                    )
                    where rd.record_attivo = 1
                        and rd.nome_utente = @Username 
                        and rd.super_user IS NOT NULL
                    order by lo.dizionario
                ";
                bool showAll = WebUtils.UserIsSuperUser() || WebUtils.UserHasRole(UserRoles.ROLE_AMMINISTRATORE);

                if (showAll)
                {
                    query = @"
                        select lo.id, lo.anno, lo.utente, lo.modulo, lo.dizionario, lo.data_operazione
                        from log_operazione lo
	                        inner join (
		                        select lo.dizionario, MAX(lo.data_operazione) as last_data
		                        from log_operazione lo
                                where lo.dizionario IS NOT NULL AND
			                        lo.operazione IN ('Create', 'Edit')
		                        group by lo.dizionario
	                        ) last_data ON (
		                        lo.dizionario = last_data.dizionario AND
		                        lo.data_operazione = last_data.last_data
	                        )
                        where lo.operazione IN ('Create', 'Edit')
                        order by lo.dizionario
                    ";
                }

                List<LogModel> logs = new List<LogModel>();
                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (!showAll)
                        {
                            command.Parameters.AddWithValue("@Username", WebUtils.GetCurrentUsername());
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int row_id = reader.GetInt32(0);
                                int row_anno = reader.GetInt32(1);
                                string row_utente = reader.IsDBNull(2) ? "" : reader.GetString(2);
                                string row_modulo = reader.IsDBNull(3) ? "" : reader.GetString(3);
                                string row_dizionario = reader.IsDBNull(4) ? "" : reader.GetString(4);
                                string row_data_operazione = reader.IsDBNull(5) ? "" : DateExtensions.OnlyDate(reader.GetDateTime(5), "dd-MM-yyyy HH:mm:ss");

                                logs.Add(
                                    new LogModel(
                                        row_id, row_anno, row_utente, row_modulo,
                                        row_dizionario, row_data_operazione
                                    )
                                );
                            }
                        }
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { result = logs });

                return Json(logs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                    "Error", null, new { msg = e.Message, e = e });

                throw e; // Così viene generato un errore catturato da datatables
            }
        }
    }
}
