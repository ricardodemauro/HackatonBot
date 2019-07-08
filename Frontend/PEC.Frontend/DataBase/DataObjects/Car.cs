using System.Collections.Generic;
using PEC.Frontend.Models;

namespace PEC.Frontend.DataBase.DataObjects
{
    public class Car : DataEntity
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Plate { get; set; }
        public IList<CarImage> CarImages { get; set; }

        public Car() : base()
        {
            CarImages = new List<CarImage>();
        }

        public Car(CarModel model) : this()
        {
            Name = model.Name;
            Brand = model.Brand;
            Color = model.Color;
            Plate = model.Plate;

            foreach (var imageBase64 in model.Base64Images)
                CarImages.Add(new CarImage(imageBase64));
        }
    }
}
