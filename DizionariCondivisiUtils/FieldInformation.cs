using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public class FieldInformation
    {
        public string name { get; set; }
        public string type { get; set; }
        public bool nullable { get; set; }
        public bool can_edit { get; set; }
        public string datatables_type { get; set; } //https://datatables.net/reference/type/
        public string editor_type { get; set; } //https://editor.datatables.net/reference/field/
        public string out_formatter { get; set; }
        public string in_formatter { get; set; }
        public string default_value { get; set; }
        public string dimension { get; set; }

        public FieldInformation(string name, string type, bool nullable, string default_value, string dimension)
        {
            this.name = name;
            this.type = type;
            this.nullable = nullable;
            this.default_value = default_value;
            this.dimension = dimension;

            switch (this.type)
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    this.datatables_type = "string";
                    this.editor_type = "text";
                    this.out_formatter = null;
                    this.in_formatter = null;
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "datetimeoffset":
                    this.datatables_type = "datetime";
                    this.editor_type = "datetime";
                    //this.datatables_type = "string";
                    //this.editor_type = "text";
                    this.out_formatter = "dd-MM-yyyy HH:mm:ss";
                    //this.in_formatter = "yyyy-MM-dd HH:mm:ss";
                    this.in_formatter = "dd-MM-yyyy HH:mm:ss";
                    break;
                case "date":
                    this.datatables_type = "date";
                    this.editor_type = "datetime";
                    //this.datatables_type = "string";
                    //this.editor_type = "text";
                    this.out_formatter = "dd-MM-yyyy";
                    //this.in_formatter = "yyyy-MM-dd";
                    this.in_formatter = "dd-MM-yyyy";
                    break;
                case "bigint":
                case "tinyint":
                case "smallint":
                case "int":
                case "real":
                case "money":
                case "smallmoney":
                    this.datatables_type = "num";
                    this.editor_type = "text";
                    this.out_formatter = null;
                    this.in_formatter = null;
                    break;
                case "bit":
                    this.datatables_type = "boolean";
                    this.editor_type = "text";
                    this.out_formatter = null;
                    this.in_formatter = null;
                    break;
            }
        }

        public int GetDimension()
        {
            if (!String.IsNullOrWhiteSpace(this.dimension))
            {
                return Convert.ToInt32(this.dimension.Replace("(", "").Replace(")", ""));
            }

            return -1;
        }

        public bool HasDimension()
        {
            return !(this.GetDimension() == -1);
        }

        public bool HasDefaultValue()
        {
            return !String.IsNullOrWhiteSpace(this.default_value);
        }

        public bool IsTypeDatetime()
        {
            return this.type.Equals("smalldatetime") ||
                this.type.Equals("datetime") ||
                this.type.Equals("datetime2") ||
                this.type.Equals("datetimeoffset");
        }

        public bool IsTypeInteger()
        {
            return this.type.Equals("bigint") ||
                this.type.Equals("tinyint") ||
                this.type.Equals("smallint") ||
                this.type.Equals("int") ||
                this.type.Equals("real") ||
                this.type.Equals("money") ||
                this.type.Equals("smallmoney");
        }

        public bool IsNullable()
        {
            return this.nullable;
        }

        public bool HasCustomFormatter() => !String.IsNullOrEmpty(out_formatter) && !String.IsNullOrEmpty(in_formatter);

        public bool CanEdit()
        {
            return this.can_edit;
        }
    }
}
