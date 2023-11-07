using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public class GeneralUtils
    {
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^\+?([0-9]*)$").Success;
        }

        public static bool IsValidCodiceFiscale(string codicefiscale)
        {
            string pattern = "^[A-Za-z]{6}[0-9]{2}[A-Za-z]{1}[0-9]{2}[A-Za-z]{1}[0-9A-Za-z]{3}[A-Za-z]{1}$";

            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(codicefiscale);

            return matches.Count > 0;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool ToExcelXls(DataSet ds, string filename)
        {
            string path = "";

            Directory.CreateDirectory(path);

            var sb = new FileStream(
                path + "\\" + filename, 
                FileMode.Create
            );

            ExcelWriterXls writer = new ExcelWriterXls(sb);

            writer.BeginWrite();
            DataTable data = ds.Tables[0]; // solo una tabella

            // Write the worksheet contents
            int x = 0;
            int y = 0;

            //Write header row
            foreach (DataColumn col in data.Columns)
            {
                writer.WriteCell(y, x, col.Caption);
                x++;
            }

            //write data
            foreach (DataRow row in data.Rows)
            {
                y++;
                x = 0;
                foreach (object o in row.ItemArray)
                {
                    if (o is string || System.Convert.IsDBNull(o))
                    {
                        writer.WriteCell(y, x, Convert.ToString(o));
                    }
                    else if (o is Int16 || o is Int32 || o is Int64 || o is SByte ||
                    o is UInt16 || o is UInt32 || o is UInt64 || o is Byte)
                    {
                        writer.WriteCell(y, x, Convert.ToInt32(o));
                    }
                    else if (o is Single || o is Double || o is Decimal) //we'll assume it's a currency
                    {
                        writer.WriteCell(y, x, Convert.ToDouble(o));
                    }
                    else if (o is DateTime)
                    {
                        writer.WriteCell(y, x, Convert.ToDateTime(o).ToString("dd/MM/yy"));
                        //writer.WriteCell(y, x, Convert.ToDateTime(o), 0);
                    }
                    x++;
                }
            }

            // Close up the document
            writer.EndWrite();
            sb.Close();

            return true;
        }

        public static string ToCsv(DataSet ds, string RowDelimiter, string ColDelimiter)
        {

            DataTable data = ds.Tables[0]; // solo una tabella
            var sb = new StringBuilder();
            int x = 0;
            for (x = 0; x < data.Columns.Count; x++)
            {
                // lo spazio è aggiunto perchè se la prima colonna comincia per "ID" da problemi ad excel (SYLK format);
                sb.Append(" " + data.Columns[x].Caption);
                if (x < data.Columns.Count - 1)
                {
                    sb.Append(ColDelimiter);
                }
            }
            foreach (DataRow row in data.Rows)
            {
                sb.Append(RowDelimiter);
                for (x = 0; x < data.Columns.Count; x++)
                {
                    if (row[x] is string)
                    {
                        string s = (string)row[x];
                        if (s.Length > 0 && "1234567890".IndexOf(s[0]) >= 0)
                        {
                            s = Convert.ToChar(0) + s;
                        }
                        //sb.Append("\"" + s.Replace("\"", "\"\"").Replace(RowDelimiter, "").Replace(ColDelimiter, "") + "\"");
                        sb.Append(s.Replace("\"", "\"\"").Replace(RowDelimiter, "").Replace(ColDelimiter, ""));
                    }
                    else
                    {
                        if (row[x] is DateTime)
                        {
                            DateTime d = (DateTime)row[x];
                            if (d.Hour == 0 && d.Minute == 0 && d.Second == 0)
                            {
                                sb.Append(d.ToShortDateString());
                            }
                            else
                            {
                                sb.Append(d);
                            }
                        }
                        else
                        {
                            sb.Append(row[x]);
                        }
                    }
                    if (x < data.Columns.Count - 1)
                    {
                        sb.Append(ColDelimiter);
                    }
                }
            }
            return sb.ToString();

        }

        public static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToSha1Encode(byte[] txt)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(txt);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
