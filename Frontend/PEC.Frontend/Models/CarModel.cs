using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PEC.Frontend.DataBase.DataObjects;

namespace PEC.Frontend.Models
{
    public class CarModel
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Brand { get; set; }
        [Required] public string Color { get; set; }
        public string Plate { get; set; }
        public IList<string> Base64Images { get; set; }

        public CarModel()
        {
            Base64Images = new List<string>();
        }

        public CarModel(Car entity) : this()
        {
            Id = entity.Id;
            Name = entity.Name;
            Brand = entity.Brand;
            Color = entity.Color;
            Plate = entity.Plate;
            
            foreach(var carImage in entity.CarImages)
                Base64Images.Add(carImage.Base64);
        }
    }
}
