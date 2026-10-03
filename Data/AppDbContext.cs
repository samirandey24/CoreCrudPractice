using CoreCrudWithJwt.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CoreCrudWithJwt.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
           
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
    
}
