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
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAutorizzaUtente.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System.Collections.Generic;
using System.Data.SqlClient;
using DotNetNuke.Services.Mail;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAutorizzaUtente.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            AutorizzaUtenteModel model = new AutorizzaUtenteModel();

            return View(model);
        }

        public ActionResult LoadData()
        {
            try
            {
                ModuleBase mb = new ModuleBase();
                List<UnauthorizedUser> list = new List<UnauthorizedUser>();

                var users = WebUtils.GetUnAuthorizedUsers(mb.PortalId);
                foreach (UserInfo user in users)
                {
                    UnauthorizedUser uu = new UnauthorizedUser(
                        user.UserID, user.FirstName, user.LastName, 
                        user.Email, user.CreatedOnDate.ToString("dd-MM-yyyy HH:mm:ss")
                    );

                    list.Add(uu);
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

        public ActionResult AuthorizeUser(int userId)
        {
            try
            {
                ModuleBase mb = new ModuleBase();

                UserInfo ui = UserController.GetUserById(mb.PortalId, userId);
                
                bool status = DbExtensions.AddUsernameToDizionario(ui.Username);
                ui.Membership.Approved = status;

                if (!status)
                {
                    throw new Exception(String.Format("Si è verificato un errore durante la gestione dei privilegi di {0}", ui.DisplayName));
                }

                UserController.UpdateUser(mb.PortalId, ui);
                Mail.SendMail(ui, MessageType.UserAuthorized, mb.PortalSettings);

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { userId = userId, authorized = true }
                );

                return Json(
                    String.Format("L'utente {0} è stato autorizzato con successo", ui.DisplayName), 
                    JsonRequestBehavior.AllowGet
                );
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Error", null, new { msg = e.Message, e = e }
                );

                return Json(
                    "Si è verificato un problema durante il processo si autorizzazione: " + e.Message,
                    JsonRequestBehavior.AllowGet
                );
            }
        }
    }
}
