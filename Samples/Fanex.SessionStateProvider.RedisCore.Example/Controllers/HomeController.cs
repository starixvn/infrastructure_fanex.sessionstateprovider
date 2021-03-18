namespace Fanex.SessionStateProvider.RedisCore.Example.Controllers
{
    using System.Diagnostics;
    using Fanex.SessionStateProvider.AspNetCore;
    using Fanex.SessionStateProvider.RedisCore.Example.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var account = new Account
            {
                FirstName = HttpContext.Session.Get<string>("FirstName"),
                MiddleName = HttpContext.Session.Get<string>("MiddleName"),
                LastName = HttpContext.Session.Get<string>("LastName"),
                Username = HttpContext.Session.Get<string>("Username"),
                Email = HttpContext.Session.Get<string>("Email"),
                Phone = HttpContext.Session.Get<string>("Phone"),
                Address = HttpContext.Session.Get<string>("Address"),
                City = HttpContext.Session.Get<string>("City"),
                Organization = HttpContext.Session.Get<string>("Organization"),
                Remarks = HttpContext.Session.Get<string>("Remarks")
            };

            return View(account);
        }

        [HttpPost]
        public IActionResult Index(Account account)
        {
            HttpContext.Session.Set("FirstName", account.FirstName);
            HttpContext.Session.Set("MiddleName", account.MiddleName);
            HttpContext.Session.Set("LastName", account.LastName);
            HttpContext.Session.Set("Username", account.Username);
            HttpContext.Session.Set("Email", account.Email);
            HttpContext.Session.Set("Phone", account.Phone);
            HttpContext.Session.Set("Address", account.Address);
            HttpContext.Session.Set("City", account.City);
            HttpContext.Session.Set("Organization", account.Organization);
            HttpContext.Session.Set("Remarks", account.Remarks);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}