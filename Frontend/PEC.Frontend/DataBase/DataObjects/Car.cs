using PEC.Frontend.Models;

namespace PEC.Frontend.DataBase.DataObjects
{
    public class Car : DataEntity
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string Plate { get; set; }
        public string Base64Image { get; set; }

        public Car() : base()
        {
        }

        public Car(CarModel model) : this()
        {
            Name = model.Name;
            Brand = model.Brand;
            Color = model.Color;
            Plate = model.Plate;
            Base64Image = model.Base64Image;
        }
    }
}
