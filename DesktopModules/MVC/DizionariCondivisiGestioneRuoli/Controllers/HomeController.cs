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
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using DotNetNuke.Entities.Users;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneRuoli.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult loadDizionari()
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start");   

            try
            {
                string query = @"
                    SELECT ELENCO_DIZIONARI.Dizionario 
                    FROM ELENCO_DIZIONARI
                    ORDER BY Dizionario
                ";

                List<DizionarioModel> listaDizionario = new List<DizionarioModel>();
                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string Dizionario = reader.IsDBNull(0) ? "" : reader.GetString(0);

                                listaDizionario.Add(new DizionarioModel(Dizionario));
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

        public ActionResult loadRuoli()
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start");
            List<UserModel> ListUser = GetListUsers();
            try
            {
                string query = @"
                    SELECT Nome_utente, Dizionario, Super_user
                    FROM  RUOLO_DIZIONARIO
	                WHERE Record_attivo = 1
                    ORDER BY Nome_utente,Dizionario
                ";

                List<RuoloModel> listaRuoli = new List<RuoloModel>();
                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string Username = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                string Dizionario = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                UserModel UserFind = ListUser.Find(user => user.Username == Username) ?? new UserModel("","","");
                                string Nome = UserFind.Nome;
                                string Cognome = UserFind.Cognome;
                                Boolean? SuperUser;
                                if (reader.IsDBNull(2)) {
                                     SuperUser = null;
                                }
                                else
                                {
                                     SuperUser = reader.GetBoolean(2);
                                }

                                listaRuoli.Add(new RuoloModel(Nome,Cognome,Username, Dizionario, SuperUser));
                            }
                        }
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { result = listaRuoli });

                return Json(listaRuoli, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e });

                throw e; // Così viene generato un errore catturato da datatables
            }
        }

        public ActionResult saveData(string data)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start", null, new { data = data });

            bool status = false;
            string msg = "";

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<RuoloModel> jsdata = serializer.Deserialize<List<RuoloModel>>(data);

                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    string query = @"
                        UPDATE RUOLO_DIZIONARIO 
                        SET Super_user = @superUser, 
                            Time_modifica = CURRENT_TIMESTAMP,
                            extend = @UtenteAdmin
                        WHERE Nome_utente = @username AND 
                            Dizionario = @dizionario;
                    ";

                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        foreach (RuoloModel model in jsdata)
                        {
                            string username = model.Username;
                            string dizionario = model.Dizionario;
                            Boolean? superUser = model.SuperUser;

                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@dizionario", dizionario);
                                command.Parameters.AddWithValue("@username", username);
                                command.Parameters.AddWithValue("@UtenteAdmin", WebUtils.GetCurrentUsername());

                                if (superUser.HasValue)
                                {
                                    command.Parameters.AddWithValue("@superuser", model.GetSuperUserBit());
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("@superuser", DBNull.Value);
                                }

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        status = true;
                        msg = "Aggiornamento ruoli effettuato con successo";

                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                            "End", null, new { status = status, msg = msg });
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();

                        status = true;
                        msg = "Si è verificato un errore durante l'aggiornamento dei dati. ERRORE: " + e.Message;

                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                            "Error", null, new { status = status, msg = msg, e = e });
                    }

                    return Json(new { status = status, msg = msg }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { msg = e.Message, e = e });

                throw e; // Così viene generato un errore catturato da datatables
            }
        }

        private List<UserModel> GetListUsers()
        {
            ModuleBase mb = new ModuleBase();
            List<UserModel> list = new List<UserModel>();

            var users = WebUtils.GetUsers(mb.PortalId);
            foreach (UserInfo user in users)
            {
                UserModel um = new UserModel(user.Username, user.FirstName, user.LastName);

                list.Add(um);
            }

            return list;
        }
    }
}
