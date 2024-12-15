using Microsoft.EntityFrameworkCore;
using System;

namespace UnitTestProject.Web.Models
{
    public class MyDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MyDb");
        }
        public DbSet<Product> Products { get; set; }
    }
}
