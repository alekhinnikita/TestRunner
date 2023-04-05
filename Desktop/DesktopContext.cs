using Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace Desktop
{
    internal class DesktopContext : DbContext
    {
        public DbSet<RunResult> RunResults { get; set; }
        public DbSet<Template> Templates { get; set; }

        public DesktopContext() { Database.EnsureCreated(); }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlite("DataSource=db.db");
        }
    }
}
