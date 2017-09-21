using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class Project
    {
       

        public int Id { get; set; }
        [DisplayName("Project Name")]
        public string Name { get; set; }
        [DisplayName("Project Description")]
        public string Description { get; set; }
        public bool Archived { get; set; }

        //Children
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<ApplicationUser> ProjectUsers { get; set; }

        public Project()
        {
            this.Tickets = new HashSet<Ticket>();
            this.ProjectUsers = new HashSet<ApplicationUser>();
        }
    }
}