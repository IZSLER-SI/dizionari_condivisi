/*
' Copyright (c) 2021 Invisiblefarm S.R.L.
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiImpostaAdmin.Models;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiImpostaAdmin.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            Home model = new Home();

            return View(model);
        }

        public ActionResult LoadData()
        {
            try
            {
                ModuleBase mb = new ModuleBase();
                List<PortalUser> list = new List<PortalUser>();

                var users = WebUtils.GetUsers(mb.PortalId);
                foreach (UserInfo ui in users)
                {
                    PortalUser uu = new PortalUser(
                        ui.UserID, ui.FirstName, ui.LastName, 
                        ui.Email, ui.CreatedOnDate.ToString("dd-MM-yyyy HH:mm:ss"),
                        ui.Membership.Approved, ui.IsInRole(UserRoles.ROLE_AMMINISTRATORE)
                    );

                    list.Add(uu);
                }

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "End", null, new { userlist = list.ToArray() }
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

        public ActionResult Grant2Admin(int userId)
        {
            try
            {
                ModuleBase mb = new ModuleBase();
                UserInfo ui = WebUtils.GetUserById(mb.PortalId, userId);

                RoleInfo ri = WebUtils.GetRoleByName(ui.PortalID, UserRoles.ROLE_AMMINISTRATORE);
                WebUtils.AddUserRole(ui.PortalID, ui.UserID, ri.RoleID);

                WebUtils.InsertUpdateNotifyDigest(ui.PortalID, ui.UserID);

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

        public ActionResult Revoke2Admin(int userId)
        {
            try
            {
                ModuleBase mb = new ModuleBase();
                UserInfo ui = WebUtils.GetUserById(mb.PortalId, userId);
                
                RoleInfo ri = WebUtils.GetRoleByName(ui.PortalID, UserRoles.ROLE_AMMINISTRATORE);
                WebUtils.UpdateUserRole(ui.PortalID, ui.UserID, ri.RoleID, RoleStatus.Disabled, false, true);

                WebUtils.InsertUpdateNotifyDigest(ui.PortalID, ui.UserID, 2, 2);

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
