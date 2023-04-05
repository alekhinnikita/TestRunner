using System.Collections.Generic;

namespace Desktop.Models
{
    internal class Template
    {
        public int Id { get; set; }
        public List<string> tests = new List<string>();
        public string? Address { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? Project { get; set; }
    }
}
