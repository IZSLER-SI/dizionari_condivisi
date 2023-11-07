using DotNetNuke.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiLogging
{
    public class DatabaseField
    {
        protected ILog Log = LoggerSource.Instance.GetLogger(typeof(DatabaseField));

        public string extend { get; set; }
        public DateTime time_modifica { get; set; }
        public int record_attivo { get; set; }
        public DatabaseField(string extend, DateTime time_modifica, int record_attivo)
        {
            this.extend = extend;
            this.time_modifica = time_modifica;
            this.record_attivo = record_attivo;
        }
    }
}
