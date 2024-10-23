using BlazorServerApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BlazorServerApp.Data 
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Project> Projects { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ProjectID);

                entity.Property(e => e.UserID)
                      .IsRequired();

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(255);
                entity.Property(e => e.Language)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.Description)
                      .IsRequired(false);

                entity.Property(e => e.CreationDate)
                      .IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
    
}