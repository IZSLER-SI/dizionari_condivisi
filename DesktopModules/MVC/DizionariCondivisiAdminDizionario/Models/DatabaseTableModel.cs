using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiAdminDizionario.Models
{
    public class DatabaseTableModel
    {
        public static string PatternCampo = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        public static string PatternCampoJs = "/" + PatternCampo + "/";

        public string Campo { get; set; }
        public string Tipo { get; set; }
        public int? Dimensione { get; set; }
        public char Obbligatorio { get; set; }
        public char Modificabile { get; set; }
        public string DefaultValue { get; set; }
        public bool NewField { get; set; }

        public bool IsPrimaryKey()
        {
            return this.Campo.Equals("id");
        }

        public bool IsNullable()
        {
            return this.Obbligatorio.Equals('N');
        }

        public bool HasDefaultValue()
        {
            return !String.IsNullOrWhiteSpace(this.DefaultValue) && !this.DefaultValue.Equals("ND");
        }

        public string GetDefaultConstraintName(string dizionario)
        {
            return $"DF_{dizionario}_{this.Campo}";
        }

        public string GetDefaultValue()
        {
            if (this.Tipo.Equals("nchar") || this.Tipo.Equals("char") ||
                this.Tipo.Contains("nvarchar") || this.Tipo.Contains("varchar") || 
                (
                    this.Tipo.Contains("date") &&
                    !this.DefaultValue.Equals("CURRENT_TIMESTAMP", StringComparison.CurrentCultureIgnoreCase) &&
                    !this.DefaultValue.Equals("getdate()", StringComparison.CurrentCultureIgnoreCase)
                )
            )
            {
                return String.Concat("'", this.DefaultValue, "'");                
            }

            return this.DefaultValue;
        }

        public string GetAlterOperation()
        {
            return this.IsNewField() ? " ADD " : " ALTER COLUMN ";
        }

        public bool IsNewField()
        {
            return this.NewField;
        }

        public bool IsModificabile() => this.Modificabile.Equals('S');
    }
}