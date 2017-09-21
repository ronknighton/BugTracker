using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class TicketsHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();
        ProjectsHelper projectsHelper = new ProjectsHelper();
        private UserRolesHelper userRoles = new UserRolesHelper();
        private ApplicationUser user;


        //Get tickets attached to a project
        public ICollection<Ticket> GetProjectTickets(int projectId)
        {
            return db.Tickets.Where(t => t.ProjectId == projectId).ToList();
        }

        //Assigned Tickets by User
        public ICollection<Ticket> AssignedTickets(string userId)
        {
            return db.Tickets.Where(t => t.AssignedToUser.Id == userId && t.Archived == false).ToList();
        }

        //Tickets by Owner
        public ICollection<Ticket> OwnedTickets(string userId)
        {
            return db.Tickets.Where(t => t.OwnerUser.Id == userId && t.Archived == false).ToList();
        }

        //User Tickets by Project
        public ICollection<Ticket> MyProjectTickets(string userId)
        {
            var userProjects = projectsHelper.ListUserProjects(userId);
            var projectTickets = new List<Ticket>();

                foreach (var projects in userProjects)
                {
                    foreach (var ticket in projects.Tickets)
                    {
                        projectTickets.Add(ticket);
                    }
                }
                return projectTickets;
           
        }

        public ICollection<Ticket> PMTickets(string userId)
        {
            var pmTickets = new List<Ticket>();
            if (userRoles.IsUserInRole(user.Id, "Project Manager"))
            {
                pmTickets = db.Tickets.Where(t => t.Project.ProjectUsers.Any(u => u.Id == userId) && t.Archived == false).ToList();
            }


            return pmTickets;

        }
        //Comments for a Ticket
        public ICollection<TicketComment> TicketComments(int ticketId)
        {
            return db.TicketComments.Where(c => c.TicketId == ticketId).ToList();
        }
        //Ticket Attachments for ticket
        public ICollection<TicketAttachment> TicketAttachments(int ticketId)
        {
            return db.TicketAttachments.Where(c => c.TicketId == ticketId).ToList();
        }

        //Notifications for a Ticket by User
        public ICollection<TicketNotification> UserTicketNotifications()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return new List<TicketNotification>();
            }
            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            return db.TicketNotifications.Where(t => t.UserId == user.Id || t.Ticket.AssignedToUserId == user.Id || t.Ticket.OwnerUserId == user.Id || t.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId)) && t.Archived == false).OrderByDescending(t => t.Created).ToList();
        }

        //Notifications for a Ticket by User
        public string UserTicketNotificationsUnread()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return 0.ToString();
            }
            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            //var count = db.TicketNotifications.Where(t => t.UserId == user.Id || t.Ticket.AssignedToUserId == user.Id || t.Ticket.OwnerUserId == user.Id || t.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId))).Where( t => t.HasBeenRead == false && t.Archived == false).ToList().Count;
            var count = db.TicketNotifications.Where(t => t.UserId == user.Id && t.Archived == false).ToList().Count;
            if (count > 99)
            {
                return "99+";
            }
            else return count.ToString();
        }

        public string UserTicketHistories()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return 0.ToString();
            }
           
            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var histories = db.TicketHistories.Where(th => th.Ticket.AssignedToUserId == user.Id || th.Ticket.OwnerUserId == user.Id || th.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId))).ToList();
            var activeHistories = histories.Where(h => h.Archived == false).ToList();
           
            var count = activeHistories.Count;
            if (count > 99)
            {
                return "99+";
            }
            else return count.ToString();
        }

        public ICollection<TicketHistory> GetUserTicketHistories()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return new List<TicketHistory>();
            }
          
            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var userTicketHistories = db.TicketHistories.Where(th => th.Ticket.AssignedToUserId == user.Id || th.Ticket.OwnerUserId == user.Id || th.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId)) && th.Archived == false).ToList();
            return userTicketHistories;
        }

        public ICollection<TicketAttachment> GetUserTicketAttachments()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return new List<TicketAttachment>();
            }

            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var userTicketAttachments = db.TicketAttachments.Where(th => th.Ticket.AssignedToUserId == user.Id || th.Ticket.OwnerUserId == user.Id || th.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId)) && th.Archived == false).ToList();
            return userTicketAttachments;
        }

        public ICollection<TicketComment> GetUserTicketComments()
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (user == null)
            {
                return new List<TicketComment>();
            }

            var roleId = db.Roles.FirstOrDefault(r => r.Name == "Project Manager").Id;
            var userTicketComments = db.TicketComments.Where(th => th.Ticket.AssignedToUserId == user.Id || th.Ticket.OwnerUserId == user.Id || th.Ticket.Project.ProjectUsers.Any(u => u.Id == user.Id && u.Roles.Any(r => r.RoleId == roleId)) && th.Archived == false).ToList();
            return userTicketComments;
        }


        public ICollection<TicketNotification> TicketNotifications(int ticketId)
        {
            return db.TicketNotifications.Where(t => t.TicketId == ticketId && t.Archived == false).ToList();
        }

        public ICollection<TicketHistory> TicketHistories(int ticketId)
        {
            return db.TicketHistories.Where(t => t.TicketId == ticketId && t.Archived == false).ToList();
        }

        //Has ticket been changed-Does not account for lag in populating objects associated with Id FK's
        public bool HasTicketChanged(Ticket oldTicket, Ticket newTicket)
        {
            return !oldTicket.Equals(newTicket);
        }

        public decimal GetOpenTickets()
        {
            decimal openPercent = 0.00m;
            int allTickets = db.Tickets.ToList().Count;
            int openTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            openPercent = ((decimal)openTickets / (decimal)allTickets) * 100;
            return Math.Round(openPercent);
        }

        public decimal GetCriticalTickets()
        {
            decimal criticalPercent = 0.00m;
            decimal allOpenTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            decimal criticalTickets = db.Tickets.Where(t => t.TicketPriority.Name == "Critical" && (t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted")).ToList().Count;
            criticalPercent = (criticalTickets / allOpenTickets) * 100;
            return Math.Round(criticalPercent);
        }

        public decimal GetHigherTickets()
        {
            decimal higherPercent = 0.00m;
            decimal allOpenTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            decimal higherTickets = db.Tickets.Where(t => t.TicketPriority.Name == "Higher" && (t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted")).ToList().Count;
            higherPercent = (higherTickets / allOpenTickets) * 100;
            return Math.Round(higherPercent);
        }
        public decimal GetHighTickets()
        {
            decimal highPercent = 0.00m;
            decimal allOpenTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            decimal highTickets = db.Tickets.Where(t => t.TicketPriority.Name == "High" && (t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted")).ToList().Count;
            highPercent = (highTickets / allOpenTickets) * 100;
            return Math.Round(highPercent);
        }

        public decimal GetMediumTickets()
        {
            decimal MediumPercent = 0.00m;
            decimal allOpenTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            decimal MediumTickets = db.Tickets.Where(t => t.TicketPriority.Name == "Medium" && (t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted")).ToList().Count;
            MediumPercent = (MediumTickets / allOpenTickets) * 100;
            return Math.Round(MediumPercent);
        }

        public decimal GetLowTickets()
        {
            decimal LowPercent = 0.00m;
            decimal allOpenTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            decimal LowTickets = db.Tickets.Where(t => t.TicketPriority.Name == "Low" && (t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted")).ToList().Count;
            LowPercent = (LowTickets / allOpenTickets) * 100;
            return Math.Round(LowPercent);
        }
        public decimal GetBugTickets()
        {
            decimal bugPercent = 0.00m;
            int allTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            int bugTickets = db.Tickets.Where(t => t.TicketType.Name == "Bug").ToList().Count;
            bugPercent = ((decimal)bugTickets / (decimal)allTickets) * 100;
            return Math.Round(bugPercent);
        }

        public decimal GetTaskTickets()
        {
            decimal taskPercent = 0.00m;
            int allTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            int taskTickets = db.Tickets.Where(t => t.TicketType.Name == "Task").ToList().Count;
            taskPercent = ((decimal)taskTickets / (decimal)allTickets) * 100;
            return Math.Round(taskPercent);
        }
        public decimal GetInfoTickets()
        {
            decimal infoPercent = 0.00m;
            int allTickets = db.Tickets.Where(t => t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted").ToList().Count;
            int infoTickets = db.Tickets.Where(t => t.TicketType.Name == "Informational").ToList().Count;
            infoPercent = ((decimal)infoTickets / (decimal)allTickets) * 100;
            return Math.Round(infoPercent);
        }

    }
}