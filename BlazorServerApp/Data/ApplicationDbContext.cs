using BlazorServerApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BlazorServerApp.Data 
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ProjectAccess> ProjectAccesses { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                   .Property(u => u.PermissionLevel)
                   .HasDefaultValue(PermissionLevel.User);
            builder.Entity<ProjectAccess>(entity =>
            {
                entity.HasKey(x => new { x.ProjectID, x.UserID });

                entity.HasOne(x => x.Project)
                      .WithMany(p => p.ProjectAccesses)
                      .HasForeignKey(x => x.ProjectID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.User)
                      .WithMany(u => u.ProjectAccesses)
                      .HasForeignKey(x => x.UserID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
    
}