using System.Web;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Models;
using System;
using System.Collections;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Authentication;
using System.Web.Mvc;
using DotNetNuke.Entities.Portals;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpidApi.Controllers
{
    public class SpidController : DnnController
    {
        private ILog Log = LoggerSource.Instance.GetLogger(typeof(SpidController));
        private static GraphClient _graphClient;
        private static GraphClient GraphClient
        {
            get
            {
                if (_graphClient == null)
                {
                    ModuleBase mb = new ModuleBase();
                    string clientId = PortalController.GetPortalSetting("Azure_ApiKey", mb.PortalId, "");
                    string clientSecret = PortalController.GetPortalSetting("Azure_ApiSecret", mb.PortalId, "");
                    string tenant = PortalController.GetPortalSetting("Azure_TenantId", mb.PortalId, "");
                    _graphClient = new GraphClient(clientId, clientSecret, tenant);
                }
                return _graphClient;
            }
        }

        [HttpPost]
        public string SpidCallback(SpidRequestModel spidRequest)
        {
            string currentMethod = Extensions.GetCurrentMethod() + "-POST";

            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), currentMethod, "Start", null,
                new { spidRequest = spidRequest }
            );

            if (spidRequest == null)
            {
                if (string.IsNullOrWhiteSpace(spidRequest.Fiscalnumber))
                {
                    DizionariCondivisiLogger.LogActivity(
                        Extensions.GetCurrentModule(), currentMethod, "Error", null,
                        new { msg = "Fiscal number not found" }
                    );

                    return "Fiscal number not found";
                }
            }

            string fiscalCode = Utility.Decrypt(spidRequest.Fiscalnumber);
            fiscalCode = fiscalCode.Substring(fiscalCode.Length - 16, 16);

            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), currentMethod, "Exec", null,
                new { fiscalCode = fiscalCode }
            );
            
            int totalRecords = 0;
            ArrayList users = new ArrayList();
            ModuleBase mb = new ModuleBase();
            string portalName = mb.PortalSettings.PortalName;
            int portalId = mb.PortalId;


            (totalRecords, users) = Utility.GetUserByProfileProperty(portalId, UserProperties.PROPERTY_CODICE_FISCALE, fiscalCode);
            char created = 'n';
            bool existAAD = false;
            if (totalRecords == 0)
            {
                // Create unauth user
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), currentMethod, "Warn", null,
                    new { fiscalCode = fiscalCode, status = "Element NOT found. Create" }
                );

                string resultAad = GraphClient.GetUser($"?$filter=employeeId eq '{fiscalCode}' ");
                Newtonsoft.Json.Linq.JObject valueAad = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(resultAad);
                existAAD = valueAad["value"].HasValues;

                if (!existAAD)
                {
                    string email = Utility.Decrypt(spidRequest.Email);
                    string nome = Utility.Decrypt(spidRequest.Name);
                    string cognome = Utility.Decrypt(spidRequest.Familyname);
                    string telefono = Utility.Decrypt(spidRequest.Mobilephone);

                    NewUserModel user = new NewUserModel()
                    {
                        CodiceFiscale = fiscalCode,
                        Email = email,
                        Nome = nome,
                        Cognome = cognome,
                        Telefono = telefono
                    };

                    Utility.CreateUser(mb, user);
                    created = 'y';
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), currentMethod, "End", null,
                new { 
                    fiscalCode = spidRequest.Fiscalnumber, 
                    created = created,
                    existAAD = existAAD,
                    result = HttpUtility.UrlEncode($"{spidRequest.Fiscalnumber}&created={created}&existAAD={existAAD}") 
                }
            );

            SpidPostReturn spr = new SpidPostReturn() {
                FiscalNumber = spidRequest.Fiscalnumber,
                Created = created,
                ExistAAD = existAAD
            };

            return HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(spr));
        }

        [HttpGet]
        public void SpidCallback(string data)
        {
            ModuleBase mb = new ModuleBase();
            string currentMethod = Extensions.GetCurrentMethod() + "-GET";

            try
            {
                SpidPostReturn spr = Newtonsoft.Json.JsonConvert.DeserializeObject<SpidPostReturn>(data);

                string myData = spr.FiscalNumber;
                char myCreated = spr.Created;
                bool existUserAAD = spr.ExistAAD;

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), currentMethod, "Start", null,
                    new { data = data, myData = myData, existUserAAD = existUserAAD, myCreated = myCreated }
                );

                string fiscalCode = Utility.Decrypt(myData);
                fiscalCode = fiscalCode.Substring(fiscalCode.Length - 16, 16);

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), currentMethod, "Exec", null,
                    new { fiscalCode = fiscalCode }
                );

                int totalRecords = 0;
                int portalId = mb.PortalId;
                string portalName = mb.PortalSettings.PortalName;
                ArrayList users = new ArrayList();

                (totalRecords, users) = Utility.GetUserByProfileProperty(portalId, UserProperties.PROPERTY_CODICE_FISCALE, fiscalCode);

                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), currentMethod, "Exec", null,
                    new { totalRecords = totalRecords, users = Newtonsoft.Json.JsonConvert.SerializeObject(users) }
                );

                if (totalRecords == 0)
                {
                    if (existUserAAD)
                    {
                        DizionariCondivisiLogger.LogActivity(
                           Extensions.GetCurrentModule(), currentMethod, "Error", null,
                           new { data = data, myData = myData, existUserAAD = existUserAAD, myCreated = myCreated, status = "User AAD Exist" }
                       );

                        Response.Redirect("/Errori?custom=exsaaad", false);
                    }
                    else
                    {
                        // No user, not possible
                        DizionariCondivisiLogger.LogActivity(
                            Extensions.GetCurrentModule(), currentMethod, "Error", null,
                            new { fiscalCode = fiscalCode, status = "Element NOT found" }
                        );

                        Response.Redirect("/Errori?custom=nousr", false);
                    }
                }
                else if (totalRecords >= 2)
                {
                    // Too much user with same CF
                    DizionariCondivisiLogger.LogActivity(
                        Extensions.GetCurrentModule(), currentMethod, "Error", null,
                        new { fiscalCode = fiscalCode, status = "Multiple element found." }
                    );

                    Response.Redirect("/Errori?custom=dblusr", false);
                }
                else
                {
                    // Try login
                    string ip = HttpContext.Request.UserHostAddress;

                    UserInfo ui = (UserInfo)users[0];

                    if (ui.Membership.Approved)
                    {
                        // User blocked? Unblock him
                        if (ui.Membership.LockedOut)
                        {
                            DizionariCondivisiLogger.LogActivity(
                                Extensions.GetCurrentModule(), currentMethod, "Warn", null,
                                new { status = "Unblock user", user = ui }
                            );
                            UserController.UnLockUser(ui);
                        }

                        UserController.UserLogin(portalId, ui, portalName, ip, true);

                        //Set the Authentication Type used 
                        AuthenticationController.SetAuthenticationType("SPID");

                        DizionariCondivisiLogger.LogActivity(
                            Extensions.GetCurrentModule(), currentMethod, "End", null,
                            new { fiscalCode = fiscalCode, status = "LoggedIn" }
                        );

                        if (
                            (String.IsNullOrWhiteSpace(ui.Profile.Telephone) ||
                            String.IsNullOrWhiteSpace(ui.Profile.Street)) &&
                            !ui.Username.ToLower().Contains("azure-") &&
                            ui.Username.Length == 16
                        )
                        {
                            DizionariCondivisiLogger.LogActivity(
                                Extensions.GetCurrentModule(), currentMethod, "End", null,
                                new { dest = "/Profilo-SPID" }
                            );

                            Response.Redirect("/Profilo-SPID", false);
                        }
                        else
                        {
                            DizionariCondivisiLogger.LogActivity(
                                Extensions.GetCurrentModule(), currentMethod, "End", null,
                                new { dest = "/" }
                            );

                            Response.Redirect("/", false);
                        }
                    }
                    else
                    {
                        if (myCreated.Equals('y'))
                        {
                            DizionariCondivisiLogger.LogActivity(
                                Extensions.GetCurrentModule(), currentMethod, "Warn", null,
                                new { status = "User created", user = ui }
                            );

                            Response.Redirect("/Errori?custom=crt", false);
                        }
                        else
                        {
                            DizionariCondivisiLogger.LogActivity(
                                Extensions.GetCurrentModule(), currentMethod, "Warn", null,
                                new { status = "Unauth user", user = ui }
                            );

                            Response.Redirect("/Errori?custom=unathusr", false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(
                    Extensions.GetCurrentModule(), currentMethod, "Error", null,
                    new { msg = e.Message, exc = e }
                );

                Response.Redirect("/Errori", false);
            }
        }
    }
}
