using Microsoft.EntityFrameworkCore;
using Real_Estate_Services.Models;

namespace Real_Estate_Services.db
{
    public class DB : DbContext
    {
        public DB() { }
        public DB(DbContextOptions options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<Sites> Sites { get; set; }
    }
}
