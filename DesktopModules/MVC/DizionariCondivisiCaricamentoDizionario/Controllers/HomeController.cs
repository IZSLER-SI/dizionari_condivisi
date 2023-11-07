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
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.Data.SqlClient;
using System.Collections.Generic;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCaricamentoDizionario.Models;
using CsvHelper;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging;
using CsvHelper.Configuration;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiCaricamentoDizionario.Controllers
{
    [DnnHandleError]
    public class HomeController : DnnController
    {
        private string CsvFileSeparator = ";";

        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDizionari()
        {
            List<DizionarioModel> listaDizionario = new List<DizionarioModel>();
            string query = @"
                SELECT ed.dizionario, ed.descrizione
                FROM elenco_dizionari ed
                WHERE ed.record_attivo = 1
                ORDER BY ed.Dizionario
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string dizionario = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            string descrizioneDizionario = reader.IsDBNull(1) ? "" : reader.GetString(1);

                            listaDizionario.Add(new DizionarioModel() { Dizionario = dizionario, Descrizione = descrizioneDizionario });
                        }
                    }
                }
            }

            return Json(listaDizionario, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadStruttura(string nome_dizionario)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null, new { dizionario = nome_dizionario });

            List<string> listaColonne = new List<string>();
            listaColonne.Add("Colonna;Datatype");

            string query = @"
                SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @Dizionario
                    AND COLUMN_NAME NOT IN ('id', 'chiave', 'chiave_composta', 'last_update', 'contatore')
                ORDER BY ORDINAL_POSITION
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Dizionario", nome_dizionario);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string columnName = reader.GetString(0);
                            string dataType = reader.GetString(1);
                            int? length = null;
                            if (!reader.IsDBNull(2))
                            {
                                length = reader.GetInt32(2);
                                dataType += $"({(length == -1 ? "MAX" : length.ToString())})";
                            }

                            listaColonne.Add($"{columnName};{dataType}");
                        }
                    }
                }
            }

            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End", null, new { dizionario = nome_dizionario, result = String.Join("\r\n", listaColonne) });

            return Json(new { structure = String.Join("\r\n", listaColonne) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadCsv(string dizionario)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null, new { dizionario = dizionario });

            if (Request.Files.Count > 0)
            {
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    string filename = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}-{file.FileName}";
                    string fileExtension = filename.Substring(filename.Length - 3);

                    DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                        new { dizionario = dizionario, filename = filename });

                    if (fileExtension.Equals("csv", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = Path.Combine(Server.MapPath("~/DesktopModules/MVC/DizionariCondivisiCaricamentoDizionario/TemporaryUploads"), filename);
                        file.SaveAs(path);

                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                            new { dizionario = dizionario, status = "File temporary saved" });

                        if (ValidateCsv(path, dizionario))
                        {
                            try
                            {
                                System.IO.File.Move(path, path.Replace("TemporaryUploads", "Uploads"));
                                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                                    new { dizionario = dizionario, status = "File saved" });

                                path = path.Replace("TemporaryUploads", "Uploads");
                                List<string> headerRows = new List<string>();
                                using (StreamReader streamReader = new StreamReader(path))
                                {
                                    using (CsvReader csv = new CsvReader(streamReader, GetCsvConfiguration()))
                                    {
                                        if (csv.Read())
                                        {
                                            if (csv.ReadHeader())
                                            {
                                                headerRows = csv.HeaderRecord.ToList();
                                                Dictionary<string, Dictionary<string, string>> queries = new Dictionary<string, Dictionary<string, string>>();
                                                string query = $"INSERT INTO {dizionario} ({String.Join(", ", headerRows)}) VALUES ";

                                                dynamic records = csv.GetRecords<dynamic>();
                                                int k = 0;
                                                foreach (dynamic record in records)
                                                {
                                                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                                                    string fields = "(";
                                                    foreach (KeyValuePair<string, object> item in record)
                                                    {
                                                        if (fields.Length > 1)
                                                        {
                                                            fields += ", ";
                                                        }
                                                        string key = $"@{item.Key}_{k}";
                                                        fields += key;

                                                        queryParams.Add(key, item.Value == null ? null : item.Value.ToString());
                                                    }
                                                    fields += ");";
                                                    k++;

                                                    queries.Add(String.Concat(query, fields), queryParams);
                                                }

                                                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                                                {
                                                    connection.Open();

                                                    SqlTransaction transaction = connection.BeginTransaction();

                                                    try
                                                    {
                                                        foreach (KeyValuePair<string, Dictionary<string, string>> obj in queries)
                                                        {
                                                            using (SqlCommand command = new SqlCommand(obj.Key, connection, transaction))
                                                            {
                                                                foreach (KeyValuePair<string, string> item in obj.Value)
                                                                {
                                                                    if (item.Value == null || String.IsNullOrWhiteSpace(item.Value))
                                                                    {
                                                                        command.Parameters.AddWithValue(item.Key, DBNull.Value);
                                                                    }
                                                                    else
                                                                    {
                                                                        command.Parameters.AddWithValue(item.Key, item.Value);
                                                                    }
                                                                }

                                                                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                                                                    new { dizionario = dizionario, query = obj.Key, parameters = obj.Value });

                                                                command.ExecuteNonQuery();
                                                            }
                                                        }

                                                        query = $@"UPDATE elenco_dizionari
                                                            SET num_record = (SELECT COUNT(*) FROM {dizionario}),
	                                                            data_ultima_modifica = CURRENT_TIMESTAMP
                                                            WHERE dizionario = @Dizionario
                                                        ";

                                                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                                                        {
                                                            command.Parameters.AddWithValue("@Dizionario", dizionario);
                                                            command.ExecuteNonQuery();
                                                        }

                                                        transaction.Commit();
                                                    }
                                                    catch (SqlException e)
                                                    {
                                                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                                            new { dizionario = dizionario, status = "Rollback", msg = e.Message, exc = e });

                                                        transaction.Rollback();

                                                        throw ((SqlException)e);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                                            new { dizionario = dizionario, status = "Rollback", msg = e.Message, exc = e });

                                                        transaction.Rollback();

                                                        throw e;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (BadDataException bd)
                            {
                                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                    new { dizionario = dizionario, msg = bd.Message, exc = bd });

                                return Json(new { status = false, msg = $"Si è verificato un errore: il file caricato non è formattato correttamente" }, JsonRequestBehavior.AllowGet);
                            }
                            catch (SqlException sqexc)
                            {
                                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                    new { dizionario = dizionario, msg = sqexc.Message, exc = sqexc });

                                return Json(new { status = false, msg = $"Si è verificato un errore: il file caricato non è formattato correttamente" }, JsonRequestBehavior.AllowGet);
                            }
                            catch (Exception e)
                            {
                                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                    new { dizionario = dizionario, msg = e.Message, exc = e });

                                return Json(new { status = false, msg = $"Si è verificato un errore: verificare il file caricato" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                                new { dizionario = dizionario, status = "Can't validate CSV" });

                            return Json(new { status = false, msg = "Il nome delle colonne caricate non coincide con la struttura in database" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                            new { dizionario = dizionario, status = "File isn't CSV" });

                        return Json(new { status = false, msg = "Estensione non valida: caricare un file CSV" }, JsonRequestBehavior.AllowGet);
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "End");

                return Json(new { status = true, msg = "File caricato correttamente!" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                    new { dizionario = dizionario, status = "No file" });

                return Json(new { status = false, msg = "Nessun file selezionato" }, JsonRequestBehavior.AllowGet);
            }

        }

        private CsvConfiguration GetCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = CsvFileSeparator
            };
        }

        private bool ValidateCsv(string path, string dizionario)
        {
            DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Start", null,
                new { path = path, dizionario = dizionario });

            try
            {
                string query = @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @Dizionario
                        AND COLUMN_NAME NOT IN ('id', 'chiave', 'chiave_composta', 'last_update', 'contatore')
                    ORDER BY ORDINAL_POSITION
                ";

                List<string> columnNames = new List<string>();
                using (SqlConnection connection = DbExtensions.GetSqlConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Dizionario", dizionario);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                columnNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                    new { dizionario = dizionario, columnNames = String.Join(CsvFileSeparator, columnNames) });

                List<string> headerRows = new List<string>();

                using (StreamReader reader = new StreamReader(path))
                {
                    // The culture is used to determine the default delimiter, default line ending, and formatting when type converting. 
                    using (CsvReader csv = new CsvReader(reader, GetCsvConfiguration()))
                    {
                        if (csv.Read())
                        {
                            if (csv.ReadHeader())
                            {
                                headerRows = csv.HeaderRecord.ToList();
                            }
                        }
                    }
                }

                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Exec", null,
                    new { dizionario = dizionario, headerRows = String.Join(CsvFileSeparator, headerRows) });

                return columnNames.SequenceEqual(headerRows);
            }
            catch (BadDataException bd)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                    new { dizionario = dizionario, msg = bd.Message, exc = bd });

                return false;
            }
            catch (SqlException sqexc)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                    new { dizionario = dizionario, msg = sqexc.Message, exc = sqexc });

                return false;
            }
            catch (Exception e)
            {
                DizionariCondivisiLogger.LogActivity(Extensions.GetCurrentModule(), Extensions.GetCurrentMethod(), "Error", null,
                    new { dizionario = dizionario, msg = e.Message, exc = e });

                return false;
            }
        }
    }
}
