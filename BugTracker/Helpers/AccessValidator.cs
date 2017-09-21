using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class AccessValidator
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ProjectsHelper projHelper = new ProjectsHelper();
        private UserRolesHelper userRoles = new UserRolesHelper();
        private TicketsHelper ticketHelper = new TicketsHelper();
        private ApplicationUser user;

        public bool PMProject(int projId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var onProject = projHelper.IsUserOnProject(user.Id, projId);
            var isPM = userRoles.IsUserInRole(user.Id, "Project Manager");

            if (onProject && isPM)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PMProjectTicket(int ticketId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (userRoles.IsUserInRole(user.Id, "Project Manager"))
            {

                return db.Tickets.Any(t => t.Id == ticketId && t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                //var flag1 = db.Tickets.Any(t => t.Id == ticketId);
                //var flag2 = db.Tickets.Any(t => t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                //if (flag1 && flag2)
                //{
                //    return true;
                //}
                //else return false;

            }
            return false;

        }

        public bool TicketOwner(int ticketId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            //var ticketsOwned = ticketHelper.OwnedTickets(user.Id);
            //foreach (var ticket in ticketsOwned)
            //{
            //    if (ticket.OwnerUserId == user.Id && ticket.Id == ticketId)
            //    {
            //        return true;
            //    }
            //}
            //return false;

            if (userRoles.IsUserInRole(user.Id, "Submitter"))
            {
                var flag = db.Tickets.Any(t => t.Id == ticketId && t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                //return db.Tickets.Any(t => t.Id == ticketId && t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                //var flag1 = db.Tickets.Any(t => t.Id == ticketId);
                //var flag2 = db.Tickets.Any(t => t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                //if (flag1 && flag2)
                //{
                //    return true;
                //}
                return flag;

            }
            return false;
        }

        public bool DevProject(int projId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var onProject = projHelper.IsUserOnProject(user.Id, projId);
            var isDev = userRoles.IsUserInRole(user.Id, "Developer");

            if (onProject && isDev)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DevProjectTicket(int ticketId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (userRoles.IsUserInRole(user.Id, "Developer"))
            {
                return db.Tickets.Any(t => t.Id == ticketId && t.AssignedToUserId == user.Id);

            }
            return false;
        }

        public bool DevTicketAssigned(int ticketId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            if (userRoles.IsUserInRole(user.Id, "Developer"))
            {
                //return db.Tickets.Any(t => t.Id == ticketId && t.Project.ProjectUsers.Any(u => u.Id == user.Id));
                return db.Tickets.Any(t => t.AssignedToUserId == user.Id && t.Id == ticketId);
            }
            return false;
        }

        public bool CommentOwner(int comId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var comment = db.TicketComments.Any(tc => tc.UserId == user.Id && tc.Id == comId);
            return comment;
        }

        public bool ProjectSubmitter(int projId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var onProject = projHelper.IsUserOnProject(user.Id, projId);
            var isSub = userRoles.IsUserInRole(user.Id, "Submitter");

            if (onProject && isSub)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AttachmentOwner(int attchId)
        {
            user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            return db.TicketAttachments.Any(ta => ta.UserId == user.Id && ta.Id == attchId);
        }

        public static LoginViewModel GuestLogin(string guestUser)
        {
            var guestModel = new LoginViewModel();

            if (guestUser == "Admin")
            {
                guestModel.Email = "guestadministrator@mailinator.com";
                guestModel.Password = "EmsAbc!23";
                guestModel.RememberMe = false;
                return guestModel;

            }
            if (guestUser == "Developer")
            {
                guestModel.Email = "guestdeveloper@mailinator.com";
                guestModel.Password = "EmsAbc!23";
                guestModel.RememberMe = false;
                return guestModel;
            }
            if (guestUser == "Project Manager")
            {
                guestModel.Email = "guestprojectmanager@mailinator.com";
                guestModel.Password = "EmsAbc!23";
                guestModel.RememberMe = false;
                return guestModel;
            }
            if (guestUser == "Submitter")
            {
                guestModel.Email = "guestsubmitter@mailinator.com";
                guestModel.Password = "EmsAbc!23";
                guestModel.RememberMe = false;
                return guestModel;
            }

            return guestModel;
        }
    }
}