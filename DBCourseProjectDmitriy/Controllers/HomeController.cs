using DBCourseProjectDmitriy.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DBCourseProjectDmitriy.Controllers
{
    public class HomeController : Controller
    {
        public HomeController() { }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Manufacturer()
        {
            return View();
        }

        public IActionResult Electronics()
        {
            return View();
        }

        public IActionResult Contract()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

		public IActionResult Orders()
		{
			return View();
		}

		public IActionResult Bills()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}