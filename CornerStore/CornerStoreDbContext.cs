using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}