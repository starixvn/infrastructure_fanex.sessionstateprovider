namespace Fanex.SessionStateProvider.Redis.Example.Controllers
{
    using System.Web.Mvc;
    using Fanex.SessionStateProvider.Redis.Example.Models;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Account account)
        {
            Session["FirstName"] = account.FirstName;
            Session["MiddleName"] = account.MiddleName;
            Session["LastName"] = account.LastName;
            Session["Username"] = account.Username;
            Session["Email"] = account.Email;
            Session["Phone"] = account.Phone;
            Session["Address"] = account.Address;
            Session["City"] = account.City;
            Session["Organization"] = account.Organization;
            Session["Remarks"] = account.Remarks;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult AbandonSession()
        {
            Session.Abandon();

            return RedirectToAction(nameof(Index));
        }
    }
}