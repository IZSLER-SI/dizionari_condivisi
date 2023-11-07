/*
' Copyright (c) 2021 Invisiblefarm.it
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/
using DotNetNuke.Common.Lists;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidProfilo.Models;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System;
using System.Web.Mvc;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidProfilo.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadOption()
        {
            try
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start");

                ListController le = new ListController();
                var lic = le.GetListEntryInfoItems(UserProperties.PROPERTY_ENTE_APPARTENENZA);

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End", null, new { lic = lic });

                return Json(lic, JsonRequestBehavior.AllowGet);
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

        public ActionResult LoadData()
        {
            try
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start");

                UserInfo currentUser = WebUtils.GetCurrentUser();

                CurrentUser cu = new CurrentUser()
                {
                    IsSuperUser = currentUser.IsSuperUser,
                    IsAdmin = currentUser.IsAdmin,
                    IsSpid = (currentUser.Username.Length == 16 && !currentUser.Username.ToLower().Contains("azure-")),
                    Nome = currentUser.FirstName,
                    Cognome = currentUser.LastName,
                    Nickname = currentUser.DisplayName,
                    CodiceFiscale = currentUser.Profile.GetPropertyValue(UserProperties.PROPERTY_CODICE_FISCALE),
                    EmailLavoro = currentUser.Email,
                    EnteAppartenenza = currentUser.Profile.GetPropertyValue(UserProperties.PROPERTY_ENTE_APPARTENENZA),
                    IndirizzoLavoro = currentUser.Profile.Street,
                    TelefonoLavoro = currentUser.Profile.Telephone
                };

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End", null, new { user = cu });

                return Json(cu, JsonRequestBehavior.AllowGet);
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

        public ActionResult Save(string ente, string mail, string address, string phone)
        {
            try
            {
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                    "Start", null, new { ente = ente, mail = mail, address = address, phone = phone }
                );

                UserInfo currentUser = WebUtils.GetCurrentUser();
                currentUser.Email = mail;
                currentUser.Profile.Street = address;
                currentUser.Profile.Telephone = phone;
                currentUser.Profile.SetProfileProperty(UserProperties.PROPERTY_ENTE_APPARTENENZA, ente);
                
                UserController.UpdateUser(currentUser.PortalID, currentUser);

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End");

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
