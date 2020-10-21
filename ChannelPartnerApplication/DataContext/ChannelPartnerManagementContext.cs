using ChannelPartnerApplication.Domain.ChannelPartner;
using ChannelPartnerApplication.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ChannelPartnerApplication.DataContext
{
    public class ChannelPartnerManagementContext : DbContext
    {
        public ChannelPartnerManagementContext(DbContextOptions<ChannelPartnerManagementContext> options)
            : base(options)
        {
        }


        #region ClassBookMain

        //public DbSet<Student> Student { get; set; }
        //public DbSet<Classes> Classes { get; set; }
        //public DbSet<Teacher> Teacher { get; set; }
        //public DbSet<CareerExpert> CareerExpert { get; set; }
        //public DbSet<School> School { get; set; }
        public DbSet<ChannelPartner> ChannelPartner { get; set; }
        public DbSet<ChannelPartnerMapping> ChannelPartnerMapping { get; set; }

        #endregion

        #region Common

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Logs> Logs { get; set; }

        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
