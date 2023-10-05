using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Drivers.Api.Data;


public class DriverDbContext : IdentityDbContext
{
    public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
    {

    }
}