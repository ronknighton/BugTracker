using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [DisplayName("Ticket Comment")]
        public string Comment { get; set; }
        [DisplayName("Date Created")]
        public DateTimeOffset Created { get; set; }
        public bool Archived { get; set; }

        //FK's
        public int TicketId { get; set; }
        public string UserId { get; set; }

        //Parents
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}