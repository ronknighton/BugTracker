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
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper userRole = new UserRolesHelper();
        private ProjectsHelper projectsHelper = new ProjectsHelper();
        private TicketsHelper ticketsHelper = new TicketsHelper();
        private AccessValidator validator = new AccessValidator();
        private NotificationHistoryHelper NHhelper = new NotificationHistoryHelper();

        // GET: Projects
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Index()
        {

            ViewBag.UserIndexFlag = false;
            ViewBag.UserId = User.Identity.GetUserId();
            return View(db.Projects.ToList());

        }
        [NoDirectAccess]
        public ActionResult UserIndex()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userProjects = projectsHelper.ListUserProjects(user.Id);
            ViewBag.UserIndexFlag = true;
            ViewBag.UserId = user.Id;            
            return View("Index", userProjects);
        }

        [NoDirectAccess]
        public ActionResult _PartialProjects()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var userProjects = projectsHelper.ListUserProjects(user.Id);
            ViewBag.UserIndexFlag = true;
            ViewBag.UserId = user.Id;
            return PartialView(userProjects);
        }

        [NoDirectAccess]
        public ActionResult _PartialAdminProjects()
        {
            ViewBag.UserIndexFlag = false;
            ViewBag.UserId = User.Identity.GetUserId();
            return PartialView(db.Projects.ToList());
        }

        [NoDirectAccess]
        public ActionResult AssignedProjects(string userId)
        {
            var projects = projectsHelper.ListUserProjects(userId);           
            ViewBag.UserIndexFlag = true;
            return View("Index", projects);
        }
        //Create auto-generated table data
        [NoDirectAccess]
        public ActionResult CreateTableData()
        {
            projectsHelper.ProjectsTicketsCreator();
            //await projectsHelper.ProjectsTicketsCreatorAsync();
            TempData["AlertMessage"] = "New Table Data has been created";
            return RedirectToAction("Index", "Projects");
        }
            

        // GET: Projects/Details/5
        [NoDirectAccess]
        public ActionResult Details(int? id, bool flag)
        {
            var projectId = 0;
            if (id != null)
            {
                projectId = (int)id;
            }
            if (User.IsInRole("Admin") || projectsHelper.IsUserOnProject(User.Identity.GetUserId(), projectId))
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Project project = db.Projects.Find(id);
                if (project == null)
                {
                    return HttpNotFound();
                }
                //var user = db.Users.Find(User.Identity.GetUserId());
                //ViewBag.HighestUserRole = userRole.GetHighestRole(user.Id);
                ViewBag.UserIndexFlag = flag;
                return View(project);
            }
            return RedirectToAction("UserIndex");
        }

        // GET: Projects/Create
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Create()
        {
           
                var userRole = new UserRolesHelper();
                var projectManagers = userRole.UsersInRole("Project Manager");
                var projectDevelopers = userRole.UsersInRole("Developer");
                var projectSubmitters = userRole.UsersInRole("Submitter");
                ViewBag.projectManagers = new SelectList(projectManagers, "Id", "DisplayName");
                ViewBag.projectDevelopers = new MultiSelectList(projectDevelopers, "Id", "DisplayName");
                ViewBag.projectSubmitters = new MultiSelectList(projectSubmitters, "Id", "DisplayName");
                return View();
            
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Project project, List<string> projectManagers, List<string> projectDevelopers, List<string> projectSubmitters)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser();
                #region Add Users to project
                if (projectManagers != null && projectManagers.Count > 0)
                {
                    foreach (var manager in projectManagers)
                    {
                        user = (db.Users.FirstOrDefault(u => u.Id == manager));
                        project.ProjectUsers.Add(user);
                    }
                }

                if (projectDevelopers != null && projectDevelopers.Count > 0)
                {
                    foreach (var manager in projectDevelopers)
                    {
                        user = (db.Users.FirstOrDefault(u => u.Id == manager));
                        project.ProjectUsers.Add(user);
                    }
                }

                if (projectSubmitters != null && projectSubmitters.Count > 0)
                {
                    foreach (var manager in projectSubmitters)
                    {
                        user = (db.Users.FirstOrDefault(u => u.Id == manager));
                        project.ProjectUsers.Add(user);
                    }
                }
                #endregion

                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
        {
            Project project = db.Projects.Find(id);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (project == null)
            {
                return HttpNotFound();
            }

            var allUsers = db.Users.ToList();
            var projectUsers = project.ProjectUsers.ToList();
            //var unusedUsers = allUsers.Except(projectUsers).ToList();

            //var currentProjectManagers = new List<ApplicationUser>();
            //var currentProjectDevelopers = new List<ApplicationUser>();
            //var currentProjectSubmitters = new List<ApplicationUser>();

            var currentProjectManagers = "";
            var currentProjectDevelopers = new List<string>();
            var currentProjectSubmitters = new List<string>();



            if (projectUsers.Count > 0)
            {

                foreach (var user in projectUsers)
                {
                    if (userRole.IsUserInRole(user.Id, "Project Manager"))
                    {
                        currentProjectManagers = user.Id;
                    }
                    if (userRole.IsUserInRole(user.Id, "Developer"))
                    {
                        currentProjectDevelopers.Add(user.Id);
                    }
                    if (userRole.IsUserInRole(user.Id, "Submitter"))
                    {
                        currentProjectSubmitters.Add(user.Id);
                    }
                }

                //ViewBag.projectManagers = new MultiSelectList(projectManagers, "Id", "DisplayName");
                //ViewBag.projectDevelopers = new MultiSelectList(projectDevelopers, "Id", "DisplayName");
                //ViewBag.projectSubmitters = new MultiSelectList(projectSubmitters, "Id", "DisplayName");


            }

            var allProjectManagers = new List<ApplicationUser>();
            var allProjectDevelopers = new List<ApplicationUser>();
            var allProjectSubmitters = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                if (userRole.IsUserInRole(user.Id, "Project Manager"))
                {
                    allProjectManagers.Add(user);
                }
                if (userRole.IsUserInRole(user.Id, "Developer"))
                {
                    allProjectDevelopers.Add(user);
                }
                if (userRole.IsUserInRole(user.Id, "Submitter"))
                {
                    allProjectSubmitters.Add(user);
                }
            }

            ViewBag.projectManagers = new SelectList(allProjectManagers, "Id", "DisplayName", currentProjectManagers);
            ViewBag.projectDevelopers = new MultiSelectList(allProjectDevelopers, "Id", "DisplayName", currentProjectDevelopers);
            ViewBag.projectSubmitters = new MultiSelectList(allProjectSubmitters, "Id", "DisplayName", currentProjectSubmitters);

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description")] Project project, List<string> projectManagers, List<string> projectDevelopers, List<string> projectSubmitters)
        {

            //var pUsers = projectManagers.Union(projectDevelopers);
            //var addProjectUsersIds = pUsers.Union(projectSubmitters);

            var oldProjectUsers = projectsHelper.UsersOnProject(project.Id).ToList();
            //var projectUsers = project.ProjectUsers.ToList();

            if (ModelState.IsValid)
            {
                foreach (var user in oldProjectUsers)
                {
                    projectsHelper.RemoveUserFromProject(user.Id, project.Id);
                }

                if (projectManagers != null && projectManagers.Count > 0)
                {
                    foreach (var userId in projectManagers)
                    {
                        projectsHelper.AddUserToProject(userId, project.Id);
                    }
                }

                if (projectDevelopers != null && projectDevelopers.Count > 0)
                {
                    foreach (var userId in projectDevelopers)
                    {
                        projectsHelper.AddUserToProject(userId, project.Id);
                    }
                }

                if (projectSubmitters != null && projectSubmitters.Count > 0)
                {
                    foreach (var userId in projectSubmitters)
                    {
                        projectsHelper.AddUserToProject(userId, project.Id);
                    }
                }


                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ViewBag.Archived = project.Archived;
            if (projectsHelper.AnyOpenTickets((int)id))
            {
                
                ViewBag.OpenTickets = "This project has open/unresolved tickets!";
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            project.Archived = true;

            //var tickets = ticketsHelper.GetProjectTickets(id);
            if(project.Tickets.Count > 0)
            {
                foreach(var ticket in project.Tickets)
                {
                    ticket.Archived = true;

                    foreach(var attachment in ticket.TicketAttachments)
                    {
                        attachment.Archived = true;
                    }

                    foreach(var comment in ticket.TicketComments)
                    {
                        comment.Archived = true;
                    }

                    foreach(var notification in ticket.TicketNotifications)
                    {
                        notification.Archived = true;
                    }

                    foreach(var history in ticket.TicketHistories)
                    {
                        history.Archived = true;
                    }
                }
            }

            //db.Projects.Remove(project);
            
            db.SaveChanges();
            NHhelper.ProjectArchivedHistory(id, true);

            return RedirectToAction("Index");
        }


        // GET: Projects/Delete/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult UnArchive(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            ViewBag.Archived = project.Archived;
          return View(project);
        }

        [HttpPost]
        [NoDirectAccess]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult UnArchive(int id)
        {
            Project project = db.Projects.Find(id);
            project.Archived = false;

            //var tickets = ticketsHelper.GetProjectTickets(id);
            if (project.Tickets.Count > 0)
            {
                foreach (var ticket in project.Tickets)
                {
                    ticket.Archived = false;

                    foreach (var attachment in ticket.TicketAttachments)
                    {
                        attachment.Archived = false;
                    }

                    foreach (var comment in ticket.TicketComments)
                    {
                        comment.Archived = false;
                    }

                    foreach (var notification in ticket.TicketNotifications)
                    {
                        notification.Archived = false;
                    }

                    foreach (var history in ticket.TicketHistories)
                    {
                        history.Archived = false;
                    }
                }
            }

            db.SaveChanges();
            NHhelper.ProjectArchivedHistory(id, false);
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
