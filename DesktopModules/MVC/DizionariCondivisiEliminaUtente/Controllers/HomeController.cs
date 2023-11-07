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

using System.Web.Mvc;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiEliminaUtente.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using System.Collections.Generic;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using System;
using DotNetNuke.Entities.Users;
using System.Data.SqlClient;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiEliminaUtente.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            EliminaUtenteModel model = new EliminaUtenteModel();
            return View(model);
        }

        public ActionResult LoadData()
        {
            try
            {
                ModuleBase mb = new ModuleBase();
                List<PortalUser> list = new List<PortalUser>();

                var users = WebUtils.GetUsers(mb.PortalId);
                foreach (UserInfo user in users)
                {
                    string utente = "";
                    string dataOperazione = "";
                    //NB: lasciare single line
                    string query = "SELECT TOP (1) utente, data_operazione FROM log_operazione WHERE modulo = 'DizionariCondivisiAutorizzaUtente' AND operazione = 'AuthorizeUser' AND status = 'END' AND data_operazione >= @Data AND dati LIKE '{\"userId\":'+CAST(@UserId AS nvarchar(255))+',\"authorized\":true}' ORDER BY data_operazione DESC";

                    try
                    {
                        using (SqlConnection connection = DbExtensions.GetSqlConnection())
                        {
                            connection.Open();

                            using (var command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@Data", user.CreatedOnDate);
                                command.Parameters.AddWithValue("@UserId", user.UserID);
                                
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        utente = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                        dataOperazione = reader.IsDBNull(1) ? "" : reader.GetDateTime(1).ToString("dd-MM-yyyy HH:mm:ss");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        DizionariCondivisiLogger.LogActivity(
                            Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                            "Error", null, new { msg = e.Message, e = e }
                        );
                    }

                    PortalUser pu = new PortalUser(
                        user.UserID, user.FirstName, user.LastName,
                        user.Email, user.CreatedOnDate.ToString("dd-MM-yyyy HH:mm:ss"), user.Membership.Approved,
                        utente, dataOperazione
                    );

                    list.Add(pu);
                }

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { unauthorizeduserlist = list.ToArray() }
                );

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e }
                );

                throw e; // Così viene generato un errore catturato da datatables
            }
        }

        public ActionResult DeleteUser(int userId)
        {
            try
            {
                ModuleBase mb = new ModuleBase();

                UserInfo ui = UserController.GetUserById(mb.PortalId, userId);
                UserController.RemoveUser(ui);

                bool status = DbExtensions.RemoveUsernameFromDizionario(ui.Username);

                if (!status)
                {
                    throw new Exception("Si è verificato un errore durante la gestione dei privilegi.");
                }

                string emailSubject = Localization.GetSystemMessage(
                    "it-IT", mb.PortalSettings,
                    "EMAIL_USER_ERASED_SUBJECT", ui, Localization.GlobalResourceFile, null, string.Empty,
                    mb.PortalSettings.AdministratorId
                );
                string emailBody = Localization.GetSystemMessage(
                    "it-IT", mb.PortalSettings,
                    "EMAIL_USER_ERASED_BODY", ui, Localization.GlobalResourceFile, null, string.Empty,
                    mb.PortalSettings.AdministratorId
                );
                string fromAddress = (UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email) != null)
                    ? $"{UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email).DisplayName} < {mb.PortalSettings.Email} >"
                    : mb.PortalSettings.Email;

                Mail.SendEmail(fromAddress, fromAddress, ui.Email, emailSubject, emailBody);

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { userId = userId, authorized = true }
                );

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e }
                );

                throw e; // Così viene generato un errore catturato da datatables
            }
        }
    }
}
