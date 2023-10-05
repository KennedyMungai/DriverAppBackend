using Microsoft.EntityFrameworkCore;

namespace Drivers.Api.Data;


public class DriverDbContext : DbContext
{
    public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
    {

    }
}