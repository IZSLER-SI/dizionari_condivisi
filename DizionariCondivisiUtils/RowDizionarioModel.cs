using System;
using System.Web.Http;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    public class RowDizionarioModel : ARowDizionarioModel
    {
        public RowDizionarioModel(string table) : base(table)
        {

        }
        public RowDizionarioModel(string table, string campiModificabili) : base(table, campiModificabili)
        {

        }

        //public RowDizionarioModel(string table, string chiave, string descrizione,
        //    DateTime last_update, DateTime valid_from, DateTime valid_to,
        //    int codice_darwin) : base(table, chiave, descrizione,
        //        last_update, valid_from, valid_to, codice_darwin)
        //{

        //}

        public override IHttpActionResult Create()
        {
            throw new NotImplementedException();
        }

        public override IHttpActionResult Edit()
        {
            throw new NotImplementedException();
        }

        public override IHttpActionResult Remove()
        {
            throw new NotImplementedException();
        }

        public override IHttpActionResult Get()
        {
            throw new NotImplementedException();
        }
    }
}
