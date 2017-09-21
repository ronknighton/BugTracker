using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BugTracker.Controllers;

namespace BugTracker.Helpers
{
    public class NotificationHistoryHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
     
        public void ProjectArchivedHistory(int projectId, bool archived)
        {
            var project = db.Projects.FirstOrDefault(p => p.Id == projectId);

            var newValue = "";
            var oldValue = "";

            if (archived)
            {
                newValue = "Archived";
                oldValue = "Un-Archive";
            }
            if (!archived)
            {
                newValue = "Un-Archive";
                oldValue = "Archived";
            }

            
            if (project != null)
            {
                var user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());                

                if(project.Tickets.Count > 0)
                {
                    foreach(var ticket in project.Tickets)
                    {
                        var history = new TicketHistory();
                        history.Property = "Archived";
                        history.OldValue = oldValue;
                        history.NewValue = newValue;
                        history.DateChanged = DateTimeOffset.Now;
                        history.Archived = false;
                        history.UserId = user.Id;
                        history.TicketId = ticket.Id;
                        db.TicketHistories.Add(history);
                       

                        var notification = new TicketNotification();
                        notification.UserId = ticket.AssignedToUserId;
                        notification.TicketId = ticket.Id;
                        notification.Created = history.DateChanged;
                        notification.NotifyReason = ticket.Title +  " has been " + newValue + " by " + user.DisplayName;
                        notification.ChangedBy = user.Id;

                        db.TicketNotifications.Add(notification);
                        db.SaveChanges();
                    }
                }

            } 
        }

        public async Task HistoryCreator(Ticket oldTicket, Ticket newTicket, string userId)
        {
            //Has the Ticket Title changed?
            if (!string.Equals(oldTicket.Title.ToLower(), newTicket.Title.ToLower()))
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Title";
                history.OldValue = oldTicket.Title;
                history.NewValue = newTicket.Title;
                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;
                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE TO: Ticket Title" + "\n FROM: " + oldTicket.Title + "\n TO: " + newTicket.Title + "\n BY: " + user.DisplayName;
                NotificationCreator(reason, newTicket, history.DateChanged, newTicket.AssignedToUserId);
            }

            //Has the Ticket Description changed?
            if (!string.Equals(oldTicket.Description.ToLower(), newTicket.Description.ToLower()))
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Description";
                history.OldValue = oldTicket.Description;
                history.NewValue = newTicket.Description;
                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;
                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE TO: Ticket Description" + "\n FROM: " + oldTicket.Description + "\n TO: " + newTicket.Description + "\n BY: " + user.DisplayName;
                NotificationCreator(reason, newTicket, history.DateChanged, newTicket.AssignedToUserId);
            }

            //Has the Ticket Type changed?
            if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Type";
                history.OldValue = oldTicket.TicketType.Name;

                var newTicketType = db.TicketTypes.FirstOrDefault(tp => tp.Id == newTicket.TicketTypeId);
                history.NewValue = newTicketType.Name;

                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;
                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE TO: Tickect Type" + "\n FROM: " + oldTicket.TicketType.Name + "\n TO: " + newTicketType.Name + "\n BY: " + user.DisplayName;
                NotificationCreator(reason, newTicket, history.DateChanged, newTicket.AssignedToUserId);
            }

            //Has the Ticket Priority changed?
            if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Priority";
                history.OldValue = oldTicket.TicketPriority.Name;

                var newTicketPriority = db.TicketPriorities.FirstOrDefault(tp => tp.Id == newTicket.TicketPriorityId);
                history.NewValue = newTicketPriority.Name;
               
                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;
                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE: Tickect Priority" + "\n FROM: " + oldTicket.TicketPriority.Name + "\n TO: " + newTicketPriority.Name + "\n BY: " + user.DisplayName;
                NotificationCreator(reason, newTicket, history.DateChanged, newTicket.AssignedToUserId);
            }

            //Has the Ticket Status changed?
            if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Status";
                history.OldValue = oldTicket.TicketStatus.Name;

                var newTicketStatus = db.TicketTypes.FirstOrDefault(tp => tp.Id == newTicket.TicketStatusId);
                history.NewValue = newTicketStatus.Name;
               
                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;
                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE: Tickect Status" + "\n FROM: " + oldTicket.TicketStatus.Name + "\n TO: " + newTicketStatus.Name + "\n BY: " + user.DisplayName;
                NotificationCreator(reason, newTicket, history.DateChanged, newTicket.AssignedToUserId);
            }

            //Has the Ticket Assigned-To user changed?
            if (oldTicket.AssignedToUserId == null)
            {
                var user = db.Users.FirstOrDefault(u => u.DisplayName == "UnassignedUser");
                oldTicket.AssignedToUserId = user.Id;
            }
            if (!string.Equals(oldTicket.AssignedToUserId, newTicket.AssignedToUserId))
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                var history = new TicketHistory();
                history.Property = "Ticket Assigned to User";
                history.OldValue = oldTicket.AssignedToUser.DisplayName;
                var newUser = db.Users.FirstOrDefault(u => u.Id == newTicket.AssignedToUserId);
                history.NewValue = newUser.DisplayName;
                history.DateChanged = DateTimeOffset.Now;
                history.TicketId = newTicket.Id;
                history.UserId = userId;

                db.TicketHistories.Add(history);
                db.SaveChanges();
                string reason = "CHANGE TO: Assigned to user" + "\n FROM: " + oldTicket.AssignedToUser.DisplayName + "\n TO: " + newUser.DisplayName + "\n BY: " + user.DisplayName;
                await AssignmentNotificationCreator(reason, newTicket, history.DateChanged, oldTicket.AssignedToUserId, newTicket.AssignedToUserId);
            }

        }

        public void NotificationCreator(string reason, Ticket ticket, DateTimeOffset created, string userId)
        {
            var changedByUser = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var notification = new TicketNotification();
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            notification.NotifyReason = reason;
            notification.TicketId = ticket.Id;
            notification.Created = created;
            notification.UserId = userId;
            notification.ChangedBy = changedByUser.Id;
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }


        public async Task AssignmentNotificationCreator(string reason, Ticket ticket, DateTimeOffset created, string oldUserId, string newUserId)
        {
            var notification = new TicketNotification();
            var changedByUser = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            var oldAssignedTo = db.Users.FirstOrDefault(u => u.Id == oldUserId);
            var newAssignedTo = db.Users.FirstOrDefault(u => u.Id == oldUserId);
            notification.NotifyReason = reason;
            notification.TicketId = ticket.Id;
            notification.Created = created;
            notification.UserId = newUserId;
            notification.ChangedBy = changedByUser.Id;
            db.TicketNotifications.Add(notification);
            db.SaveChanges();

            var notification2 = new TicketNotification();
            notification2.NotifyReason = reason;
            notification2.TicketId = ticket.Id;
            notification2.Created = created;
            notification2.UserId = oldUserId;
            notification2.ChangedBy = changedByUser.Id;
            db.TicketNotifications.Add(notification2);
            db.SaveChanges();
            //Send email to user
            await NotificationEmailCreator(notification, oldUserId, newUserId);
        }



        public async Task NotificationEmailCreator(TicketNotification notification, string oldUserId, string newUserId)
        {
            UserHelper userHelper = new UserHelper();
            var oldUserEmail = userHelper.GetUserEmailFromId(oldUserId);
            var newUserEmail = userHelper.GetUserEmailFromId(newUserId);

            var notificationMessage = new NotifcationMessage();

            if (!string.IsNullOrEmpty(oldUserId))
            {
                //Send email to old user
                notificationMessage.RecipientEmail = oldUserEmail;
                notificationMessage.Body = notification.NotifyReason;
                await EmailHelper.SendNotificationEmailAsync(notificationMessage);

            }

            if (!string.IsNullOrEmpty(newUserId))
            {
                //Send email to old user
                notificationMessage.RecipientEmail = newUserEmail;
                notificationMessage.Body = notification.NotifyReason;
                await EmailHelper.SendNotificationEmailAsync(notificationMessage);

            }


        }

     


    }
}