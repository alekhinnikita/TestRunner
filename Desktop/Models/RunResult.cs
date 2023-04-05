using System;

namespace Desktop.Models
{
    internal class RunResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
        public string Log { get; set; }
        public bool Success { get; set; } = false;
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
