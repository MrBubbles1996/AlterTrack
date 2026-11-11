using AlterTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlterTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.ToTable(t => t.HasCheckConstraint("CK_Customers_EmployeeNumber_Format","\"EmployeeNumber\" ~ '^800[0-9]{6}$'"));
            // Column limits keep bad input out of the database, not just out of the API.
            customer.Property(c => c.Name).HasMaxLength(100);
            customer.Property(c => c.EmployeeNumber).HasMaxLength(9);
            customer.Property(c => c.Phone).HasMaxLength(30);
            customer.Property(c => c.Email).HasMaxLength(254);
            customer.Property(c => c.Notes).HasMaxLength(1000);

            // Staff look customers up by phone at the counter, so index it.
            customer.HasIndex(c => c.Phone);
            customer.HasIndex(c => c.EmployeeNumber).IsUnique();
        });
    }
}
