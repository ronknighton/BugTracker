using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        [DisplayName("File Path or Image")]
        public string MediaURL { get; set; }
        [DisplayName("Attachment Description")]
        public string Description{ get; set; }
        [DisplayName("Date Created")]
        public DateTimeOffset Created {get; set; }
        public bool Archived { get; set; }

        //FK's
        public string UserId { get; set; }
        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}