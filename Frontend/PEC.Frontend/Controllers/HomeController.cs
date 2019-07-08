using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            var carModels = _context.Cars.OrderByDescending(x => x.CreationDate).Select(x => new CarModel(x));

            return View(carModels);
        }
    }
}
