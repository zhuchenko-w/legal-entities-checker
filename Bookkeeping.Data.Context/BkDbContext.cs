using Bookkeeping.Data.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Bookkeeping.Data.Context
{
    public partial class BkDbContext : IdentityDbContext<BkUser>
    {
        public BkDbContext(string nameOrConnectionString) : base(nameOrConnectionString: nameOrConnectionString)
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<BkDbContext>());//TODO: add setting
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<BkDbContext, Configuration>());
        }

        public BkDbContext() : this("BkDbContext")
        {
        }

        public static BkDbContext Create()
        {
            return new BkDbContext();
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Resolution> Resolutions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
        }
    }
}
