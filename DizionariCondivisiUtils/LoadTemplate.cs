using System;
using DotNetNuke.Common;
using System.Web.Caching;
using System.Globalization;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using System.Text.RegularExpressions;
using DotNetNuke.Services.Localization;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public class LoadTemplate
    {

        public static string GetEmailBodyTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_BODY", Localization.GlobalResourceFile, portalSettings, language);
        }

        public static string GetEmailBody(string template, string messageBody, PortalSettings portalSettings, UserInfo recipientUser)
        {
            template = template.Replace("[MESSAGEBODY]", messageBody); // moved to top since that we we can replace tokens in there too...
            template = template.Replace("[RECIPIENTUSERID]", recipientUser.UserID.ToString(CultureInfo.InvariantCulture));
            template = template.Replace("[RECIPIENTDISPLAYNAME]", recipientUser.DisplayName);
            template = template.Replace("[RECIPIENTEMAIL]", recipientUser.Email);
            template = template.Replace("[SITEURL]", GetPortalHomeUrl(portalSettings));
            template = template.Replace("[NOTIFICATIONURL]", GetNotificationUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[PORTALNAME]", portalSettings.PortalName);
            template = template.Replace("[LOGOURL]", GetPortalLogoUrl(portalSettings));
            template = template.Replace("[UNSUBSCRIBEURL]", GetSubscriptionsUrl(portalSettings, recipientUser.UserID));
            template = template.Replace("[YEAR]", DateTime.Now.Year.ToString());
            template = ResolveUrl(portalSettings, template);

            return template;
        }

        public static string GetEmailBodyItemTemplate(PortalSettings portalSettings, string language)
        {
            return Localization.GetString("EMAIL_MESSAGING_DISPATCH_ITEM", Localization.GlobalResourceFile, portalSettings, language);
        }

        public static string GetEmailItemContent(PortalSettings portalSettings, string Subject, string Body, int authorId, 
            string itemTemplate)
        {
            var emailItemContent = itemTemplate;
            emailItemContent = emailItemContent.Replace("[TITLE]", Subject);
            emailItemContent = emailItemContent.Replace("[CONTENT]", HtmlUtils.ConvertToHtml(Body));
            emailItemContent = emailItemContent.Replace("[PROFILEPICURL]", GetProfilePicUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[PROFILEURL]", GetProfileUrl(portalSettings, authorId));
            emailItemContent = emailItemContent.Replace("[DISPLAYNAME]", GetDisplayName(portalSettings, authorId));

            emailItemContent = emailItemContent.Replace("[FOLLOWREQUESTACTIONS]", string.Empty);
            emailItemContent = emailItemContent.Replace("[FRIENDREQUESTACTIONS]", string.Empty);

            return emailItemContent;
        }

        private static int GetMessageTab(PortalSettings sendingPortal)
        {
            var cacheKey = string.Format("MessageTab:{0}", sendingPortal.PortalId);

            var cacheItemArgs = new CacheItemArgs(cacheKey, 30, CacheItemPriority.Default, sendingPortal);

            return CBO.GetCachedObject<int>(cacheItemArgs, GetMessageTabCallback);
        }

        private static object GetMessageTabCallback(CacheItemArgs cacheItemArgs)
        {
            var portalSettings = cacheItemArgs.Params[0] as PortalSettings;

            var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                foreach (var tab in childTabs)
                {
                    foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
            return portalSettings.UserTabId;
        }

        private static string ResolveUrl(PortalSettings portalSettings, string template)
        {
            const string linkRegex = "(href|src)=\"(/[^\"]*?)\"";
            var matches = Regex.Matches(template, linkRegex, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var link = match.Groups[2].Value;
                var defaultAlias = portalSettings.DefaultPortalAlias;
                var domain = Globals.AddHTTP(defaultAlias);
                if (defaultAlias.Contains("/"))
                {
                    var subDomain =
                        defaultAlias.Substring(defaultAlias.IndexOf("/", StringComparison.InvariantCultureIgnoreCase));
                    if (link.StartsWith(subDomain, StringComparison.InvariantCultureIgnoreCase))
                    {
                        link = link.Substring(subDomain.Length);
                    }
                }

                template = template.Replace(match.Value, $"{match.Groups[1].Value}=\"{domain}{link}\"");
            }

            return template;
        }

        private static string GetPortalHomeUrl(PortalSettings portalSettings)
        {
            return string.Format("http://{0}", portalSettings.DefaultPortalAlias);
        }

        private static string GetPortalLogoUrl(PortalSettings portalSettings)
        {
            return string.Format(
                "http://{0}/{1}/{2}",
                GetDomainName(portalSettings),
                portalSettings.HomeDirectory,
                portalSettings.LogoFile);
        }

        private static string GetDomainName(PortalSettings portalSettings)
        {
            var portalAlias = portalSettings.DefaultPortalAlias;
            return portalAlias.IndexOf("/", StringComparison.InvariantCulture) != -1 ?
                       portalAlias.Substring(0, portalAlias.IndexOf("/", StringComparison.InvariantCulture)) :
                       portalAlias;
        }

        private static string GetSubscriptionsUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/tabid/{1}/ctl/Profile/userId/{2}/pageno/3",
                portalSettings.DefaultPortalAlias,
                GetMessageTab(portalSettings),
                userId);
        }

        private static string GetNotificationUrl(PortalSettings portalSettings, int userId)
        {
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", portalSettings.PortalId, portalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
            if (messageTabId <= 0)
            {
                messageTabId = portalSettings.UserTabId;
                var profileTab = TabController.Instance.GetTab(portalSettings.UserTabId, portalSettings.PortalId, false);
                if (profileTab != null)
                {
                    var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
                    foreach (var tab in childTabs)
                    {
                        foreach (var kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                        {
                            var module = kvp.Value;
                            if (module.DesktopModule.FriendlyName == "Message Center")
                            {
                                messageTabId = tab.TabID;
                                break;
                            }
                        }
                    }
                }

                DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));
            }

            return string.Format(
                "http://{0}/tabid/{1}/userId/{2}/{3}#dnnCoreNotification",
                portalSettings.DefaultPortalAlias,
                messageTabId,
                userId,
                Globals.glbDefaultPage);
        }

        private static string GetProfilePicUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/DnnImageHandler.ashx?mode=profilepic&userId={1}&h={2}&w={3}",
                portalSettings.DefaultPortalAlias,
                userId,
                64,
                64);
        }

        private static string GetDisplayName(PortalSettings portalSettings, int userId)
        {
            return (UserController.GetUserById(portalSettings.PortalId, userId) != null)
                       ? UserController.GetUserById(portalSettings.PortalId, userId).DisplayName
                       : portalSettings.PortalName;
        }

        private static string GetProfileUrl(PortalSettings portalSettings, int userId)
        {
            return string.Format(
                "http://{0}/tabid/{1}/userId/{2}/{3}",
                portalSettings.DefaultPortalAlias,
                portalSettings.UserTabId,
                userId,
                Globals.glbDefaultPage);
        }
    }
}
