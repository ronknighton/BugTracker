using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BugTracker.Helpers
{
    public class ProjectsHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();

        public bool AnyOpenTickets(int projectId)
        {
            return db.Tickets.Any(t => t.ProjectId == projectId && t.TicketStatus.Name != "Closed" && t.TicketStatus.Name != "Deleted" && t.TicketStatus.Name != "Resolved");
        }

        public bool IsUserOnProject(string userId, int projectId)
        {
            var project = db.Projects.Find(projectId);
            var flag = project.ProjectUsers.Any(u => u.Id == userId);
            return (flag);
        }

        public ICollection<Project> ListUserProjects(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);
            var projects = user.Projects.Where(p => p.Archived == false).ToList();
            return (projects);
        }

        public void AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                Project proj = db.Projects.Find(projectId);
                var newUser = db.Users.Find(userId);
                proj.ProjectUsers.Add(newUser);
                db.SaveChanges();
            }
        }

        public void RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                Project proj = db.Projects.Find(projectId);
                var delUser = db.Users.Find(userId);
                proj.ProjectUsers.Remove(delUser);
                db.Entry(proj).State = EntityState.Modified; // just saves this obj instance.
                db.SaveChanges();
            }
        }

        public ICollection<ApplicationUser> UsersOnProject(int projectId)
        {
            return db.Projects.Find(projectId).ProjectUsers;
        }

        public ICollection<ApplicationUser> UsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

        private void ClearDB()
        {
            var projects = db.Projects;
            var tickets = db.Tickets;
            var ticketComments = db.TicketComments;
            var ticketAttachments = db.TicketAttachments;
            var ticketHistories = db.TicketHistories;
            var ticketNotifications = db.TicketNotifications;

            foreach (var project in projects)
            {
                db.Projects.Remove(project);
            }

            foreach (var ticket in tickets)
            {
                db.Tickets.Remove(ticket);
            }

            foreach (var ticketComment in ticketComments)
            {
                db.TicketComments.Remove(ticketComment);
            }

            foreach (var ticketAttachment in ticketAttachments)
            {
                db.TicketAttachments.Remove(ticketAttachment);
            }

            foreach (var ticketHistory in ticketHistories)
            {
                db.TicketHistories.Remove(ticketHistory);
            }

            foreach (var ticketNotification in ticketNotifications)
            {
                db.TicketNotifications.Remove(ticketNotification);
            }

            db.SaveChanges();
        }


        public void ProjectsTicketsCreator()
        {

            ClearDB();

            var projectManager = db.Users.FirstOrDefault(pm => pm.UserName == "guestprojectmanager@mailinator.com");
            var developer = db.Users.FirstOrDefault(pm => pm.UserName == "guestdeveloper@mailinator.com");
            var submitter = db.Users.FirstOrDefault(pm => pm.UserName == "guestsubmitter@mailinator.com");
            Random rnd = new Random();

            for (int p = 1; p < 11; p++)
            {
                //Create New Project
                var project = new Project();
                project.Name = "Auto-Gen Project " + p;
                project.Description = "Automatically generated project for test purposes only";
                project.ProjectUsers.Add(projectManager);
                project.ProjectUsers.Add(developer);
                project.ProjectUsers.Add(submitter);

                db.Projects.Add(project);
                db.SaveChanges();

                //Create Tickets for project
                for (int t = 1; t <= 10; t++)
                {
                    var ticket = new Ticket();
                    ticket.Title = "Auto-Gen Ticket " + t + " for " + project.Name; ;
                    ticket.Description = "Automatically generated ticket for test purposes only";
                    ticket.Created = DateTimeOffset.Now;
                    ticket.ProjectId = project.Id;
                    ticket.OwnerUserId = submitter.Id;
                    ticket.AssignedToUserId = developer.Id;

                    //Assign Ticket Status
                    int pickStatus = rnd.Next(1, 5);
                    var ticketStatus = new TicketStatus();

                    switch (pickStatus)
                    {
                        case 1:
                            ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Open/Assigned");
                            break;
                        case 2:
                            ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Resolved");
                            break;
                        case 3:
                            ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Waiting For Info");
                            break;
                        case 4:
                            ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Closed");
                            break;

                    }

                    ticket.TicketStatusId = ticketStatus.Id;

                    //Assign ticket Priority
                    int pickPriority = rnd.Next(1, 6);
                    var ticketPriority = new TicketPriority();

                    switch (pickPriority)
                    {
                        case 1:
                            ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Critical");
                            break;
                        case 2:
                            ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Higher");
                            break;
                        case 3:
                            ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "High");
                            break;
                        case 4:
                            ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Medium");
                            break;
                        case 5:
                            ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Low");
                            break;
                    }

                    ticket.TicketPriorityId = ticketPriority.Id;

                    //Assign Ticket Type

                    int pickType = rnd.Next(1, 6);
                    var ticketType = new TicketType();

                    switch (pickType)
                    {
                        case 1:
                            ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Bug");
                            break;
                        case 2:
                            ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Task");
                            break;
                        case 3:
                            ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Informational");
                            break;
                        case 4:
                            ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Feature Request");
                            break;
                        case 5:
                            ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Call For Documentation");
                            break;
                    }

                    ticket.TicketTypeId = ticketType.Id;

                    db.Tickets.Add(ticket);
                    db.SaveChanges();

                    for (int ta = 1; ta < 6; ta++)
                    {
                        int pickAttach = rnd.Next(1, 6);
                        var ticketAttachment = new TicketAttachment();

                        ticketAttachment.TicketId = ticket.Id;
                        ticketAttachment.Description = "Auto-Gen Ticket Attachment " + ta + ", " + ticket.Description;
                        ticketAttachment.Created = DateTimeOffset.Now;
                        ticketAttachment.UserId = developer.Id;

                        switch (pickAttach)
                        {
                            case 1:
                                ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                break;
                            case 2:
                                ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week2.pdf";
                                break;
                            case 3:
                                ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week3.pdf";
                                break;
                            case 4:
                                ticketAttachment.MediaURL = "/Assets/attachments/EMS Instructions.pdf";
                                break;
                            case 5:
                                ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week3-Final.pdf";
                                break;
                        }

                        db.TicketAttachments.Add(ticketAttachment);
                        db.SaveChanges();
                    }

                    for (int tc = 1; tc < 6; tc++)
                    {
                        int pickComment = rnd.Next(1, 6);
                        var ticketComment = new TicketComment();

                        ticketComment.TicketId = ticket.Id;
                        ticketComment.Comment = "Auto-Generated Ticket Comment " + tc + ", " + ticket.Description;
                        ticketComment.Created = DateTimeOffset.Now;
                        ticketComment.UserId = developer.Id;



                        db.TicketComments.Add(ticketComment);
                        db.SaveChanges();

                    }
                }
            }

        }

      

        //Helper Methods
        private async Task ClearDBAsync()
        {
            try
            {
                var projects = db.Projects;
                var tickets = db.Tickets;
                var ticketComments = db.TicketComments;
                var ticketAttachments = db.TicketAttachments;
                var ticketHistories = db.TicketHistories;
                var ticketNotifications = db.TicketNotifications;

                var step1 = Task.Run(() =>
                {
                    foreach (var project in projects)
                    {
                        db.Projects.Remove(project);
                    }
                });

                var step2 = Task.Run(() =>
                {
                    foreach (var ticket in tickets)
                    {
                        db.Tickets.Remove(ticket);
                    }
                });

                var step3 = Task.Run(() =>
                {
                    foreach (var ticketComment in ticketComments)
                    {
                        db.TicketComments.Remove(ticketComment);
                    }
                });

                var step4 = Task.Run(() =>
                {
                    foreach (var ticketAttachment in ticketAttachments)
                    {
                        db.TicketAttachments.Remove(ticketAttachment);
                    }
                });

                var step5 = Task.Run(() =>
                {
                    foreach (var ticketHistory in ticketHistories)
                    {
                        db.TicketHistories.Remove(ticketHistory);
                    }
                });

                var step6 = Task.Run(() =>
                {
                    foreach (var ticketNotification in ticketNotifications)
                    {
                        db.TicketNotifications.Remove(ticketNotification);
                    }
                });

                step1.Wait();
                step2.Wait();
                step3.Wait();
                step4.Wait();
                step5.Wait();
                step6.Wait();

                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Task.FromResult(0);
            }
        }

        public async Task ProjectsTicketsCreatorAsync()
        {
            try
            {
                await ClearDBAsync();

                var projectManager = db.Users.FirstOrDefault(pm => pm.UserName == "guestprojectmanager@mailinator.com");
                var developer = db.Users.FirstOrDefault(pm => pm.UserName == "guestdeveloper@mailinator.com");
                var submitter = db.Users.FirstOrDefault(pm => pm.UserName == "guestsubmitter@mailinator.com");
                Random rnd = new Random();

                var doWork = Task.Run(() =>
                {
                    for (int p = 1; p <= 10; p++)
                    {
                        //Create New Project
                        var project = new Project();
                        project.Name = "Auto-Gen Project " + p;
                        project.Description = "Automatically generated project for test purposes only";
                        project.ProjectUsers.Add(projectManager);
                        project.ProjectUsers.Add(developer);
                        project.ProjectUsers.Add(submitter);

                        db.Projects.Add(project);
                        db.SaveChanges();

                        //Create Tickets for project
                        for (int t = 1; t <= 10; t++)
                        {
                            var ticket = new Ticket();
                            ticket.Title = "Auto-Gen Ticket " + t + " for " + project.Name; ;
                            ticket.Description = "Automatically generated ticket for test purposes only";
                            ticket.Created = DateTimeOffset.Now;
                            ticket.ProjectId = project.Id;
                            ticket.OwnerUserId = submitter.Id;
                            ticket.AssignedToUserId = developer.Id;

                            //Assign Ticket Status
                            int pickStatus = rnd.Next(1, 5);
                            var ticketStatus = new TicketStatus();

                            switch (pickStatus)
                            {
                                case 1:
                                    ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Open/Assigned");
                                    break;
                                case 2:
                                    ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Resolved");
                                    break;
                                case 3:
                                    ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Waiting For Info");
                                    break;
                                case 4:
                                    ticketStatus = db.TicketStatuses.FirstOrDefault(ts => ts.Name == "Closed");
                                    break;

                            }

                            ticket.TicketStatusId = ticketStatus.Id;

                            //Assign ticket Priority
                            int pickPriority = rnd.Next(1, 6);
                            var ticketPriority = new TicketPriority();

                            switch (pickPriority)
                            {
                                case 1:
                                    ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Critical");
                                    break;
                                case 2:
                                    ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Higher");
                                    break;
                                case 3:
                                    ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "High");
                                    break;
                                case 4:
                                    ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Medium");
                                    break;
                                case 5:
                                    ticketPriority = db.TicketPriorities.FirstOrDefault(ts => ts.Name == "Low");
                                    break;
                            }

                            ticket.TicketPriorityId = ticketPriority.Id;

                            //Assign Ticket Type

                            int pickType = rnd.Next(1, 6);
                            var ticketType = new TicketType();

                            switch (pickType)
                            {
                                case 1:
                                    ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Bug");
                                    break;
                                case 2:
                                    ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Task");
                                    break;
                                case 3:
                                    ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Informational");
                                    break;
                                case 4:
                                    ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Feature Request");
                                    break;
                                case 5:
                                    ticketType = db.TicketTypes.FirstOrDefault(ts => ts.Name == "Call For Documentation");
                                    break;
                            }

                            ticket.TicketTypeId = ticketType.Id;

                            db.Tickets.Add(ticket);
                            db.SaveChanges();

                            for (int ta = 1; ta <= 10; ta++)
                            {
                                int pickAttach = rnd.Next(1, 6);
                                var ticketAttachment = new TicketAttachment();

                                ticketAttachment.TicketId = ticket.Id;
                                ticketAttachment.Description = "Auto-Gen Ticket Attachment " + ta + ", " + ticket.Description;
                                ticketAttachment.Created = DateTimeOffset.Now;
                                ticketAttachment.UserId = developer.Id;

                                switch (pickAttach)
                                {
                                    case 1:
                                        ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                        break;
                                    case 2:
                                        ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                        break;
                                    case 3:
                                        ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                        break;
                                    case 4:
                                        ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                        break;
                                    case 5:
                                        ticketAttachment.MediaURL = "/Assets/attachments/BugTracker-Deliverables-Week1.pdf";
                                        break;
                                }

                                db.TicketAttachments.Add(ticketAttachment);
                                db.SaveChanges();
                            }

                            for (int tc = 1; tc <= 10; tc++)
                            {
                                int pickComment = rnd.Next(1, 6);
                                var ticketComment = new TicketComment();

                                ticketComment.TicketId = ticket.Id;
                                ticketComment.Comment = "Auto-Generated Ticket Comment " + tc + ", " + ticket.Description;
                                ticketComment.Created = DateTimeOffset.Now;
                                ticketComment.UserId = developer.Id;



                                db.TicketComments.Add(ticketComment);
                                db.SaveChanges();

                            }
                        }
                    }
                });

                doWork.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await Task.FromResult(0);
            }
        }
    }
}

