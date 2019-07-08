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
        [Required] public string Name { get; set; }
        [Required] public string Brand { get; set; }
        [Required] public string Color { get; set; }
        public string Plate { get; set; }
        public string Base64Image { get; set; }

        public CarModel()
        {
        }

        public CarModel(Car entity)
        {
            Name = entity.Name;
            Brand = entity.Brand;
            Color = entity.Color;
            Plate = entity.Plate;
            Base64Image = entity.Base64Image;
        }
    }
}
