    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.AllClass
{
    public class Record
    {
        public int RecordID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string UserName { get; set; }
        public string ClientName { get; set; }
        public string Diagnosis { get; set; }
        public string DrugName { get; set; }
    }

}
