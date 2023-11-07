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

using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using System.Web;
using DotNetNuke.Entities.Tabs;
using System;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiClearCookie.Models
{
    public class Home
    {
        public static bool IsUnauth()
        {
            string cookiesName = "AzureUserToken";
            var authCookies = HttpContext.Current.Request.Cookies.AllKeys;

            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start", null, new { cookiesName = cookiesName, cookies = authCookies }
            );

            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Url", null, new { url = TabController.CurrentPage.FullUrl }
            );

            foreach (var authCookie in authCookies)
            {
                if (authCookie.Equals(cookiesName))
                {
                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "NAuth");

                    return true;
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Auth");

            return false;
        }
    }
}
