using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Models;
using System.Collections;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Mail;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi
{
    public static class Utility
    {
        public static string Decrypt(string encryptedData, string privateKeyJson = "")
        {
            RsaKeyParameters keyPair;
            string pem = HttpContext.Current.Server.MapPath("~\\DesktopModules\\MVC\\DizionariCondivisiSpidApi\\Certificate\\public.pem");
            
            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start",
                null, new { path = pem, encryptedData = encryptedData }
            );

            using (var reader = System.IO.File.OpenText(pem)) // file containing RSA PKCS1 private key
            {
                keyPair = (RsaKeyParameters)new PemReader(reader).ReadObject();
            }

            Pkcs1Encoding decryptEngine = new Pkcs1Encoding(new RsaEngine());
            decryptEngine.Init(false, keyPair);

            byte[] bytesToDecrypt = Convert.FromBase64String(encryptedData);
            string decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));

            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End",
                null, new { decrypted = decrypted }
            );

            return decrypted;
        }

        public static (int, ArrayList) GetUserByProfileProperty(int portalId, string propertyName, string propertyValue)
        {
            int totalRecords = 0;

            ArrayList usersList = WebUtils.GetUserByProfileProperty(portalId, propertyName, propertyValue, ref totalRecords);

            return (totalRecords, usersList);
        }

        public static UserInfo GetUserByUsername(int portalId, string username)
        {
            return WebUtils.GetUserByUsername(username, portalId);
        }

        public static void CreateUser(ModuleBase mb, NewUserModel userModel)
        {
            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null,
                new { portalId = mb.PortalId, userModel = userModel }
            );

            UserInfo user = new UserInfo();
            user.Username = userModel.CodiceFiscale;
            user.Email = userModel.Email;
            user.FirstName = userModel.Nome;
            user.LastName = userModel.Cognome;
            user.DisplayName = String.Concat(userModel.Cognome.ToUpper(), " ", userModel.Nome.ToUpper());
            user.Membership.Password = "sp!D"+userModel.CodiceFiscale+"it_4";
            user.Membership.UpdatePassword = true;
            user.Membership.Approved = false;
            user.AffiliateID = Null.NullInteger;
            user.PortalID = mb.PortalId;
            user.Profile.InitialiseProfile(mb.PortalId);
            // Basic
            user.Profile.FirstName = userModel.Nome;
            user.Profile.LastName = userModel.Cognome;
            user.Profile.SetProfileProperty(UserProperties.PROPERTY_CODICE_FISCALE, userModel.CodiceFiscale); // Custom property
            // Contact
            user.Profile.Cell = userModel.Telefono;
            user.Profile.Telephone = userModel.Telefono;
            // Location
            user.Profile.SetProfileProperty(UserProperties.PROPERTY_ENTE_APPARTENENZA, userModel.Ente); // Custom property
            // Language
            user.Profile.PreferredLocale = "it-IT";
            // Fuso orario
            user.Profile.PreferredTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            UserCreateStatus ucs = UserController.CreateUser(ref user);

            if (ucs == UserCreateStatus.Success)
            {
                // Send mail to SPID user
                Mail.SendMail(user, MessageType.UserRegistrationPrivate, mb.PortalSettings);

                // Notify all admin...maybe
                IList<UserInfo> managersList = RoleController.Instance.GetUsersByRole(mb.PortalSettings.PortalId, UserRoles.ROLE_AMMINISTRATORE);
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", 
                    null, new { managersList = managersList});
                
                string fromAddress = (UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email) != null)
                   ? $"{UserController.GetUserByEmail(mb.PortalSettings.PortalId, mb.PortalSettings.Email).DisplayName} < {mb.PortalSettings.Email} >"
                   : mb.PortalSettings.Email;

                foreach (UserInfo userAdmin in managersList)
                {
                    string emailSubject = String.Format(Localization.GetSystemMessage(
                        "it-IT",
                        mb.PortalSettings, "EMAIL_SUBJECT_FORMAT", user,
                        Localization.GlobalResourceFile, null, string.Empty, mb.PortalSettings.AdministratorId
                    ), mb.PortalSettings.PortalName);

                    string adminSubject = String.Format(Localization.GetSystemMessage(
                        "it-IT",
                        mb.PortalSettings, "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT", user,
                        Localization.GlobalResourceFile, null, string.Empty, mb.PortalSettings.AdministratorId
                    ), mb.PortalSettings.PortalName);
                    
                    string adminBody = Localization.GetSystemMessage(
                        "it-IT",
                        mb.PortalSettings, "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY", user, Localization.GlobalResourceFile,
                        null, string.Empty, mb.PortalSettings.AdministratorId
                    );

                    var emailBodyTemplate = LoadTemplate.GetEmailBodyTemplate(mb.PortalSettings, "it-IT");
                    var emailBodyItemTemplate = LoadTemplate.GetEmailBodyItemTemplate(mb.PortalSettings, "it-IT");
                    var emailBodyItemContent = LoadTemplate.GetEmailItemContent(mb.PortalSettings, adminSubject, adminBody, mb.PortalSettings.AdministratorId, emailBodyItemTemplate);
                    var body = LoadTemplate.GetEmailBody(emailBodyTemplate, emailBodyItemContent, mb.PortalSettings, user);

                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "ToDo", null,
                        new {
                            user = userAdmin,
                            emailSubject = emailSubject,
                            adminSubject = adminSubject,
                            adminBody = adminBody,
                            emailBodyTemplate = emailBodyTemplate,
                            emailBodyItemTemplate = emailBodyItemTemplate,
                            emailBodyItemContent = emailBodyItemContent,
                            body = body
                        });

                    Mail.SendEmail(fromAddress, fromAddress, userAdmin.Email, emailSubject, body); 
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End");
            }
            else
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error",
                    null, new { msg = ucs.ToString() });
            }
        }
    }
}