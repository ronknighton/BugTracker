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

namespace BugTracker.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private AccessValidator validator = new AccessValidator();
        private TicketsHelper ticketsHelper = new TicketsHelper();

        // GET: TicketAttachments
        //[Authorize(Roles = "Admin")]
        [NoDirectAccess]
        public ActionResult Index()
        {
            var ticketAttachments = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            ViewBag.UserIndexFlag = false;
            return View(ticketAttachments.ToList());
        }

        [NoDirectAccess]
        public ActionResult UserIndex()
        {
            var ticketAttachments = ticketsHelper.GetUserTicketAttachments();
            ViewBag.UserIndexFlag = true;
            return View("Index", ticketAttachments);
        }

        // GET: TicketAttachments/Details/5
        [NoDirectAccess]
        
        public ActionResult Details(int? id, bool flag)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserIndexFlag = flag;
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName");
            return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MediaURL,Description,Created,UserId,TicketId")] TicketAttachment ticketAttachment)
        {
            if (ModelState.IsValid)
            {
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Edit/5
        [NoDirectAccess]
       public ActionResult Edit(int? id, bool flag)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attachId = (int)id;
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (User.IsInRole("Admin") || validator.PMProjectTicket(ticketAttachment.Ticket.Id) || validator.AttachmentOwner(attachId))
            {

                if (ticketAttachment == null)
                {
                    return HttpNotFound();
                }
                ViewBag.UserIndexFlag = flag;
                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
                //ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachment.UserId);
                ViewBag.TicketTitle = db.Tickets.FirstOrDefault(t => t.Id == ticketAttachment.TicketId).Title;
                ViewBag.UserDisplayName = db.Users.FirstOrDefault(u => u.Id == ticketAttachment.UserId).DisplayName;
                return View(ticketAttachment);
            }
            return RedirectToAction("UserIndex", "Tickets");
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MediaURL,Description,Created,UserId,TicketId")] TicketAttachment ticketAttachment, bool flag, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(ticketAttachment.Description))
                {
                    ViewBag.TicketTitle = db.Tickets.FirstOrDefault(t => t.Id == ticketAttachment.TicketId).Title;
                    ViewBag.UserDisplayName = db.Users.FirstOrDefault(u => u.Id == ticketAttachment.UserId).DisplayName;
                    TempData["FileError"] = "Attachment must have a description";
                    ViewBag.UserIndexFlag = flag;
                    //return View(ticketComment);
                    return View(ticketAttachment);
                }
                if (!string.IsNullOrWhiteSpace(ticketAttachment.Description) && attachment != null)
                {
                    var fileValidator = new FileUploadValidator();
                    var fileUrl = fileValidator.ValidateAndSaveFile2(attachment);
                    var message = "";

                    switch (fileUrl)
                    {
                        case "null":
                            message = "An Image Must Be Selected";
                            break;
                        case "small":
                            message = "Invalid Image: Must be larger than 3Kb.";
                            break;
                        case "large":
                            message = "Invalid Image: Must be smaller than 2Mb.";
                            break;
                        case "format":
                            message = "Invalid Image format";
                            break;
                    }

                    if (message != "")
                    {
                        TempData["FileError"] = message;
                        ViewBag.UserIndexFlag = flag;
                        //return View(ticketComment);
                        return View(ticketAttachment);
                    }

                    ticketAttachment.MediaURL = fileUrl;
                }
                db.Entry(ticketAttachment).State = EntityState.Modified;
                    db.SaveChanges();
                if (flag)
                {
                    return RedirectToAction("UserIndex", "TicketAttachments");
                }
                return RedirectToAction("Index", "TicketAttachments");
            }
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            //ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketAttachment.UserId);
            if (string.IsNullOrWhiteSpace(ticketAttachment.Description))
            {
                TempData["FileError"] = "Attachment must have a description";
               
            }
            ViewBag.TicketTitle = db.Tickets.FirstOrDefault(t => t.Id == ticketAttachment.TicketId).Title;
            ViewBag.UserDisplayName = db.Users.FirstOrDefault(u => u.Id == ticketAttachment.UserId).DisplayName;
            ViewBag.UserIndexFlag = flag;
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5  
        [NoDirectAccess]
        public ActionResult Delete(int? id, bool flag)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var attachId = (int)id;
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (User.IsInRole("Admin") || validator.PMProjectTicket(ticketAttachment.Ticket.Id) || validator.AttachmentOwner(attachId))
            {

                if (ticketAttachment == null)
                {
                    return HttpNotFound();
                }
                ViewBag.UserIndexFlag = flag;
                return View(ticketAttachment);
            }
            return RedirectToAction("UserIndex", "Tickets");
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, bool flag)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachment);
            db.SaveChanges();
            if (flag)
            {
                return RedirectToAction("UserIndex", "Tickets");
            }
            return RedirectToAction("Index", "Tickets");
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
