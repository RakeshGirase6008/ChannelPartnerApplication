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

        public DbSet<States> States { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Pincode> Pincode { get; set; }
        public DbSet<ChannelPartner> ChannelPartner { get; set; }
        public DbSet<ChannelPartnerMapping> ChannelPartnerMapping { get; set; }
        public DbSet<PromotionalCycle> PromotionalCycle { get; set; }

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
