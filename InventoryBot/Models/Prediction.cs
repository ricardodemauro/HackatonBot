using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BertaBot.Models
{
    public class Prediction
    {
        public decimal probability { get; set; }
        public string tagId { get; set; }
        public string tagName { get; set; }

        public string brand { get; set; }

        public string model { get; set; }
        public bool isModel { get; set; }

        public override string ToString()
        {
            return tagName;
        }
    }
}
