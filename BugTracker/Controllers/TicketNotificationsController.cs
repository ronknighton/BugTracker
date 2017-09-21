using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Helpers;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    public class TicketNotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketsHelper ticketHelper = new TicketsHelper();
        private ProjectsHelper projectHelper = new ProjectsHelper();
        private UserHelper userHelper = new UserHelper();
        private UserRolesHelper userRolesHelper = new UserRolesHelper();
        // GET: TicketNotifications
        [NoDirectAccess]
        public ActionResult Index()
        {
            var ticketNotifications = db.TicketNotifications.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketNotifications.ToList());
        }

        [NoDirectAccess]
        public ActionResult UserIndex()
        {
            return View("Index", ticketHelper.UserTicketNotifications());
        }

        [NoDirectAccess]
        public ActionResult _PartialNotifications()
        {
            return PartialView(ticketHelper.UserTicketNotifications());
        }

        [NoDirectAccess]
        public ActionResult _PartialAdminNotifications()
        {
            var ticketNotifications = db.TicketNotifications.Include(t => t.Ticket).Include(t => t.User);
            return PartialView(ticketNotifications.ToList());
        }

        // GET: TicketNotifications/Details/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Create       
        //[Authorize(Roles = "Admin")]
        [NoDirectAccess]
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userTickets = ticketHelper.MyProjectTickets(user.Id).Where(t => t.Archived == false).ToList();
            var userProjects = projectHelper.ListUserProjects(user.Id).Where(t => t.Archived == false).ToList();
            var adminUsers = userRolesHelper.UsersInRole("Admin");
            var usersOnProjects = new List<ApplicationUser>();
            var tempUsers = new List<ApplicationUser>();

            if (User.IsInRole("Admin"))
            {
                ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
                ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName");
            }
            else
            {
                foreach (var project in userProjects)
                {
                    foreach (var currentUser in project.ProjectUsers)
                    {
                        tempUsers.Add(currentUser);
                    }
                }
                foreach(var admin in adminUsers)
                {
                    tempUsers.Add(admin);
                }
                usersOnProjects = tempUsers.Distinct().ToList();
                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
                ViewBag.TicketId = new SelectList(userTickets, "Id", "Title");
                //ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName");
                ViewBag.UserId = new SelectList(usersOnProjects, "Id", "DisplayName");
            }
            return View();
        }

        // POST: TicketNotifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,NotifyReason,TicketId,UserId")] TicketNotification ticketNotification)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ticketNotification.ChangedBy = user.Id;
                ticketNotification.Created = DateTimeOffset.Now;
                db.TicketNotifications.Add(ticketNotification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Edit/5
        [NoDirectAccess]
        //[Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,NotifyReason,TicketId,UserId,HasBeenRead,Created,ChangedBy")] TicketNotification ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketNotification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketNotification.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.UserId);
            return View(ticketNotification);
        }

        // GET: TicketNotifications/Delete/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            if (ticketNotification == null)
            {
                return HttpNotFound();
            }
            return View(ticketNotification);
        }

        // POST: TicketNotifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketNotification ticketNotification = db.TicketNotifications.Find(id);
            db.TicketNotifications.Remove(ticketNotification);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
