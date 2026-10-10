using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestApplication.Models;

namespace TestApplication.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Guest> Guests => Set<Guest>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>()
            .HasIndex(tenant => tenant.Email)
            .IsUnique();

        builder.Entity<ApplicationUser>()
            .HasOne(user => user.Tenant)
            .WithMany()
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Room>()
            .HasIndex(room => new { room.TenantId, room.RoomNumber })
            .IsUnique();

        builder.Entity<Room>()
            .HasOne(room => room.Tenant)
            .WithMany()
            .HasForeignKey(room => room.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Guest>()
            .HasIndex(guest => new { guest.TenantId, guest.NicNumber });

        builder.Entity<Guest>()
            .HasAlternateKey(guest => new { guest.TenantId, guest.Id });

        builder.Entity<Guest>()
            .HasOne(guest => guest.Tenant)
            .WithMany()
            .HasForeignKey(guest => guest.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Room>()
            .HasAlternateKey(room => new { room.TenantId, room.Id });

        builder.Entity<Booking>()
            .HasIndex(booking => new { booking.TenantId, booking.RoomId, booking.CheckInDate });

        builder.Entity<Booking>()
            .HasOne(booking => booking.Guest)
            .WithMany()
            .HasForeignKey(booking => new { booking.TenantId, booking.GuestId })
            .HasPrincipalKey(guest => new { guest.TenantId, guest.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(booking => booking.Room)
            .WithMany()
            .HasForeignKey(booking => new { booking.TenantId, booking.RoomId })
            .HasPrincipalKey(room => new { room.TenantId, room.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override int SaveChanges()
    {
        SetTenantTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Tenant>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = now;
                entry.Entity.UpdatedOn = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = now;
                entry.Property(tenant => tenant.CreatedOn).IsModified = false;
            }
        }
    }
}
