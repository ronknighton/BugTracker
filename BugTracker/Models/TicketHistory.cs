using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        [DisplayName("Property Changed")]
        public string Property { get; set; }
        [DisplayName("Property Old Value")]
        public string OldValue { get; set; }
        [DisplayName("Property New Value")]
        public string NewValue { get; set; }
        [DisplayName("Date Changed")]
        public DateTimeOffset DateChanged { get; set; }
        public bool Archived { get; set; }

        //FK's
        public string UserId { get; set; }
        public int TicketId { get; set; }

        //Parents
        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }

       
    }
}