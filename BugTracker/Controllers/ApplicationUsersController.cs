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
    public class ApplicationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper userHelper = new UserRolesHelper();

        // GET: ApplicationUsers

        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: ApplicationUsers/Details/5
        [NoDirectAccess]
        //[Authorize(Roles = "Admin")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Create
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,DisplayName,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(applicationUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(applicationUser);
        }

        //GET change user Display Name or Image
        [NoDirectAccess]
        public ActionResult ChangeUser()
        {
            ApplicationUser applicationUser = db.Users.Find(User.Identity.GetUserId());
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        //User change Display Name or Profile Image
        
        [HttpPost]
        public ActionResult ChangeUser(string userId, string DisplayName, HttpPostedFileBase image)
        {
            var user = db.Users.Find(userId);
           
            if (user.Protected == true)
            {
                ViewBag.AdminError = "Can't change protected user.";
                return View(user);
            }

            #region Check Image
            if (image != null)
            {
                var fileValidator = new FileUploadValidator();
                var fileUrl = fileValidator.ValidateAndSaveFile3(image);
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
                    ApplicationUser applicationUser = db.Users.Find(User.Identity.GetUserId());
                    if (applicationUser == null)
                    {
                        return HttpNotFound();
                    }
                    return View(applicationUser);
                    
                }

                user.DisplayImage = fileUrl;
            }
            #endregion
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                if(DisplayName.Trim().Length < 5)
                {
                    TempData["DisplayNameError"] = "Display name must be greater than 5 characters";
                    ApplicationUser applicationUser = db.Users.Find(User.Identity.GetUserId());
                    if (applicationUser == null)
                    {
                        return HttpNotFound();
                    }
                    return View(applicationUser);
                }

                user.DisplayName = DisplayName;
            }

            //db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // GET: ApplicationUsers/Edit/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            if (applicationUser.Protected == true)
            {
                ViewBag.AdminError = "Can't change protected user.";
                return View(applicationUser);
            }
            return View(applicationUser);
        }

        // POST: ApplicationUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,DisplayName,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,DisplayImage")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                applicationUser.UserName = applicationUser.Email;
                db.Entry(applicationUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(applicationUser);
        }

        // GET: ApplicationUsers/Delete/5
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
           
            if (applicationUser.Protected == true)
            {
                ViewBag.AdminError = "Can't delete protected user.";
                return View(applicationUser);
            }
            var userId = applicationUser.Id;
            if (!userHelper.IsUserInRole(userId, "Admin"))
            {
                return View(applicationUser);
            }
            else if (userHelper.IsUserInRole(userId, "Admin") && userHelper.UsersInRole("Admin").Count <= 1)
            {
                return View(applicationUser);
            }
            else
            {
                ViewBag.AdminError = "Can't remove Admin without adding another first.";
                return View(applicationUser);
            }
            //return View(applicationUser);
        }

        // POST: ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);

            db.Users.Remove(applicationUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //Allow Admin to assign roles to users
        [NoDirectAccess]
        [Authorize(Roles = "Admin")]
        public ActionResult AssignRoles(string id)
        {

            var user = db.Users.Find(id);
            var currentuserRoles = userHelper.ListUserRoles(id);
            var allUserRoles = db.Roles.Select(r => r.Name).ToList();
            //ViewBag.userRoles = new MultiSelectList(allUserRoles, currentuserRoles);
            ViewBag.userRoles = new SelectList(allUserRoles, currentuserRoles);
            return View(user);
        }

        [HttpPost]
        public ActionResult AssignRoles(string id, List<string> userRoles)
        {
            //Unassign the user from all roles
            var user = db.Users.Find(id);
            if (user.Protected == true)
            {
                ViewBag.AdminError = "Can't change protected user.";
                return View(user);
            }
            else
            {
                foreach (var role in userHelper.ListUserRoles(id))
                {
                    if (role != "Admin")
                    {
                        userHelper.RemoveUserFromRole(id, role);
                    }
                    else if ((role == "Admin" && userHelper.UsersInRole("Admin").Count > 1) || (userRoles != null && userRoles.Contains("Admin")))
                    {
                        userHelper.RemoveUserFromRole(id, role);
                    }
                    else
                    {
                        ViewBag.AdminError = "Can't remove Admin without adding another first.";
                        return View(user);
                    }
                }

                if (userRoles != null)
                {
                    //Assign user to selected roles
                    foreach (var role in userRoles)
                    {
                        userHelper.AddUserToRole(id, role);
                    }
                }
            }

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
