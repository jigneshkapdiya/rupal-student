using RupalStudentCore8App.Server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RupalStudentCore8App.Server.Data
{
    public partial class ApplicationDbContext : IdentityDbContext<AspNetUser, AspNetRole, int>
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

 
        public virtual DbSet<AspMenu> AspMenus { get; set; }
        public virtual DbSet<AspMenuPermission> AspMenuPermissions { get; set; }
        public virtual DbSet<AspNetPermission> AspNetPermissions { get; set; }
        public virtual DbSet<AspNetRefreshToken> AspNetRefreshTokens { get; set; }
        //public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        //public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
        //public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        //public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        //public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        //public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<AutoIncrement> AutoIncrements { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<LoginHistory> LoginHistories { get; set; }
        public virtual DbSet<StudentMarkSheet> StudentMarkSheets { get; set; }
        public virtual DbSet<UserDevice> UserDevices { get; set; }
        public virtual DbSet<StudentEducation> StudentEducations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

        modelBuilder.Entity<AspMenu>(entity =>
        {
            entity.Property(e => e.Class).HasDefaultValue("");
            entity.Property(e => e.Icon).HasDefaultValue("");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasConstraintName("FK_AspMenu_ParentId");
        });

        modelBuilder.Entity<AspMenuPermission>(entity =>
        {
            entity.HasOne(d => d.Menu).WithMany(p => p.AspMenuPermissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AspMenuPermissions_AspMenu");

            entity.HasOne(d => d.Role).WithMany(p => p.AspMenuPermissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AspMenuPermissions_AspNetRoles");
        });

        modelBuilder.Entity<AspNetPermission>(entity =>
        {
            entity.HasOne(d => d.Menu).WithMany(p => p.AspNetPermissions).HasConstraintName("FK_AspPermissions_AspMenu");
        });

        modelBuilder.Entity<AspNetRefreshToken>(entity =>
        {
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetRefreshTokens).HasConstraintName("FK_AspNetRefreshToken_AspNetUsers");
        });
            //modelBuilder.Entity<AspNetUser>(entity =>
            //{
            //    entity.HasMany(d => d.Roles)
            //        .WithMany(p => p.Users)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "AspNetUserRole",
            //            l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
            //            r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
            //            j =>
            //            {
            //                j.HasKey("UserId", "RoleId");

            //                j.ToTable("AspNetUserRoles");
            //            });
            //});

            //modelBuilder.Entity<AspNetUserLogin>(entity =>
            //{
            //    entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            //});

            //modelBuilder.Entity<AspNetUserToken>(entity =>
            //{
            //    entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            //});

           

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories).HasConstraintName("FK_LoginHistory_AspNetUsers");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.UserDevices).HasConstraintName("FK_UserDevices_AspNetUsers");
        });

           // modelBuilder.Entity<UserDevice>(entity =>
            //{
                // Configure default values for timestamps
               // entity.Property(e => e.FirstLogin).HasDefaultValueSql("(getutcdate())");
               // entity.Property(e => e.LastLogin).HasDefaultValueSql("(getutcdate())");

                // Configure required string properties with max lengths
               // entity.Property(e => e.DeviceIdentifier).IsRequired().HasMaxLength(256);
               // entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(100);
               // entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(20);
               // entity.Property(e => e.Os).IsRequired().HasMaxLength(100);
               // entity.Property(e => e.Browser).IsRequired().HasMaxLength(200);
               // entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);

               // // Configure foreign key relationship
                //entity.HasOne(d => d.User)
                //    .WithMany(p => p.UserDevices)
                //    .HasForeignKey(d => d.UserId)
                //    .OnDelete(DeleteBehavior.Cascade)
                 //   .HasConstraintName("FK_UserDevices_AspNetUsers");

               // // Configure unique index on UserId and DeviceIdentifier
               // entity.HasIndex(e => new { e.UserId, e.DeviceIdentifier })
               //     .IsUnique();
           // });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
