using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YobiWi.Development.Models
{
    public partial class YobiWiContext : DbContext
    {
        public YobiWiContext()
        {
        }
        public YobiWiContext(DbContextOptions<YobiWiContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Build> Builds { get; set; }
        public virtual DbSet<LogMessage> Logs { get; set; }
        public virtual DbSet<UserCache> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("server=localhost;database=yobiWi;user=root;pwd=root;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Build>(entity =>
            {
                entity.HasKey(build => build.buildId)
                    .HasName("PRIMARY");

                entity.ToTable("builds");

                entity.Property(e => e.buildId)
                    .HasColumnName("build_id")
                    .HasColumnType("int(11)");
                
                entity.Property(build => build.userId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(build => build.buildName)
                    .HasColumnName("build_name")
                    .HasColumnType("varchar(256) CHARACTER SET utf8 COLLATE utf8_general_ci");
                
                entity.Property(build => build.archiveName)
                    .HasColumnName("archive_name")
                    .HasColumnType("varchar(256) CHARACTER SET utf8 COLLATE utf8_general_ci");

                entity.Property(build => build.buildHash)
                    .HasColumnName("build_hash")
                    .HasColumnType("varchar(20)");
                
                entity.Property(build => build.urlInstall)
                    .HasColumnName("url_install")
                    .HasColumnType("varchar(256)");

                entity.Property(build => build.urlArchive)
                    .HasColumnName("url_archive")
                    .HasColumnType("varchar(256)");

                entity.Property(build => build.urlIcon)
                    .HasColumnName("url_icon")
                    .HasColumnType("varchar(256)");

                entity.Property(build => build.version)
                    .HasColumnName("version")
                    .HasColumnType("varchar(10)");                
                
                entity.Property(build => build.numberBuild)
                    .HasColumnName("number_build")
                    .HasColumnType("varchar(10)");

                entity.Property(build => build.bundleIdentifier)
                    .HasColumnName("bundle_identifier")
                    .HasColumnType("varchar(256) CHARACTER SET utf8 COLLATE utf8_general_ci");

                entity.Property(build => build.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("int(11)");

                entity.Property(build => build.buildDeleted)
                    .HasColumnName("build_delete")
                    .HasColumnType("boolean");
            });
            modelBuilder.Entity<UserCache>(entity =>
            {
                entity.HasKey(e => e.userId)
                    .HasName("PRIMARY");

                entity.ToTable("users");

                entity.HasIndex(e => e.userEmail)
                    .HasName("user_email")
                    .IsUnique();

                entity.Property(e => e.userId)
                    .HasColumnName("user_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.activate)
                    .HasColumnName("activate")
                    .HasColumnType("boolean");

                entity.Property(e => e.deleted)
                    .HasColumnName("deleted")
                    .HasColumnType("boolean");

                entity.Property(e => e.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("int(11)");

                entity.Property(e => e.lastLoginAt)
                    .HasColumnName("last_login_at")
                    .HasColumnType("int(11)");

                entity.Property(e => e.userEmail)
                    .HasColumnName("user_email")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.userHash)
                    .HasColumnName("user_hash")
                    .HasColumnType("varchar(120)");

                entity.Property(e => e.userPassword)
                    .HasColumnName("user_password")
                    .HasColumnType("varchar(256)");

                entity.Property(e => e.userToken)
                    .HasColumnName("user_token")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.recoveryCode)
                    .HasColumnName("recovery_code")
                    .HasColumnType("int(11)");
                    
                entity.Property(e => e.recoveryToken)
                    .HasColumnName("recovery_token")
                    .HasColumnType("varchar(50)");
            });
            modelBuilder.Entity<LogMessage>(entity =>
            {
                entity.HasKey(e => e.logId)
                    .HasName("PRIMARY");

                entity.ToTable("logs");

                entity.Property(e => e.logId)
                    .HasColumnName("log_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.message)
                    .HasColumnName("message")
                    .HasColumnType("varchar(2000)");

                entity.Property(e => e.userComputer)
                    .HasColumnName("user_computer")
                    .HasColumnType("varchar(100)");

                entity.Property(e => e.createdAt)
                    .HasColumnName("created_at")
                    .HasColumnType("DATETIME");
                
                entity.Property(e => e.level)
                    .HasColumnName("level")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.userId)
                    .HasColumnName("user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.threadId)
                    .HasColumnName("thread_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.userIP)
                    .HasColumnName("user_ip")
                    .HasColumnType("varchar(20)");
            });
        }
    }
}
