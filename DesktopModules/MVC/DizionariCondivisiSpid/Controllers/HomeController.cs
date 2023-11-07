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

using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpid.Models;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Http;
using System.Web;
using System.Net.Http;
using System.Collections;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Authentication;
using System.Net.Mime;
using DotNetNuke.Entities.Tabs;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiSpid.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            Item item = new Item();

            item.Url = ModuleContext.Settings["DizionariCondivisiSpid_Url"].ToString();
            item.Servizio = ModuleContext.Settings["DizionariCondivisiSpid_Servizio"].ToString();
            
            DizionariCondivisiLogger.LogActivity(
                Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null,
                new { item = item }
            );

            return View(item);
        }

        public ActionResult Home()
        {
            return View();
        }
    }
}
