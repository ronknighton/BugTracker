namespace BugTracker.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {

            // Creat instance of role manager
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            #region Roles
            //Check if role exists. If not, create it
            // Admin
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }

            // Project Manager
            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }

            //Developer
            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }

            // Submitter
            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }
            #endregion


            #region Create Users
            //Create instance of User manager
            var userManger = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            // Add Admin
            if (!context.Users.Any(u => u.Email == "emsadministrator@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "emsadministrator@mailinator.com",
                    Email = "emsadministrator@mailinator.com",
                    FirstName = "EMS",
                    LastName = "Administrator",
                    DisplayName = "EMS Administrator",
                    DisplayImage = "/Assets/images/profile-pics/3.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            var userId = userManger.FindByEmail("emsadministrator@mailinator.com").Id;
            userManger.AddToRole(userId, "Admin");
            //End Admin

            //Add Project Manager
            if (!context.Users.Any(u => u.Email == "emsprojectmanager@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "emsprojectmanager@mailinator.com",
                    Email = "emsprojectmanager@mailinator.com",
                    FirstName = "EMS",
                    LastName = "ProjectManager",
                    DisplayName = "EMS ProjectManager",
                    DisplayImage = "/Assets/images/profile-pics/4.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("emsprojectmanager@mailinator.com").Id;
            userManger.AddToRole(userId, "Project Manager");
            //End Project Manager

            //Add Developer
            if (!context.Users.Any(u => u.Email == "emsdeveloper@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "emsdeveloper@mailinator.com",
                    Email = "emsdeveloper@mailinator.com",
                    FirstName = "EMS",
                    LastName = "Developer",
                    DisplayName = "EMS Developer",
                    DisplayImage = "/Assets/images/profile-pics/2.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("emsdeveloper@mailinator.com").Id;
            userManger.AddToRole(userId, "Developer");
            //End Developer

            //Add Submitter
            if (!context.Users.Any(u => u.Email == "emssubmitter@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "emssubmitter@mailinator.com",
                    Email = "emssubmitter@mailinator.com",
                    FirstName = "EMS",
                    LastName = "Submitter",
                    DisplayName = "EMS Submitter",
                    DisplayImage = "/Assets/images/profile-pics/1.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("emssubmitter@mailinator.com").Id;
            userManger.AddToRole(userId, "Submitter");
            //End Submitter

            //Add Unassigned user
            if (!context.Users.Any(u => u.Email == "UnassignedUser@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "UnassignedUser@mailinator.com",
                    Email = "UnassignedUser@mailinator.com",
                    FirstName = "Unassigned",
                    LastName = "User",
                    DisplayName = "UnassignedUser",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("UnassignedUser@mailinator.com").Id;
            userManger.AddToRole(userId, "Developer");
            //End add unnassigned user

            // Add Guest Admin
            if (!context.Users.Any(u => u.Email == "guestadministrator@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "guestadministrator@mailinator.com",
                    Email = "guestadministrator@mailinator.com",
                    FirstName = "Guest",
                    LastName = "Administrator",
                    DisplayName = "Guest Administrator",
                    DisplayImage = "/Assets/images/profile-pics/3.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("guestadministrator@mailinator.com").Id;
            userManger.AddToRole(userId, "Admin");
            //End Guest Admin

            //Add Guest Project Manager
            if (!context.Users.Any(u => u.Email == "guestprojectmanager@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "guestprojectmanager@mailinator.com",
                    Email = "guestprojectmanager@mailinator.com",
                    FirstName = "Guest",
                    LastName = "ProjectManager",
                    DisplayName = "Guest ProjectManager",
                    DisplayImage = "/Assets/images/profile-pics/4.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("guestprojectmanager@mailinator.com").Id;
            userManger.AddToRole(userId, "Project Manager");
            //End Guest Project Manager

            //Add Guest Developer
            if (!context.Users.Any(u => u.Email == "guestdeveloper@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "guestdeveloper@mailinator.com",
                    Email = "guestdeveloper@mailinator.com",
                    FirstName = "Guest",
                    LastName = "Developer",
                    DisplayName = "Guest Developer",
                    DisplayImage = "/Assets/images/profile-pics/2.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("guestdeveloper@mailinator.com").Id;
            userManger.AddToRole(userId, "Developer");
            //End Developer

            //Add Submitter
            if (!context.Users.Any(u => u.Email == "guestsubmitter@mailinator.com"))
            {
                userManger.Create(new ApplicationUser
                {
                    UserName = "guestsubmitter@mailinator.com",
                    Email = "guestsubmitter@mailinator.com",
                    FirstName = "Guest",
                    LastName = "Submitter",
                    DisplayName = "Guest Submitter",
                    DisplayImage = "/Assets/images/profile-pics/1.jpg",
                    Protected = true
                }, "EmsAbc!23");
            }

            userId = userManger.FindByEmail("guestsubmitter@mailinator.com").Id;
            userManger.AddToRole(userId, "Submitter");
            //End Submitter
            #endregion

            #region Seed Tables
            context.TicketStatuses.AddOrUpdate(
               p => p.Name,
                   new TicketStatus { Name = "Open/Unassigned" },
                   new TicketStatus { Name = "Open/Assigned" },
                   new TicketStatus { Name = "Resolved" },
                   new TicketStatus { Name = "Waiting For Info" },
                   new TicketStatus { Name = "Closed" },
                   new TicketStatus { Name = "Deleted" }
           );

            context.TicketTypes.AddOrUpdate(
                p => p.Name,
                    new TicketType { Name = "Bug" },
                    new TicketType { Name = "Task" },
                    new TicketType { Name = "Informational" },
                    new TicketType { Name = "Feature Request" },
                    new TicketType { Name = "Call For Documentation" }
            );

            context.TicketPriorities.AddOrUpdate(
                p => p.Name,
                    new TicketPriority { Name = "Critical" },
                    new TicketPriority { Name = "Higher" },
                    new TicketPriority { Name = "High" },
                    new TicketPriority { Name = "Medium" },
                    new TicketPriority { Name = "Low" }
            );
            #endregion
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
