using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using BugTracker.Helpers;

namespace BugTracker.Controllers
{
    public class TicketCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private AccessValidator validator = new AccessValidator();
        private NotificationHistoryHelper NHHelper = new NotificationHistoryHelper();
        private TicketsHelper ticketsHelper = new TicketsHelper();


        //GET: TicketComments 
        [NoDirectAccess]
        public ActionResult Index()
        {
            var ticketComments = db.TicketComments.Include(t => t.Ticket).Include(t => t.User);
            ViewBag.UserIndexFlag = false;
            return View(ticketComments.ToList());
        }

        [NoDirectAccess]
        public ActionResult UserIndex()
        {
            var ticketComments = ticketsHelper.GetUserTicketComments();
            ViewBag.UserIndexFlag = true;
            return View("Index", ticketComments);
        }

        // GET: TicketComments/Details/5
        //[Authorize(Roles = "Admin")]
        [NoDirectAccess]
        public ActionResult Details(int? id, bool flag)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserIndexFlag = flag;
            return View(ticketComment);
        }

        //GET: TicketComments/Create
        //[NoDirectAccess]
        //[Authorize(Roles = "Admin")]
        //public ActionResult Create()
        //{
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName");
        //    return View();
        //}

        //POST: TicketComments/Create
        //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
         //more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Comment,TicketId")] TicketComment ticketComment, HttpPostedFileBase attachment, string attachDesc, bool flag)
        {
            var ticketAttachment = new TicketAttachment();

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(attachDesc))
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
                        //return View(ticketComment);
                        return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId, flag = flag });
                    }
                    
                    ticketAttachment.MediaURL = fileUrl;
                    ticketAttachment.Description = attachDesc;
                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.TicketId = ticketComment.TicketId;
                    ticketAttachment.UserId = User.Identity.GetUserId();

                    db.TicketAttachments.Add(ticketAttachment);
                    db.SaveChanges();
                }
                if (!string.IsNullOrWhiteSpace(ticketComment.Comment))
                {
                    if (ticketComment.Comment.Trim().Length < 10)
                    {
                        TempData["CommentError"] = "Comment must be at least 10 characters";
                        return RedirectToAction("Details", "Tickets", new {id = ticketComment.TicketId, flag = flag});
                        //return View(ticketComment);
                    }

                    ticketComment.UserId = User.Identity.GetUserId();
                    ticketComment.Created = DateTimeOffset.Now;
                    db.TicketComments.Add(ticketComment);
                    if (!string.IsNullOrWhiteSpace(ticketAttachment.Description))
                    {
                        var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketComment.TicketId);
                        NHHelper.NotificationCreator("Attachment Added", ticket, ticketAttachment.Created, ticket.AssignedToUserId);
                    }

                    if (!string.IsNullOrWhiteSpace(ticketComment.Comment))
                    {
                        var ticket = db.Tickets.FirstOrDefault(t => t.Id == ticketComment.TicketId);
                        NHHelper.NotificationCreator("Comment Added", ticket, ticketComment.Created, ticket.AssignedToUserId);
                    }
                    db.SaveChanges();
                 
                    return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId, flag = flag });
                }
            }

            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "DisplayName", ticketComment.UserId);
            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId, flag = flag });
        
        }

        // GET: TicketComments/Edit/5
        [NoDirectAccess]
        public ActionResult Edit(int? id, bool flag)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var commentId = (int)id;
            TicketComment ticketComment = db.TicketComments.Find(id);
            
            if (User.IsInRole("Admin") || validator.PMProjectTicket(ticketComment.Ticket.Id) || validator.CommentOwner(commentId))
            {
                if (ticketComment == null)
                {
                    return HttpNotFound();
                }
                ViewBag.UserIndexFlag = flag;             
                ViewBag.UserDisplayName = db.Users.FirstOrDefault(u => u.Id == ticketComment.UserId).DisplayName;
                ViewBag.TicketTitle = db.Tickets.FirstOrDefault(t => t.Id == ticketComment.TicketId).Title;
                return View(ticketComment);
            }
            return RedirectToAction("UserIndex", "Tickets");
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment, bool flag)
        {
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(ticketComment.Comment))
            {
                db.Entry(ticketComment).State = EntityState.Modified;
                db.SaveChanges();
                if (flag)
                {
                    return RedirectToAction("UserIndex", "TicketComments");
                }
                return RedirectToAction("Index", "TicketComments");
            }
            ViewBag.UserIndexFlag = flag;
            ViewBag.UserDisplayName = db.Users.FirstOrDefault(u => u.Id == ticketComment.UserId).DisplayName;
            ViewBag.TicketTitle = db.Tickets.FirstOrDefault(t => t.Id == ticketComment.TicketId).Title;
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        [NoDirectAccess]
        public ActionResult Delete(int? id, bool flag)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var commentId = (int)id;
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (User.IsInRole("Admin") || validator.PMProjectTicket(ticketComment.Ticket.Id) || validator.CommentOwner(commentId))
            {
                if (ticketComment == null)
                {
                    return HttpNotFound();
                }
                ViewBag.UserIndexFlag = flag;
                return View(ticketComment);
            }
            return RedirectToAction("UserIndex", "Tickets");
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, bool flag)
        {
            TicketComment ticketComment = db.TicketComments.Find(id);
            db.TicketComments.Remove(ticketComment);
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
