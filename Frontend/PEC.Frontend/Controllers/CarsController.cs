using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PEC.Frontend.DataBase;
using PEC.Frontend.DataBase.DataObjects;
using PEC.Frontend.Hubs;
using PEC.Frontend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IEnumerable<Car>> ListAsync(CancellationToken cancellationToken)
        {
            var vehicles = await _context.Cars.Take(6).ToListAsync(cancellationToken);

            return vehicles;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var vehicleDelete = await _context.Cars.FindAsync(id);
            if (vehicleDelete == null)
                return NotFound();

            _context.Cars.Remove(vehicleDelete);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}