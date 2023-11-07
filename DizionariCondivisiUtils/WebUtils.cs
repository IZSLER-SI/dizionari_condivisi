using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Messaging;
using System;
using System.Collections;
using System.Linq;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public class WebUtils
    {

        /**
         * Controlla se l'utente corrente ha il ruolo richiesto
         **/
        public static bool UserHasRole(string role)
        {
            UserInfo user = GetCurrentUser();
            if (user == null || user.UserID == -1)
                return false;

            bool ret = user.Roles.Contains(role);
            return ret;

        }

        /**
         * Controlla se l'utente corrente è SuperUser
         **/
        public static bool UserIsSuperUser()
        {
            UserInfo user = GetCurrentUser();
            if (user == null || user.UserID == -1)
                return false;

            bool ret = user.IsSuperUser;
            return ret;
        }

        /**
         * Ritorna l'utente corrente
         **/
        public static UserInfo GetCurrentUser()
        {
            UserInfo user = UserController.Instance.GetCurrentUserInfo();
            return user;
        }

        /**
         * Ritorna l'utente corrente
         **/
        public static string GetCurrentUsername()
        {
            UserInfo user = GetCurrentUser();
            return user == null ? null : user.Username;
        }

        public static string GetCurrentDisplayName()
        {
            UserInfo user = GetCurrentUser();
            return user == null ? null : user.DisplayName;
        }
        public static string GetCurrentNomeCognome()
        {
            UserInfo user = GetCurrentUser();
            return user == null ? null : $"{user.FirstName} {user.LastName}";
        }

        public static string[] GetRoleList()
        {
            UserInfo user = GetCurrentUser();
            return user.Roles;
        }
        
        public static DateTime GetLastLoginDate()
        {
            UserInfo current_user = GetCurrentUser();
            return current_user.Membership.LastLoginDate;
        }

        public static UserInfo GetUserByUsername(string username)
        {
            UserInfo user = UserController.GetUserByName(username);
            return user;
        }

        public static UserInfo GetUserByUsername(string username, int portalId = -1)
        {
            UserInfo user = GetUserByUsername(username);
            if (portalId != -1)
            {
                user = UserController.GetUserByName(portalId, username);
            }
            
            return user;
        }

        public static string GetUserEmailByUsername(string username)
        {
            UserInfo ui = GetUserByUsername(username);
            return ui.Email;
        }

        public static UserInfo GetUserById(int portalId, int userId)
        {
            return UserController.GetUserById(portalId, userId);
        }

        public static RoleInfo GetRoleByName(int portalId, string roleName) 
        {
            RoleController rc = new RoleController();
            return rc.GetRoleByName(portalId, roleName);
        }

        public static void AddUserRole(int portalId, int userId, int roleId, DateTime? expireDate = null)
        {
            RoleController rc = new RoleController();
            rc.AddUserRole(portalId, userId, roleId, DateTime.Now, expireDate.HasValue ? expireDate.Value : DateTime.MaxValue);
        }

        public static void UpdateUserRole(int portalId, int userId, int roleId, RoleStatus roleStatus, bool isOwner, bool cancel)
        {
            RoleController rc = new RoleController();
            rc.UpdateUserRole(portalId, userId, roleId, roleStatus, isOwner, cancel);
        }

        private static Frequency GetFrequency(int frequency)
        {
            switch (frequency)
            {
                case 1: return Frequency.Daily;
                case 2: return Frequency.Hourly;
                case 3: return Frequency.Weekly;
                case 4: return Frequency.Monthly;
                case -1: return Frequency.Never;
                default: return Frequency.Instant;
            }
        }

        /// <summary>
        /// Consente di cambiare le impostazioni di notifica e dei messaggi
        /// <param name="portalId"></param>
        /// <param name="userId"></param>
        /// <param name="messageEmailFrequency"></param>
        /// <param name="notificationEmailFrequency">
        /// -1: mai
        /// 0: istantanea
        /// 1: giornaliera
        /// 2: ogni ora
        /// 3: settimanale
        /// 4: mensile
        /// </param>
        /// </summary>
        public static void InsertUpdateNotifyDigest(int portalId, int userId, int messageEmailFrequency = 0, int notificationEmailFrequency = 0)
        {
            UserPreference up = new UserPreference();
            up.PortalId = portalId;
            up.UserId = userId;
            up.MessagesEmailFrequency = GetFrequency(messageEmailFrequency);
            up.NotificationsEmailFrequency = GetFrequency(notificationEmailFrequency);

            UserPreferencesController upc = new UserPreferencesController();
            upc.SetUserPreference(up);
        }

        public static ArrayList GetUserByProfileProperty(int portalId, string propertyName, string propertyValue, ref int output, int pageIndex = -1, int pageSize = -1, bool includeUnauth = false)
        {
            return UserController.GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize, ref output);
        }

        public static ArrayList GetUsers(int portalId, bool includeDeleted = false, bool superUserOnly = false)
        {
            return UserController.GetUsers(includeDeleted, superUserOnly, portalId);
        }

        public static ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted = false, bool superUserOnly = false)
        {
            return UserController.GetUnAuthorizedUsers(portalId, includeDeleted, superUserOnly);
        }
    }
}
