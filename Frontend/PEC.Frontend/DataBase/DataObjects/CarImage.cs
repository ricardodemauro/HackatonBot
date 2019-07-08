using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PEC.Frontend.DataBase.DataObjects
{
    public class CarImage : DataEntity
    {
        public string Base64 { get; set; }
        public int CarId { get; set; }

        public CarImage() : base()
        {
            
        }

        public CarImage(string base64) : this()
        {
            Base64 = base64;
        }
    }
}
