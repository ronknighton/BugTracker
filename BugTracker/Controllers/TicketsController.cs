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
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketsHelper ticketsHelper = new TicketsHelper();
        private ProjectsHelper projectsHelper = new ProjectsHelper();
        private AccessValidator validator = new AccessValidator();
        private UserRolesHelper rolesHelper = new UserRolesHelper();
        private NotificationHistoryHelper NHHelper = new NotificationHistoryHelper();

        // GET: Tickets
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            ViewBag.UserIndexFlag = false;
            ViewBag.UserId = user.Id;
            return View(tickets.ToList());
        }

        [NoDirectAccess]
        public ActionResult UserIndex()
        {
            var ticketList = new List<Ticket>();
            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Project Manager"))
            {
                ticketList = ticketsHelper.MyProjectTickets(user.Id).ToList();
            }
            if (User.IsInRole("Developer"))
            {
                ticketList = db.Tickets.Where(t => t.AssignedToUserId == user.Id).ToList();
            }
            if (User.IsInRole("Submitter"))
            {
                ticketList = ticketsHelper.OwnedTickets(user.Id).ToList();
            }
           
           
            ViewBag.UserIndexFlag = true;
            ViewBag.UserId = user.Id;
            return View("Index", ticketList);
        }

        [NoDirectAccess]
        public ActionResult _PartialTickets()
        {
            var ticketList = new List<Ticket>();
            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Project Manager") || User.IsInRole("Developer"))
            {
                ticketList = ticketsHelper.MyProjectTickets(user.Id).ToList();
            }
            if (User.IsInRole("Submitter"))
            {
                ticketList = ticketsHelper.OwnedTickets(user.Id).ToList();
            }


            ViewBag.UserIndexFlag = true;
            ViewBag.UserId = user.Id;
            return PartialView(ticketList);
        }

        [NoDirectAccess]
        public ActionResult _PartialAdminTickets()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var tickets = db.Tickets.Include(t => t.AssignedToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            ViewBag.UserIndexFlag = false;
            ViewBag.UserId = user.Id;
            return PartialView(tickets.ToList());
        }

        [NoDirectAccess]
        public ActionResult AssignedTickets(string userId)
        {
            var ticketList = new List<Ticket>();
           
            var tickets = db.Tickets.Where(t => t.AssignedToUserId == userId).ToList();
            ViewBag.UserIndexFlag = true;
            return View("Index", tickets);
        }

        [NoDirectAccess]
        public ActionResult OwnedTickets(string userId)
        {
            var ticketList = new List<Ticket>();

            var tickets = db.Tickets.Where(t => t.OwnerUserId == userId).ToList();
            ViewBag.UserIndexFlag = true;
            return View("Index", tickets);
        }

        [NoDirectAccess]
        public ActionResult ProjectTickets(int projectId)
        {
            var tickets = ticketsHelper.GetProjectTickets(projectId);            
            ViewBag.UserIndexFlag = true;
            return View("Index", tickets);
        }

        // GET: Tickets/Details/5
        [NoDirectAccess]
        public ActionResult Details(int? id, bool flag)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserIndexFlag = flag;
            return View(ticket);
        }

        // GET: Tickets/Create
        [NoDirectAccess]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(int projectId, bool flag)
        {
            if (validator.ProjectSubmitter(projectId))
            {

                var project = db.Projects.FirstOrDefault(p => p.Id == projectId);
                ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "DisplayName");
                ViewBag.ProjectId = projectId;
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
                ViewBag.ProjectName = project.Name;
                ViewBag.UserIndexFlag = flag;
                return View();
            }
            return RedirectToAction("UserIndex", "Projects");
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] Ticket ticket, string cBody, string attachDesc, bool flag, HttpPostedFileBase attachment)
        {

            if (ModelState.IsValid)
            {
                //if ()
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.Created = DateTimeOffset.Now;
                var ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Open/Unassigned");
                ticket.TicketStatusId = ticketStatus.Id;
                var assignedToUser = db.Users.FirstOrDefault(u => u.DisplayName == "UnassignedUser");
                ticket.AssignedToUserId = assignedToUser.Id;
                var comment = new TicketComment();
                var ticketAttachment = new TicketAttachment();
               

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
                        ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "DisplayName", ticket.OwnerUserId);
                        ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
                        ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                        ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                        ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
                        ViewBag.UserIndexFlag = flag;
                        return View(ticket);
                    }
                    
                    ticketAttachment.MediaURL = fileUrl;
                    ticketAttachment.Created = DateTimeOffset.Now;
                    ticketAttachment.Description = attachDesc;
                    //ticketAttachment.TicketId = ticket.Id;
                    ticketAttachment.UserId = User.Identity.GetUserId();
                    db.TicketAttachments.Add(ticketAttachment);
                    

                }

                if (!string.IsNullOrWhiteSpace(cBody))
                {
                    if (cBody.Length < 10)
                    {
                        TempData["CommentError"] = "Comment must be at least 10 characters";
                        ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "DisplayName", ticket.OwnerUserId);
                        ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
                        ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                        ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                        ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
                        ViewBag.UserIndexFlag = flag;
                        return View(ticket);
                    }
                    
                    comment.Comment = cBody;
                    comment.Created = DateTimeOffset.Now;
                    comment.UserId = User.Identity.GetUserId();
                    db.TicketComments.Add(comment);
                   

                }
                db.Tickets.Add(ticket);                
                db.SaveChanges();
                //Create a notification that a ticket has been added
                NHHelper.NotificationCreator("Ticket Created", ticket, ticket.Created, ticket.OwnerUserId);
                comment.TicketId = ticket.Id;

                //Create a notification that a comment has been added
                if (!string.IsNullOrWhiteSpace(comment.Comment))
                {
                    NHHelper.NotificationCreator("Comment Added", ticket, comment.Created, ticket.OwnerUserId);
                }

                ticketAttachment.TicketId = ticket.Id;

                //Create a notification that an attachment has been added
                if (!string.IsNullOrWhiteSpace(ticketAttachment.Description))
                {
                    NHHelper.NotificationCreator("Attachment Added", ticket, ticketAttachment.Created, ticket.OwnerUserId);
                }
                db.SaveChanges();

                if (flag)
                {
                    return RedirectToAction("UserIndex", "Projects");
                }
                return RedirectToAction("Index", "Projects");

            }
            //var devList = rolesHelper.UsersInRole("Developer");
            //ViewBag.AssignedToUserId = new SelectList(devList, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "DisplayName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.UserIndexFlag = flag;
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager, Submitter, Developer")]
        public ActionResult Edit(int? id, bool flag)
        {
            int ticketId = 0;

            if (id != null)
            {
                ticketId = (int)id;
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Ticket ticket = db.Tickets.Find(id);

            if (ticket == null)
            {
                return HttpNotFound();
            }

            if (User.IsInRole("Admin") || validator.DevTicketAssigned(ticketId) || validator.PMProjectTicket(ticketId) || validator.TicketOwner(ticketId))
            {
                var ticketOwner = db.Users.FirstOrDefault(u => u.Id == ticket.OwnerUserId);
                var project = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId);
                if (User.IsInRole("Admin") || validator.PMProjectTicket(ticketId))
                {
                    //var devList = rolesHelper.UsersInRole("Developer");
                    var role = db.Roles.FirstOrDefault(r => r.Name == "Developer");
                    var devList = ticket.Project.ProjectUsers.Where(u => u.Roles.Any(r => r.RoleId == role.Id)).ToList();
                    ViewBag.AssignedToUserId = new SelectList(devList, "Id", "DisplayName", ticket.AssignedToUserId);
                }
                else
                {
                    var assignedUser = db.Users.FirstOrDefault(u => u.Id == ticket.AssignedToUserId);
                    var assignedUserName = assignedUser.DisplayName;
                    ViewBag.AssignedToUserDisplayName = assignedUserName;
                }
                ViewBag.OwnerUser = ticketOwner.DisplayName;

                ViewBag.ProjectName = project.Name;
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
                ViewBag.UserIndexFlag = flag;
                return View(ticket);
            }

            return RedirectToAction("UserIndex");


        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId,Archived")] Ticket ticket, bool flag, HttpPostedFileBase attachment)
        {
            var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

            if (ModelState.IsValid)
            {
                
                var userId = User.Identity.GetUserId();
                
                ticket.Updated = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                //Check for changes to ticket and create notifications
                await NHHelper.HistoryCreator(oldTicket, ticket, userId);
                if (flag)
                {
                    return RedirectToAction("UserIndex");
                }
                return RedirectToAction("Index");

            }
            var devList = rolesHelper.UsersInRole("Developer");
            ViewBag.AssignedToUserId = new SelectList(devList, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.UserIndexFlag = flag;
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        //[NoDirectAccess]
        //[Authorize(Roles = "Admin, Project Manager, Submitter")]
        //public ActionResult Delete(int? id, bool flag)
        //{
        //    int ticketId = 0;

        //    if (id != null)
        //    {
        //        ticketId = (int)id;
        //    }
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (User.IsInRole("Admin") || validator.DevProjectTicket(ticketId) || validator.PMProjectTicket(ticketId) || validator.TicketOwner(ticketId))
        //    {
        //        ViewBag.UserIndexFlag = flag;
        //        return View(ticket);
        //    }
        //    ViewBag.UserIndexFlag = flag;
        //    if (flag)
        //    {
        //        return RedirectToAction("UserIndex");
        //    }
        //    return RedirectToAction("Index");

        //}

        //// POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id, bool flag)
        //{
        //    Ticket ticket = db.Tickets.Find(id);
        //    db.Tickets.Remove(ticket);
        //    db.SaveChanges();

        //    if (flag)
        //    {
        //        return RedirectToAction("UserIndex");
        //    }
        //    return RedirectToAction("Index");
        //}

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
