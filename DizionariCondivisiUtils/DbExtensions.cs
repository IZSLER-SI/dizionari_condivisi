using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using DataTables;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public static class DbExtensions
    {
        public static SqlConnection GetSqlConnection()
        {
            string conn_string;

            if (ConfigurationManager.ConnectionStrings["dizionari_condivisi"] != null)
                conn_string = ConfigurationManager.ConnectionStrings["dizionari_condivisi"].ConnectionString;
            else
                throw new Exception("Cannot find connection string for dizionari_condivisi in web.config");

            SqlConnection connection = new SqlConnection(conn_string);

            return connection;
        }
        public static SqlConnection GetDnnConnection()
        {
            string conn_string;

            if (ConfigurationManager.ConnectionStrings["SiteSqlServer"] != null)
                conn_string = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;
            else
                throw new Exception("Cannot find connection string for dizionari_condivisi_dnn in web.config");

            SqlConnection connection = new SqlConnection(conn_string);

            return connection;
        }

        public static Database GetDatabaseConnection()
        {
            string conn_string;

            if (ConfigurationManager.ConnectionStrings["dizionari_condivisi"] != null)
                conn_string = ConfigurationManager.ConnectionStrings["dizionari_condivisi"].ConnectionString;
            else
                throw new Exception("Cannot find connection string for dizionari_condivisi in web.config");

            Database db = new Database("sqlserver", conn_string);

            return db;
        }
        
        public static SqlParameter AddWithValueOrNull(this SqlParameterCollection collection, string parameterName, object value)
        {
            return  collection.AddWithValue(parameterName, value == null ? DBNull.Value : value);
        }

        public static bool IsDBNullByName(this SqlDataReader reader, string colName)
        {
            int ordinal = reader.GetOrdinal(colName);
            return reader.IsDBNull(ordinal);
        }

        public static T GetValueOrNull<T>(this SqlDataReader reader, string colName, string dateFormat = "dd-MM-yyyy", string datetimeFormat = "dd-MM-yyyy HH:mm:ss")
        {
            CultureInfo culture = new CultureInfo("it");

            object tmp;

            int colIndex = reader.GetOrdinal(colName);

            if (reader.GetFieldType(colIndex) == typeof(short))
            {
                tmp = reader.IsDBNull(colIndex) ? default(short) : reader.GetInt16(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(int))
            {
                tmp = reader.IsDBNull(colIndex) ? default(int) : reader.GetInt32(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(double))
            {
                tmp = reader.IsDBNull(colIndex) ? default(double) : reader.GetDouble(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(long))
            {
                tmp = reader.IsDBNull(colIndex) ? default(long) : reader.GetInt64(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(string))
            {
                tmp = reader.IsDBNull(colIndex) ? default(string) : reader.GetString(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(DateTime))
            {
                tmp = reader.IsDBNull(colIndex) ? default(DateTime) : reader.GetDateTime(colIndex);
                if (reader.GetDataTypeName(colIndex).Equals("datetime") || reader.GetDataTypeName(colIndex).Equals("smalldatetime") || reader.GetDataTypeName(colIndex).Equals("datetime2") || reader.GetDataTypeName(colIndex).Equals("datetimeoffset"))
                {
                    tmp = ((DateTime)tmp).ToString(datetimeFormat);
                }
                else if (reader.GetDataTypeName(colIndex).Equals("date"))
                {
                    tmp = ((DateTime)tmp).ToString(dateFormat);
                }
            }
            else if (reader.GetFieldType(colIndex) == typeof(byte))
            {
                tmp = reader.IsDBNull(colIndex) ? default(byte) : reader.GetByte(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(bool))
            {
                tmp = reader.IsDBNull(colIndex) ? default(bool) : reader.GetBoolean(colIndex);
            }
            else if (reader.GetFieldType(colIndex) == typeof(Single))
            {
                tmp = reader.IsDBNull(colIndex) ? default(Single) : reader.GetFloat(colIndex);
            }
            else
            {
                throw new ArgumentException("Type not supported: " + reader.GetFieldType(colIndex));
            }

            if (typeof(T) == typeof(string))
            {
                tmp = tmp == null ? null : tmp.ToString();
            }

            if (typeof(T) == typeof(double))
            {
                tmp = tmp == null ? 0.0 : Convert.ToDouble(tmp, culture);
            }

            if (typeof(T) == typeof(int))
            {
                tmp = tmp == null ? 0 : Convert.ToInt32(tmp, culture);
            }

            return (T)tmp;
        }

        public static T GetValue<T>(this SqlDataReader reader, string colName)
        {
            return (T)reader[colName];
        }

        public static string BuildWhere(this string query, IDictionary<string, string> where)
        {
            string w = "";

            foreach(KeyValuePair<string, string> kvp in where)
            {
                if(!kvp.Value.IsNullOrWhiteSpace())
                {
                    if (w == "")
                        w = "WHERE";
                    else
                        w += "AND";
                    w += " " + kvp.Key + " like '%' + @" + kvp.Key + " + '%' ";
                }
            }
            return query.Replace("%where", w);
        }

        /// <summary>
        /// Inserisce un parametro DateTime come una stringa nel formato #dd/MM/yyyy HH.mm.ss#
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="name"></param>
        /// <param name="dt"></param>
        public static void AddDateTimeParameter(this OleDbCommand command, string name, DateTime dt)
        {
            //string date = dt.ToString("dd/MM/yyyy HH.mm.ss");
            string date = dt.ToString("yyyy-MM-dd HH.mm.ss");
            command.CommandText = command.CommandText.Replace(name, "#" + date + "#");
        }

        /// <summary>
        /// Inserisce un parametro DateTime come una stringa nel formato #dd/MM/yyyy HH.mm.ss#
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="name"></param>
        /// <param name="dt"></param>
        public static void AddDateParameter(this OleDbCommand command, string name, DateTime dt)
        {
            string date = dt.ToString("yyyy-MM-dd");
            command.CommandText = command.CommandText.Replace(name, "#" + date + "#");
        }

        public static string GetValueInTable(string valore, string tabella, string filter_field, string desc_field,
            bool hasRecordAttivo = false, bool hasGroupBy = false)
        {
            string result = null;

            string query = @"
                SELECT " + filter_field + ", " + desc_field + @" AS '" + desc_field + @"'
                FROM " + tabella + @"
                WHERE " + filter_field + @" = @Param
            ";

            if (hasRecordAttivo)
            {
                query += " AND record_attivo = 1";
            }

            if (hasGroupBy)
            {
                query += (" GROUP BY " + filter_field);
            }

            using (SqlConnection connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Param", valore);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = GetValueOrNull<string>(reader, desc_field);
                        }
                    }
                }
            }

            return result;
        }

        public static List<T> GetValueInTable<T>(string valore, string tabella, string filter_field, string desc_field,
            bool hasRecordAttivo = false, bool hasGroupBy = false)
        {
            List<T> result = new List<T>();

            string query = @"
                SELECT " + filter_field + ", " + desc_field + @" AS '" + desc_field + @"'
                FROM " + tabella + @"
                WHERE " + filter_field + @" = @Param
            ";

            if (hasRecordAttivo)
            {
                query += " AND record_attivo = 1";
            }

            if (hasGroupBy)
            {
                query += (" GROUP BY " + filter_field);
            }

            using (SqlConnection connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Param", valore);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(GetValueOrNull<T>(reader, desc_field));
                        }
                    }
                }
            }

            return result;
        }

        public static int GetTableRowsCount(string table)
        {
            int rows = 0;
            string query = "SELECT COUNT(chiave) FROM " + table;

            using (SqlConnection connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            rows = reader.GetInt32(0);
                        }
                    }
                }
            }

            return rows;
        }

        /**
        * Ritorna un'intera tabella id-descrizione indicizzata in un dictionary
        */
        public static Dictionary<string, string> GetTableAsDictionary(string tabella, string campo_id, string campo_descrizione, string campo_ordina)
        {
            return GetTableAsDictionary<string>(tabella, campo_id, campo_descrizione, campo_ordina, null, null);
        }

        /**
        * Ritorna un'intera tabella id-descrizione indicizzata in un dictionary
        */
        public static Dictionary<string, string> GetTableAsDictionary(string tabella, string campo_id, string campo_descrizione, string campo_ordina, Dictionary<string, string> condizioniEquals)
        {
            return GetTableAsDictionary<string>(tabella, campo_id, campo_descrizione, campo_ordina, condizioniEquals, null);
        }

        /**
        * Ritorna un'intera tabella id-descrizione indicizzata in un dictionary
        */
        public static Dictionary<string, string> GetTableAsDictionary(string tabella, string campo_id, string campo_descrizione, string campo_ordina, Dictionary<string, string> condizioniEquals, Dictionary<string, string> condizioniNotEquals)
        {
            return GetTableAsDictionary<string>(tabella, campo_id, campo_descrizione, campo_ordina, condizioniEquals, condizioniNotEquals);
        }

        /**
        * Ritorna un'intera tabella id-descrizione indicizzata in un dictionary
        */
        public static Dictionary<T, string> GetTableAsDictionary<T>(string tabella, string campo_id, string campo_descrizione, string campo_ordina, Dictionary<string, string> condizioniEquals, Dictionary<string, string> condizioniNotEquals)
        {
            string query = "SELECT DISTINCT " + campo_id + ", " + campo_descrizione + " FROM " + tabella + "";

            if (condizioniEquals != null && condizioniEquals.Count > 0 || condizioniNotEquals != null && condizioniNotEquals.Count > 0)
            {
                query += " WHERE (";

                List<string> condizioni_list = new List<string>();

                if (condizioniEquals != null && condizioniEquals.Count > 0)
                {
                    foreach (KeyValuePair<string, string> condizione in condizioniEquals)
                    {
                        condizioni_list.Add(condizione.Key + " = @CondEq_" + condizione.Key + " ");
                    }
                }
                if (condizioniNotEquals != null && condizioniNotEquals.Count > 0)
                {
                    foreach (KeyValuePair<string, string> condizione in condizioniNotEquals)
                    {
                        condizioni_list.Add(condizione.Key + " != @CondNeq_" + condizione.Key + " ");
                    }
                }

                query += String.Join(" AND ", condizioni_list.ToArray());
                query += " ) ";
            }

            query += " ORDER BY " + campo_ordina;

            Dictionary<T, string> ret = new Dictionary<T, string>();

            using (SqlConnection connection = GetSqlConnection())
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    if (condizioniEquals != null && condizioniEquals.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> condizione in condizioniEquals)
                        {
                            command.Parameters.AddWithValue("@CondEq_" + condizione.Key, condizione.Value);
                        }
                    }
                    if (condizioniNotEquals != null && condizioniNotEquals.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> condizione in condizioniNotEquals)
                        {
                            command.Parameters.AddWithValue("@CondNEq_" + condizione.Key, condizione.Value);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(0) || reader.IsDBNull(1)) continue;
                            string descrizione = reader.GetString(1);
                            object tmp;
                            if (typeof(T) == typeof(short))
                                tmp = reader.GetInt16(0);
                            else if (typeof(T) == typeof(int))
                                tmp = reader.GetInt32(0);
                            else if (typeof(T) == typeof(double))
                                tmp = reader.GetDouble(0);
                            else if (typeof(T) == typeof(string))
                                tmp = reader.GetString(0);
                            else if (typeof(T) == typeof(DateTime))
                                tmp = reader.GetDateTime(0);
                            else throw new ArgumentException("Type not supported: " + typeof(T));
                            ret[(T)tmp] = descrizione;
                        }
                    }
                }
            }
            return ret;
        }

        public static bool AddUsernameToDizionario(string username)
        {
            bool status = false;
            string query = @"
                INSERT INTO ruolo_dizionario (
                    nome_utente, dizionario, 
                    time_modifica, record_attivo
                ) VALUES (
                    @Username, @Dizionario,
                    CURRENT_TIMESTAMP, 1
                )
            ";

            // Aggiorno la tabella ruolo_dizionario aggiungendo 
            // tutti i dizionari presenti in elenco_dizionario
            Dictionary<string, string> dizionari = DbExtensions.GetTableAsDictionary(
                "elenco_dizionari", "dizionario", "chiave", "dizionario",
                new Dictionary<string, string> { { "record_attivo", "1" } }, null
            );

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    foreach (string dizionario in dizionari.Keys)
                    {
                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Username", username);
                            command.Parameters.AddWithValue("@Dizionario", dizionario);

                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    status = true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    status = false;

                    if (e.Message.StartsWith("Cannot insert duplicate key row"))
                    {
                        status = true;
                    }
                }
            }

            return status;
        }

        public static bool RemoveUsernameFromDizionario(string username)
        {
            bool status = false;
            string query = @"
                UPDATE ruolo_dizionario
                SET time_modifica = CURRENT_TIMESTAMP, 
                    record_attivo = 0
                WHERE nome_utente = @Username
            ";

            using (SqlConnection connection = DbExtensions.GetSqlConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    status = true;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    status = false;
                }
            }

            return status;
        }
    }
}