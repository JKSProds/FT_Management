using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class CalendarioEvent
    {
        public string id { get; set; }
        public string title { get; set; }
        public string calendarId { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string body { get; set; }
        public int IdTecnico { get; set; }
        public string bgColor { get; set; }
        public bool setAllDay { get; set; }
        public string obs { get; set; }
        public string category { get; set; }
        public string dueDateClass { get; set; }
        public string location { get; set; }
        public string state { get; set; }
        public string attendees { get; set; }
        public string url { get; set; }
    }
}