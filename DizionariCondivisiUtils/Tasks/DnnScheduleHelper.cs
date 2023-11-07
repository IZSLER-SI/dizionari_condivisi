using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Common.Utilities;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils.Tasks
{
    /// <summary>
    /// Gestisce la creazione e l'avvio del task
    /// </summary>
    /// <seealso cref="http://www.dnnsoftware.com/forums/threadid/354348/scope/posts/how-to-start-a-scheduled-task-from-module-code"/>
    /// <seealso cref="https://dnnzone.wordpress.com/2010/08/28/create-dnn-schedule-in-code/"/>
    public abstract class DnnScheduleHelper
    {

        /// <summary>
        /// Ritorna il task richiesto
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static ScheduleItem GetScheduleItem(string typeFullName)
        {
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            ScheduleItem si = scheduler.GetSchedule(typeFullName, "");
            return si;
        }

        /// <summary>
        /// Ritorna la lista delle esecuzioni del task specificato
        /// </summary>
        /// <param name="si"></param>
        /// <returns></returns>
        public static List<ScheduleHistoryItem> GetScheduleHistory(ScheduleItem si)
        {
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            List<ScheduleHistoryItem> history = scheduler.GetScheduleHistory(si.ScheduleID).Cast<ScheduleHistoryItem>().ToList();
            return history;
        }

        /// <summary>
        /// Ritorna la lista dei task in coda pronti per essere eseguiti
        /// </summary>
        /// <returns></returns>
        public static List<ScheduleHistoryItem> GetScheduleProcessing()
        {
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            var processing = scheduler.GetScheduleProcessing().Cast<ScheduleHistoryItem>().ToList();
            return processing;
        }

		/// <summary>
		/// Chiede a DNN di eseguire al più presto possibile il task richiesto
		/// </summary>
		/// <param name="typeFullName"></param>
        public static void RunSchedule(string typeFullName)
        {
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            ScheduleItem si = scheduler.GetSchedule(typeFullName, "");
            if (si == null)
                throw new TaskNotFoundException(typeFullName);
            scheduler.RunScheduleItemNow(si);
        }

        /// <summary>
        /// Crea una schedule per il tipo impostato
        /// </summary>
        /// <param name="typeFullName">il type name completo della classe che contiene il metodo DoWork da eseguire</param>
        /// <param name="friendlyName">il nome da mostrare all'utente</param>
        /// <param name="objDependencies"></param>
        /// <param name="timeLapse">l'intervallo temporale di esecuzione del task</param>
        /// <param name="timeLapseMeasurement">l'unità di misura dell'intervallo temporale</param>
        /// <returns>the scheduleId</returns>
        public static int CreateSchedule(string typeFullName, string friendlyName, string objDependencies, int timeLapse, string timeLapseMeasurement)
        {
            // prima controllo se il task esiste già
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            ScheduleItem si = scheduler.GetSchedule(typeFullName, "");
            if (si != null)
                throw new TaskAlreadyExistException(typeFullName);

            // creo il task
            si = new ScheduleItem();
            si.FriendlyName = friendlyName;
            si.TypeFullName = typeFullName;
            si.TimeLapse = timeLapse;
            si.TimeLapseMeasurement = timeLapseMeasurement;
            si.RetryTimeLapse = 1;
            si.RetryTimeLapseMeasurement = "m";
            si.RetainHistoryNum = 100;
            si.AttachToEvent = "";
            si.CatchUpEnabled = false;
            si.Enabled = true;
            si.ObjectDependencies = objDependencies;
            si.Servers = Null.NullString;

            // aggiungo il task
            return scheduler.AddSchedule(si);

        }
    }
}