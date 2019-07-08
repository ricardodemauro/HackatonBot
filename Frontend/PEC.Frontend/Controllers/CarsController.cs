using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PEC.Frontend.DataBase;
using PEC.Frontend.DataBase.DataObjects;
using PEC.Frontend.Hubs;
using PEC.Frontend.Models;

namespace PEC.Frontend.Controllers
{
    [Route("/{controller}")]
    public class CarsController : ControllerBase
    {
        private readonly PECContext _context;
        private readonly IHubContext<CarHub> _carHubContext;

        public CarsController(PECContext context, IHubContext<CarHub> carHubContext)
        {
            _context = context;
            _carHubContext = carHubContext;
        }

        [HttpPost]
        public IActionResult Post([FromBody]CarModel model)
        {
            var dataEntity = new Car(model);

            _context.Cars.Add(dataEntity);
            _context.SaveChanges();

            _carHubContext.Clients.All.SendAsync("AddNewVehicle", dataEntity);

            return Created("", dataEntity);
        }
    }
}