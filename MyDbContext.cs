using Microsoft.EntityFrameworkCore;

namespace HealthChecksExample
{
    public class MyDbContext : DbContext
    {

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    }
}
