global using Microsoft.EntityFrameworkCore;

namespace StudentAPI
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<Student> Students => Set<Student>();
    }
}
