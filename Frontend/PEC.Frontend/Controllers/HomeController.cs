using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PEC.Frontend.DataBase;
using PEC.Frontend.Models;

namespace PEC.Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly PECContext _context;

        public HomeController(PECContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var carModels = _context.Cars.Include(x => x.CarImages).OrderByDescending(x => x.CreationDate).Select(x => new CarModel(x));

            return View(carModels);
        }
    }
}
