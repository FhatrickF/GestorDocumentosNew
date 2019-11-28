using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GestorDocumentosEntities
{
    public class DataTableObjectEntity <T> where T : IList
    {
        public DataTableObjectEntity(T aaData, int sEcho, int totalRow)
        {
            this.aaData = aaData;
            this.sEcho = sEcho;
            this.iTotalRecords = totalRow;
            this.iTotalDisplayRecords = totalRow;
        }
        public int sEcho { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public T aaData { get; set; }
    }
}
