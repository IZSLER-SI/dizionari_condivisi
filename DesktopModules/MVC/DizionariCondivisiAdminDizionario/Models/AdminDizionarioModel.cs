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
using System.Collections.Generic;
using System.Web.Caching;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Content;
using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Models
{
    public class AdminDizionarioModel
    {
        public int Step { get; private set; }
        public string Dizionario { get; private set; }
        public string Chiave { get; private set; }
        public string Descrizione { get; private set; }
        public int DefaultField { get; private set; }
        public int CustomField { get; private set; }
        public int TotaleField { get; private set; }
        public Dictionary<string, string> ListDatatypeField { get; private set; }
        public string AdminAction { get; private set; }
        public List<FieldInformation> Fields { get; private set; }

        public AdminDizionarioModel(int step, string dizionario, string chiave,
            int numero_field, string descrizione, string adminAction)
        {
            this.Step = step;
            this.Dizionario = dizionario;
            this.Chiave = chiave;
            this.Descrizione = descrizione;
            this.DefaultField = 7;
            this.CustomField = numero_field;
            this.TotaleField = this.DefaultField + this.CustomField;
            this.ListDatatypeField = DbExtensions.GetTableAsDictionary(
                "libreria_datatype", "datatype", "CAST(show_dimensione AS NCHAR(1))", "datatype",
                new Dictionary<string, string> { { "record_attivo", "1" } }, null
            );
            this.AdminAction = adminAction;
        }
        public AdminDizionarioModel(int step, string dizionario, string chiave,
            int numero_field, string descrizione, string adminAction, List<FieldInformation> fields)
        {
            this.Step = step;
            this.Dizionario = dizionario;
            this.Chiave = chiave;
            this.Descrizione = descrizione;
            this.DefaultField = 7;
            this.CustomField = numero_field;
            this.TotaleField = this.DefaultField + this.CustomField;
            this.ListDatatypeField = DbExtensions.GetTableAsDictionary(
                "libreria_datatype", "datatype", "CAST(show_dimensione AS NCHAR(1))", "datatype",
                new Dictionary<string, string> { { "record_attivo", "1" } }, null
            );
            this.AdminAction = adminAction;
            this.Fields = fields;
        }

        public AdminDizionarioModel(int step, string dizionario, string adminAction)
        {
            this.Step = step;
            this.Dizionario = dizionario;
            this.AdminAction = adminAction;
        }

        public void SetStep(int step)
        {
            this.Step = step;
        }

        public bool IsCreateTable()
        {
            return this.AdminAction.Equals("create");
        }

        public string DefinePageTitle()
        {
            return IsCreateTable() ? "Creazione" : "Modifica";
        }

        public string DefineButtonStep1()
        {
            return IsCreateTable() ? "Crea" : "Modifica";
        }
    }
}
