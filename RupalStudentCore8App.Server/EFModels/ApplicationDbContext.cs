using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Apex14Core8App.Server.EFModels;

public partial class ApplicationDbContext : DbContext
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

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<AutoIncrement> AutoIncrements { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<UserDevice> UserDevices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=JIGNESH;Initial Catalog=Dev_ApexCore8DB;User ID=sa;Password=Patidar84;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Readonly).HasDefaultValue(true);
            entity.Property(e => e.RefreshTokenExpiry).HasDefaultValueSql("(getdate())");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Item__3214EC0770E3E7BA");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories).HasConstraintName("FK_LoginHistory_AspNetUsers");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.UserDevices).HasConstraintName("FK_UserDevices_AspNetUsers");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
