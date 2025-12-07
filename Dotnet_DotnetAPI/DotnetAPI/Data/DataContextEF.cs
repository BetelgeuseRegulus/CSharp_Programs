using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Options;

namespace DotnetAPI.Data
{
    public class DataContextEF : DbContext
    {
        private readonly IConfiguration _config;

        public DataContextEF(IConfiguration config)
        {
            _config = config;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserSalary> UserSalary { get; set; }
        public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 让 DbContext 默认使用 appsettings.json 中名为 "DefaultConnection" 的连接字符串来连接 SQL Server 数据库
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    _config.GetConnectionString("DefaultConnection"),
                    optionsBuilder => optionsBuilder.EnableRetryOnFailure()
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema"); // 下面的表已经默认在 TutorialAppSchema后面添加新表

            modelBuilder
                .Entity<User>()
                .ToTable("Users", "TutorialAppSchema") // C#类 "User" 映射到数据库的表 "TutorialAppSchema.Users", 即类"User"是表的一项
                .HasKey(u => u.UserId);

            modelBuilder
                .Entity<UserSalary>() // 默认 UserSalary ==> TutorialAppSchema.UserSalary
                .HasKey(u => u.UserId);

            modelBuilder
                .Entity<UserJobInfo>() // 默认 UserJobInfo ==> TutorialAppSchema.UserJobInfo
                .HasKey(u => u.UserId);
        }
    }
}
