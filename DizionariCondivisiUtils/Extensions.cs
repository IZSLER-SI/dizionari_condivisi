using DotNetNuke.Instrumentation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public static class Extensions
    {

        public static int ToIntDef(this string s, int def)
        {
            int res;
            if (!int.TryParse(s, out res))
                res = def;
            return res;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static int[] ConvertRowsId(string id, ILog log)
        {
            int[] ret = null;

            id = id.Replace("\"[", "");
            id = id.Replace("]\"", "");
            id = id.Replace("\"", "");
            id = id.Replace("[", "");
            id = id.Replace("]", "");

            // Array che contiene gli id_campione selezionati nel cruscotto
            try
            {
                string[] tmp_rows_convert = id.Split(',');
                ret = new int[tmp_rows_convert.Length];
                for (int i = 0; i < tmp_rows_convert.Length; i++)
                {
                    try
                    {
                        ret[i] = Convert.ToInt32(tmp_rows_convert[i]);
                    }
                    catch (Exception e)
                    {
                        log.Error("Errore nella conversione dell'id_campione: @" +
                            tmp_rows_convert[i] + ". Messaggio: " + e.Message, e);
                        throw e;
                    }
                }
            }
            catch (Exception onlyOneId)
            {
                ret = new int[1];
                ret[0] = Convert.ToInt32(id);
                return ret;
            }

            return ret;
        }

        public static int[] ConvertRowsId(string id, ILog log, bool nuovo)
        {
            var js = new JavaScriptSerializer();
            int[] ret = null;

            // Array che contiene gli id_campione selezionati nel cruscotto
            try
            {
                ret = js.Deserialize<int[]>(id);
            }
            catch (Exception e)
            {
                log.Error("Errore nella conversione del vettore di id_campione: @" + id + ". Messaggio: " + e.Message, e);
            }

            return ret;
        }

        #region ArrayToString
        public static string ArrayToString(IList array)
        {
            return ArrayToString(array, Environment.NewLine);
        }

        public static string ArrayToString(IList array, string delimeter)
        {
            string outputString = "";

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is IList)
                {
                    //Recursively convert nested arrays to string
                    outputString += ArrayToString((IList)array[i], delimeter);
                }
                else
                {
                    outputString += array[i];
                }

                if (i != array.Count - 1)
                    outputString += delimeter;
            }

            return outputString;
        }



        public static string ArrayToStringGeneric<T>(IList<T> array)
        {
            return ArrayToStringGeneric<T>(array, Environment.NewLine);
        }

        public static string ArrayToStringGeneric<T>(IList<T> array, string delimeter)
        {
            string outputString = "";

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is IList)
                {
                    //Recursively convert nested arrays to string
                    outputString += ArrayToStringGeneric<T>((IList<T>)array[i], delimeter);
                }
                else
                {
                    outputString += array[i];
                }

                if (i != array.Count - 1)
                    outputString += delimeter;
            }

            return outputString;
        }
        #endregion

        public static string GetCurrentModule()
        {
            return Assembly.GetCallingAssembly().GetName().Name;
        }

        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            return sf.GetMethod().Name;
        }
    }
}
