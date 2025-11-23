using Microsoft.EntityFrameworkCore;

namespace GestionLaboresAcademicas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
