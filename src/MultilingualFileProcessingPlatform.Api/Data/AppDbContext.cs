using Microsoft.EntityFrameworkCore;
using MultilingualFileProcessingPlatform.Api.Models;

namespace MultilingualFileProcessingPlatform.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }
}