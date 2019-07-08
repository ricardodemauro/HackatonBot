using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEC.Frontend.DataBase.DataObjects
{
    public abstract class DataEntity
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }

        protected DataEntity()
        {
            CreationDate = DateTime.Now;
        }
    }
}
