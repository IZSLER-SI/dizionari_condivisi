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
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCreaUtente.Models;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using System.ComponentModel.DataAnnotations;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security.Membership;
using System.Collections.Generic;
using System.Data.SqlClient;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCreaUtente.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            CreaUtenteModel model = new CreaUtenteModel();

            return View(model);
        }

        public ActionResult CreateUser(string nome, string cognome, string username,
            string email, string codicefiscale, string telefono, string ente, bool admin)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start", null, new
                {
                    nome = nome,
                    cognome = cognome,
                    username = username,
                    email = email,
                    codicefiscale = codicefiscale,
                    telefono = telefono,
                    ente = ente,
                    admin = admin
                }
            );

            bool status = true;
            string msg = "";

            #region Validate input
            if (String.IsNullOrWhiteSpace(nome))
            {
                status = false;
                msg = "Campo <b>Nome</b> obbligatorio";
            }
            if (String.IsNullOrWhiteSpace(cognome))
            {
                status = false;
                if (msg.Length > 0)
                {
                    msg += "<br />";
                }
                msg += "Campo <b>Cognome</b> obbligatorio";
            }
            if (String.IsNullOrWhiteSpace(username))
            {
                status = false;
                if (msg.Length > 0)
                {
                    msg += "<br />";
                }
                msg += "Campo <b>Username</b> obbligatorio";
            }
            if (!GeneralUtils.IsValidEmail(email))
            {
                status = false;
                if (msg.Length > 0)
                {
                    msg += "<br />";
                }
                msg += "Campo <b>Email</b> obbligatorio/non corretto";
            }
            if (!GeneralUtils.IsValidCodiceFiscale(codicefiscale))
            {
                status = false;
                msg = "Campo <b>Codice Fiscale</b> obbligatorio/non corretto";
            }
            if (!GeneralUtils.IsPhoneNumber(telefono))
            {
                status = false;
                if (msg.Length > 0)
                {
                    msg += "<br />";
                }
                msg += "Campo <b>Telefono</b> obbligatorio/non corretto";
            }
            if (String.IsNullOrWhiteSpace(ente))
            {
                status = false;
                if (msg.Length > 0)
                {
                    msg += "<br />";
                }
                msg += "Campo <b>Ente di appartenenza</b> obbligatorio";
            }
            #endregion

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Mid", null, new { status = status, msg = msg });

            if (status)
            {
                try
                {
                    ModuleBase mb = new ModuleBase();
                    UserInfo user = new UserInfo();
                    user.Username = username;
                    user.Email = email;
                    user.FirstName = nome;
                    user.LastName = cognome;
                    user.DisplayName = String.Concat(nome, " ", cognome);
                    user.Membership.Password = username;
                    user.Membership.UpdatePassword = true;
                    user.Membership.Approved = true;
                    user.AffiliateID = Null.NullInteger;
                    user.PortalID = mb.PortalId;
                    user.Profile.InitialiseProfile(mb.PortalId);
                    // Basic
                    user.Profile.FirstName = nome;
                    user.Profile.LastName = cognome;
                    user.Profile.SetProfileProperty(UserProperties.PROPERTY_CODICE_FISCALE, codicefiscale); // Custom property
                                                                                                            // Contact
                    user.Profile.Cell = telefono;
                    user.Profile.Telephone = telefono;
                    // Location
                    user.Profile.SetProfileProperty(UserProperties.PROPERTY_ENTE_APPARTENENZA, ente); // Custom property

                    UserCreateStatus ucs = UserController.CreateUser(ref user);

                    if (ucs == UserCreateStatus.Success)
                    {
                        msg = "L'utente è stato creato con successo";

                        if (admin)
                        {
                            RoleController roleController = new RoleController();
                            RoleInfo role = roleController.GetRoleByName(user.PortalID, UserRoles.ROLE_AMMINISTRATORE);
                            roleController.AddUserRole(user.PortalID, user.UserID, role.RoleID, DateTime.MinValue, DateTime.MaxValue);
                        }

                        status = DbExtensions.AddUsernameToDizionario(username);

                        if (!status)
                        {
                            throw new Exception(".<br />Si è verificato un errore durante la gestione dei privilegi.<br />");
                        }

                        // Notify all admin...maybe
                        IList<UserInfo> managersList = RoleController.Instance.GetUsersByRole(user.PortalID, UserRoles.ROLE_AMMINISTRATORE);
                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec",
                            null, new { managersList = managersList });

                        string fromAddress = (UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email) != null)
                           ? $"{UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email).DisplayName} < {mb.PortalSettings.Email} >"
                           : mb.PortalSettings.Email;

                        foreach (UserInfo userAdmin in managersList)
                        {
                            string adminSubject = Localization.GetSystemMessage(
                                userAdmin.Profile.PreferredLocale,
                                mb.PortalSettings, "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT", user,
                                Localization.GlobalResourceFile, null, string.Empty, mb.PortalSettings.AdministratorId
                            );

                            string adminBody = Localization.GetSystemMessage(
                                userAdmin.Profile.PreferredLocale,
                                mb.PortalSettings, "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY", user, Localization.GlobalResourceFile,
                                null, string.Empty, mb.PortalSettings.AdministratorId
                            );

                            var emailBodyTemplate = LoadTemplate.GetEmailBodyTemplate(mb.PortalSettings, userAdmin.Profile.PreferredLocale);
                            var emailBodyItemTemplate = LoadTemplate.GetEmailBodyItemTemplate(mb.PortalSettings, userAdmin.Profile.PreferredLocale);
                            var emailBodyItemContent = LoadTemplate.GetEmailItemContent(mb.PortalSettings, adminSubject, adminBody, user.UserID, emailBodyItemTemplate);
                            var body = LoadTemplate.GetEmailBody(emailBodyTemplate, emailBodyItemContent, mb.PortalSettings, user);

                            Mail.SendEmail(fromAddress, fromAddress, userAdmin.Email, adminSubject, body);
                        }
                    }
                    else
                    {
                        throw new Exception(ucs.ToString());
                    }
                }
                catch (Exception e)
                {
                    status = false;
                    msg = "Si è verificato un errore durante la creazione dell'utente.<br />" + e.Message;

                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                        "Error", null, new { status = status, msg = msg });
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
               "End", null, new { status = status, msg = msg });

            return Json(new { status = status, msg = msg });
        }
    }
}
