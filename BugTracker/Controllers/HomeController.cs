using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            return View();
        }

        [NoDirectAccess]
        public ActionResult MasterIndex()
        {
            return View();
        }

        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult AdminMasterIndex()
        {
            return View();
        }

        public ActionResult LandingPage()
        {
            return View();
        }

        [NoDirectAccess]
        public ActionResult Calendar()
        {
            return View();
        }
        [NoDirectAccess]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [NoDirectAccess]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel contactEmail)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var from = "EMS<rhksoftdev@gmail.com>";
                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        Subject = "A Message from Error Management Systems",
                        //Body = string.Format(body, model.FromName, model.FromEmail, model.Subject, model.Body),
                        Body = "Email From: " + contactEmail.FromName + " Email:" + contactEmail.FromEmail + "Message: " + contactEmail.Body,
                        IsBodyHtml = true
                    };
                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);
                    //ViewBag.IsEamilSent = true;
                    //ViewBag.Message = "Message Sent";
                    TempData["AlertMessage"] = "Your message has been sent!";
                    return RedirectToAction("LandingPage", "Home");
                    //return View();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View("LandingPage");
        }
    }

}