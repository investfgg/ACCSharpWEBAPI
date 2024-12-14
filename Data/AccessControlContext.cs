using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using netwebapi_access_control.Models;

namespace netwebapi_access_control.Data;

public partial class AccessControlContext(DbContextOptions<AccessControlContext> options) : DbContext(options)
{
    public virtual DbSet<TbApplication> TbApplications { get; set; }

    public virtual DbSet<TbAppsobj> TbAppsobjs { get; set; }

    public virtual DbSet<TbObject> TbObjects { get; set; }

    public virtual DbSet<TbPermission> TbPermissions { get; set; }

    public virtual DbSet<TbProfile> TbProfiles { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    public virtual DbSet<TbUsersapp> TbUsersapps { get; set; }

    public virtual DbSet<TbUsertype> TbUsertypes { get; set; }

    public virtual DbSet<TbUsraccess> TbUsraccesses { get; set; }

    public virtual DbSet<TbUstypeperm> TbUstypeperms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<TbApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_applications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title");
            entity.Property(e => e.Acronym).HasMaxLength(255).HasColumnName("acronym");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");
        });

        modelBuilder.Entity<TbAppsobj>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_appsobjs");

            entity.HasIndex(e => e.IdApplications, "id_applications");
            entity.HasIndex(e => e.IdObjects, "id_objects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdApplications).HasColumnName("id_applications");
            entity.Property(e => e.IdObjects).HasColumnName("id_objects");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");

            entity.HasOne(d => d.IdApplicationsNavigation).WithMany(p => p.TbAppsobjs).HasForeignKey(d => d.IdApplications).HasConstraintName("tb_appsobjs_ibfk_1");
            entity.HasOne(d => d.IdObjectsNavigation).WithMany(p => p.TbAppsobjs).HasForeignKey(d => d.IdObjects).HasConstraintName("tb_appsobjs_ibfk_2");
        });

        modelBuilder.Entity<TbObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_objects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");
        });

        modelBuilder.Entity<TbPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_permissions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");
        });

        modelBuilder.Entity<TbProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_profiles");

            entity.HasIndex(e => e.IdAppsobjs, "id_appsobjs");
            entity.HasIndex(e => e.IdUsersapps, "id_usersapps");
            entity.HasIndex(e => e.IdUstypeperms, "id_ustypeperms");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdUsersapps).HasColumnName("id_usersapps");
            entity.Property(e => e.IdAppsobjs).HasColumnName("id_appsobjs");
            entity.Property(e => e.IdUstypeperms).HasColumnName("id_ustypeperms");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");

            entity.HasOne(d => d.IdUsersappsNavigation).WithMany(p => p.TbProfiles).HasForeignKey(d => d.IdUsersapps).HasConstraintName("tb_profiles_ibfk_2");
            entity.HasOne(d => d.IdAppsobjsNavigation).WithMany(p => p.TbProfiles).HasForeignKey(d => d.IdAppsobjs).HasConstraintName("tb_profiles_ibfk_1");
            entity.HasOne(d => d.IdUstypepermsNavigation).WithMany(p => p.TbProfiles).HasForeignKey(d => d.IdUstypeperms).HasConstraintName("tb_profiles_ibfk_3");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");
        });

        modelBuilder.Entity<TbUsersapp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_usersapps");

            entity.HasIndex(e => e.IdApplications, "id_applications");
            entity.HasIndex(e => e.IdUsrsaccess, "id_usrsaccess");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdApplications).HasColumnName("id_applications");
            entity.Property(e => e.IdUsrsaccess).HasColumnName("id_usrsaccess");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");

            entity.HasOne(d => d.IdApplicationsNavigation).WithMany(p => p.TbUsersapps).HasForeignKey(d => d.IdApplications).HasConstraintName("tb_usersapps_ibfk_1");
            entity.HasOne(d => d.IdUsrsaccessNavigation).WithMany(p => p.TbUsersapps).HasForeignKey(d => d.IdUsrsaccess).HasConstraintName("tb_usersapps_ibfk_2");
        });

        modelBuilder.Entity<TbUsertype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_usertypes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasMaxLength(255).HasColumnName("title");
            entity.Property(e => e.Description).HasMaxLength(255).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");
        });

        modelBuilder.Entity<TbUsraccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_usraccess");

            entity.HasIndex(e => e.IdUsers, "id_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasMaxLength(255).HasColumnName("username");
            entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
            entity.Property(e => e.Tip).HasMaxLength(255).HasColumnName("tip");
            entity.Property(e => e.IdUsers).HasColumnName("id_users");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.TbUsraccesses).HasForeignKey(d => d.IdUsers).HasConstraintName("tb_usraccess_ibfk_1");
        });

        modelBuilder.Entity<TbUstypeperm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tb_ustypeperms");

            entity.HasIndex(e => e.IdPermissions, "id_permissions");
            entity.HasIndex(e => e.IdUsertypes, "id_usertypes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdUsertypes).HasColumnName("id_usertypes");
            entity.Property(e => e.IdPermissions).HasColumnName("id_permissions");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnType("datetime").HasColumnName("deleted_at");

            entity.HasOne(d => d.IdPermissionsNavigation).WithMany(p => p.TbUstypeperms).HasForeignKey(d => d.IdPermissions).HasConstraintName("tb_ustypeperms_ibfk_2");
            entity.HasOne(d => d.IdUsertypesNavigation).WithMany(p => p.TbUstypeperms).HasForeignKey(d => d.IdUsertypes).HasConstraintName("tb_ustypeperms_ibfk_1");
        });

        #pragma warning disable S3251 // Implementations should be provided for "partial" methods
        OnModelCreatingPartial(modelBuilder);
        #pragma warning restore S3251 // Implementations should be provided for "partial" methods
    }

    #pragma warning disable S3251 // Implementations should be provided for "partial" methods
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    #pragma warning restore S3251 // Implementations should be provided for "partial" methods
}