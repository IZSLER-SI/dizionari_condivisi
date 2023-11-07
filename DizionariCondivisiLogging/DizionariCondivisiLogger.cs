using it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging
{
    public class DizionariCondivisiLogger
    {
        /// <summary>
        /// Inserisce un evento nel log attività
        /// </summary>
        /// <param name="utente"></param>
        /// <param name="descrizione_operazione"></param>
        /// <param name="data">un qualsiasi oggetto, verrà codificato in JSON</param>
        public static void LogActivity(string modulo, string operazione, string status,
            string dizionario = null, object dati = null)
        {
            DateTime oggi = DateTime.Now;
            LogOperazione op = new LogOperazione(
                oggi.Year, WebUtils.GetCurrentUsername(), modulo,
                operazione, status, dizionario, dati, oggi, null, oggi, 1
            );

            op.insert();
        }
        
        public static void LogInsertStatisticheDizionario(string dizionario, string descrizione, string chiave)
        {
            StatisticheDizionario sd = new StatisticheDizionario(dizionario, descrizione, chiave,
                0, WebUtils.GetCurrentUsername(), null, DateTime.Now, 1);

            sd.InsertStatisticheDizionario();
        }

        public static void LogUpdateStatisticheDizionario(string dizionario, int numero_record)
        {
            StatisticheDizionario sd = new StatisticheDizionario(dizionario, numero_record, WebUtils.GetCurrentUsername(),
                null, DateTime.Now, 1);

            sd.UpdateStatisticheDizionario();
        }
    }
}
