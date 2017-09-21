using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        [DisplayName("Priorty Name")]
        public string Name { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public TicketPriority()
        {
            this.Tickets = new HashSet<Ticket>();
        }
    }
}