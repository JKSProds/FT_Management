﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FT_Management.Models
{
    public class CalendarEvent
    {
        public int id { get; set; }
        public string title { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string url { get; set; }
        public int IdTecnico { get; set; }
        public string color { get; set; }
    }
}