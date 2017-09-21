using BugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Helpers
{
    public class UserHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public string UserProfileImage()
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
                if (user.DisplayImage != null)
                {
                    return user.DisplayImage;
                }
            }
            return "/Assets/images/profile-pics/defaultProfileImage.jpg";


        }

        public string getUserDisplayName(string userId)
        {
            var user = db.Users.Find(userId);
            if (user != null)
            {
               
                return user.DisplayName;
            }
            return userId;
        }

        public string getUserDisplayImage (string userId)
        {
            var user = db.Users.Find(userId);
            if (user != null)
            {

                return user.DisplayImage;
            }
            return "~/Assets/images/profile-pics/einstein.jpg";
        }

        public string GetUserNameFromId(string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            var fullName = user.FirstName + " " + user.LastName;
            return fullName;
        }
        public string GetUserEmailFromId(string userId)
        {
            return db.Users.FirstOrDefault(u => u.Id == userId).Email;
        }

        public bool AreSameUserId(string userId)
        {
            var user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            return (userId == user.Id);
        }
    }
}