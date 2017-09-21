using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        private ApplicationDbContext db = new ApplicationDbContext();

        //Is user in role true or false
        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }

        //List of user roles by user Id
        public ICollection<string> ListUserRoles(string userId)
        {
            return userManager.GetRoles(userId);
        }


        //Add a user to a role and return true if successful
        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }


        //Remove user form role and return tru if successful
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        //Gets the users in a specific role
        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }

        //Gets the users not in a role
        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }

        public string GetHighestRole (string userId)
        {
            var userRoles = ListUserRoles(userId);

            if (userRoles.Contains("Admin"))
            {
                return "Admin";
            }
            else if (userRoles.Contains("Project Manager"))
            {
                return "Project Manager";
            }
            else if (userRoles.Contains("Developer"))
            {
                return "Developer";
            }
            else if (userRoles.Contains("Submitter"))
            {
                return "Submitter";
            }
            else return "No Roles Assigned";
            
           
        }
    }
}