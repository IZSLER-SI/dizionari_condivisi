using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
	public abstract class ARowDizionarioModel
    {
		private string campiModificabili;
		public string table;
        public List<FieldInformation> fields;

		public ARowDizionarioModel(string table)
		{
			this.table = table;
			this.fields = GetSchema();
		}

		public ARowDizionarioModel(string table, string campiModificabili)
		{
			this.campiModificabili = campiModificabili;

			this.table = table;
			this.fields = GetSchema();
		}

		public void AddFields(List<FieldInformation> fields)
		{
			this.fields = fields;
		}

		public List<FieldInformation> GetSchema()
		{
			List<FieldInformation> result = new List<FieldInformation>();

			string query = @"
				SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, SUBSTRING(COLUMN_DEFAULT, 2, LEN(COLUMN_DEFAULT) - 2), CHARACTER_MAXIMUM_LENGTH
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = @Table
				ORDER BY ORDINAL_POSITION
			";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Table", this.table);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
							string row_field_name = reader.GetString(0);
							string row_field_type = reader.GetString(1);
							bool row_is_nullable = reader.GetString(2).Equals("YES") ? true : false;
							string row_default_value = reader.IsDBNull(3) ? null : reader.GetString(3);
							string row_dimension = null;
							if (!reader.IsDBNull(4))
							{
								row_dimension = "("+reader.GetInt32(4)+")";
							}

							FieldInformation fi = new FieldInformation(row_field_name, row_field_type,
								row_is_nullable, row_default_value, row_dimension);
							fi.can_edit = !String.IsNullOrWhiteSpace(this.campiModificabili) 
								? this.campiModificabili.Contains(row_field_name)
								: false;

							result.Add(fi);
						}
                    }
                }
            }

			return result;
        }

		public abstract IHttpActionResult Create();
		public abstract IHttpActionResult Edit();
		public abstract IHttpActionResult Remove();
		public abstract IHttpActionResult Get();
	}
}
