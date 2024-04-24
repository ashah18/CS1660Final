using Microsoft.EntityFrameworkCore;
using MvcApp.Models;

namespace MvcApp.DataModel;
public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }

    // public DbSet<UploadModel> uploadModels { get; set; }
    public DbSet<AdminCreds> adminCreds { get; set; }
    public DbSet<vwSavedModel> SavedModels { get; set; }
    public DbSet<AdminDetails> vwAdminDetails { get; set; }
    public DbSet<SaveModel> SaveModel { get; set; }
}