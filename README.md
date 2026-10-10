# Hotel Management

This ASP.NET Core MVC application uses SQL Server and ASP.NET Core Identity for super-admin and tenant sign-in.

## Configuration

Set the following environment variables before starting the application:

```powershell
$env:BootstrapAdmin__Email = "admin@example.com"
$env:BootstrapAdmin__Password = "<strong unique password>"
```

`BootstrapAdmin__Password` must be at least 12 characters and include uppercase and lowercase letters, a number, and a symbol. The app creates the configured super-admin account on first startup and ensures its role exists on subsequent startups. Do not store production credentials in source control.

The default connection string uses SQL Server LocalDB. Override `ConnectionStrings__DefaultConnection` for another SQL Server instance, for example:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=.;Database=HotelManagement;Trusted_Connection=True;TrustServerCertificate=True"
```

The app applies EF Core migrations during startup. The super-admin signs in at `/Account/Login`, opens **Tenant management**, and onboards tenants by supplying contact details and an initial password. The tenant email is also its login name. Tenant accounts can only access their own tenant portal; deactivating a tenant prevents subsequent sign-in and invalidates active tenant sessions.

Tenant admins can add rooms from **My hotel**. Each room has a positive integer room number and a room type (for example, Single, Double, or Master). Room numbers must be unique within a hotel; different tenants may use the same room number.

Tenant admins can add guest records and confirm bookings from **Guests & bookings**. A booking assigns one of the tenant's guests to one of the tenant's rooms and records a required check-in date. A room cannot be assigned to an overlapping stay. Check-out is recorded later by entering the actual date, which must be after check-in; a stay without a check-out date remains active and occupies its room.
