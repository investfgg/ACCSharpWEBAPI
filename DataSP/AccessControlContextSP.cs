using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace netwebapi_access_control.DataSP
{
    // Elaboração de funções criadas no schema Access_Control, do MySQL.
    public class FNCodedPassword
    {
        #pragma warning disable S1104 // Fields should not have public accessibility
        public string? Password;
        #pragma warning restore S1104 // Fields should not have public accessibility

        #pragma warning disable S3400 // Methods should not return constants
        public static string FNCodedPassword_SEL() {
            return "SELECT `access_control`.`fn_CodedPassword`( '{0}' ) as Password";
        }
        #pragma warning restore S3400 // Methods should not return constants
    }

    public partial class AccessControlContextSP: DbContext
    {
        public virtual DbSet<SPUsersAppsRelUsrByApp> SPUsersAppsRelUsrByApp { get; set; } = null!;

        public virtual DbSet<SPUsersAppsRelAppByUsr> SPUsersAppsRelAppByUsr { get; set; } = null!;

        public virtual DbSet<SPUsersSelAll> SPUsersSelAll { get; set; } = null!;

        public virtual DbSet<FNCodedPassword> FunCodedPassword { get; set; }

        public AccessControlContextSP()
        {

        }

        public AccessControlContextSP( DbContextOptions<AccessControlContextSP> options ) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Controller: TbApplicationController ** Http: Get ** Address: UsersByApplication
            modelBuilder.Entity<SPUsersAppsRelUsrByApp>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.UsersAppsID);
                entity.Property(e => e.ApplicationID);
                entity.Property(e => e.ApplicationName);
                entity.Property(e => e.ApplicationTitle);
                entity.Property(e => e.UsrAccessID);
                entity.Property(e => e.UsrAccessUserName);
                entity.Property(e => e.UserName);
                entity.Property(e => e.UserEmail);
            });

            // Controller: TbUserController ** Http: Get ** Address: ApplicationsByUser
            modelBuilder.Entity<SPUsersAppsRelAppByUsr>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.UsersAppsID);
                entity.Property(e => e.UserID);
                entity.Property(e => e.UserName);
                entity.Property(e => e.UserUserName);
                entity.Property(e => e.UserEmail);
                entity.Property(e => e.ApplicationID);
                entity.Property(e => e.ApplicationName);
                entity.Property(e => e.ApplicationTitle);
            });

            modelBuilder.Entity<SPUsersSelAll>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Id);
                entity.Property(e => e.Username);
                entity.Property(e => e.Password);
                entity.Property(e => e.Tip);
                entity.Property(e => e.Description);
                entity.Property(e => e.Name);
                entity.Property(e => e.Email);
                entity.Property(e => e.Created_At);
                entity.Property(e => e.Updated_At);
                entity.Property(e => e.Deleted_At);
            });

            modelBuilder.Entity<FNCodedPassword>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Password);
            });
        }
    }
}