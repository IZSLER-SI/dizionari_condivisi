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
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Instrumentation;
using System.Web;
using System.Web.Mvc;
using DataTables;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneDizionario.Models;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiGestioneDizionario.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        public ActionResult Index()
        {
            string tbl = this.Request.Params["tbl"];
            
            GestioneDizionarioModel model = new GestioneDizionarioModel(tbl, String.IsNullOrWhiteSpace(tbl));

            return View(model);
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetTableStructure(string table)
        {
            GestioneDizionarioModel page_model = new GestioneDizionarioModel(table, String.IsNullOrWhiteSpace(table));

            return Json(new { response = page_model.dizionarioModel.fields });
        }

        public ActionResult List(string table)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "Start", table, HttpUtility.UrlDecode(request.Form.ToString()));

            var result = DoAction(table, request);
            
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "End", table, result);

            return result;
        }

        public ActionResult Create(string table)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start", table, HttpUtility.UrlDecode(request.Form.ToString()));

            var result = DoAction(table, request, "insert");

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "End", table, result);

            return result;
        }

        public ActionResult Edit(string table)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(),
                "Start", table, HttpUtility.UrlDecode(request.Form.ToString()));

            var result = DoAction(table, request, "edit");

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "End", table, result);

            return result;
        }

        public ActionResult Remove(string table)
        {
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "Start", table, request.Form);

            var result = DoAction(table, request, "remove");

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), 
                "End", table, result);

            return result;
        }

        private ActionResult DoAction(string table, System.Web.HttpRequest request, string action = null)
        {
            //https://editor.datatables.net/docs/1.9.4/net/api/index.html
            bool action_defined = !string.IsNullOrWhiteSpace(action);
            GestioneDizionarioModel page_model = new GestioneDizionarioModel(table, String.IsNullOrWhiteSpace(table));
            DtResponse response = null;

            using (Database db = DbExtensions.GetDatabaseConnection())
            {
                Editor editor = new Editor(db, table, "id"); //Specify the field key

                foreach (FieldInformation field in page_model.dizionarioModel.fields)
                {
                    Field f = new Field(field.name);

                    if (field.name.Equals("id") || field.name.Equals("chiave") || field.name.Equals("contatore")) {
                        f.Set(false);
                    }
                    
                    if (field.HasCustomFormatter())
                    {
                        f.GetFormatter(Format.DateSqlToFormat(field.out_formatter)); //Format for client-side from server data
                        if (field.name.Equals("last_update"))
                        {
                            f.Set(false);
                        }
                        else
                        {
                            f.SetFormatter(Format.DateFormatToSql(field.in_formatter));  //Format for server-side from client data
                        }
                    }
                    
                    if (!field.nullable)
                    {
                        // Necessario perché last_update NON è nella form
                        if (!field.name.Equals("last_update") && !field.name.Equals("id") 
                            && !field.name.Equals("chiave_composta") && !field.name.Equals("chiave"))
                        {
                            f.Validator(Validation.Required(
                                new ValidationOpts { Message = field.name + ": obbligatorio" }
                            ));
                        }
                    }
                    
                    if (field.IsTypeInteger())
                    {
                        if (!field.name.Equals("id"))
                        {
                            f.SetFormatter(Format.IfEmpty(null));
                            f.Validator(Validation.Numeric(
                                "it-IT",
                                new ValidationOpts { Message = field.name + ": valore NON numerico" }
                            ));
                        }
                    }

                    if (field.type.Equals("char") || field.type.Equals("nchar") || field.type.Equals("varchar") || field.type.Equals("nvarchar"))
                    {
                        int maxlen = field.GetDimension();
                        f.Validator(Validation.MaxLen(maxlen, new ValidationOpts { Message = field.name + ": lunghezza massima " + maxlen }));
                    }

                    if (field.HasDefaultValue())
                    {
                        if (field.IsTypeDatetime())
                        {
                            f.SetFormatter(Format.IfEmpty(field.default_value));
                        }
                        else
                        {
                            string t = field.default_value;
                            t = t.Remove(0, 1);
                            t = t.Remove(t.Length - 1);

                            f.SetFormatter(Format.IfEmpty(t));
                        }
                    }

                    editor.Field(f);
                }

                if (action_defined && action.Equals("remove"))
                {
                    // Disallow delete just in case anyone uses the API to do it
                    editor.PreRemove += (sender, args) => args.Cancel = true;
                }

                editor.Debug(true);
                editor.Process(request);
                response = editor.Data();
            }

            if (action_defined)
            {
                DizionariCondivisiLogger.LogUpdateStatisticheDizionario(table, DbExtensions.GetTableRowsCount(table));
            }

            return Json(response);
        }
    }
}