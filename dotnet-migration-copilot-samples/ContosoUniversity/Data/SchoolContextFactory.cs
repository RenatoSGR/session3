using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ContosoUniversity.Data
{
    /// <summary>
    /// Design-time factory used by EF Core CLI tools (dotnet ef migrations add, etc.)
    /// </summary>
    public class SchoolContextFactory : IDesignTimeDbContextFactory<SchoolContext>
    {
        public SchoolContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(
                "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=ContosoUniversityNoAuthEFCore;Integrated Security=True");
            return new SchoolContext(optionsBuilder.Options);
        }
    }
}
