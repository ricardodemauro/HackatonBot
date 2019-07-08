using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BertaBot.Vehicles.Models
{
    public class CarModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Brand { get; set; }

        public string Color { get; set; }

        public string Plate { get; set; }

        public IList<string> Base64Images { get; set; }

        public CarModel()
        {
            Base64Images = new List<string>();
        }
    }
}
