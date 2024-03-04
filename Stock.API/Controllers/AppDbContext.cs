using Microsoft.EntityFrameworkCore;

namespace Stock.API.Controllers
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Models.Stock> Stocks { get; set; }
    }
}
