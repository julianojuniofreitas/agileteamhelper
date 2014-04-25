using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace JiraTrayApp
{
    public class JiraTrayAppDbContext: DbContext
    {
        public JiraTrayAppDbContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<IssueTemplate> Issues { get; set; } 
    }
}
