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
        public DbSet<Years> Years { get; set; }
        public DbSet<Months> Months { get; set; }
        public DbSet<ChannelPartner> ChannelPartner { get; set; }
        public DbSet<ChannelPartnerMapping> ChannelPartnerMapping { get; set; }
        public DbSet<PromotionalCycle> PromotionalCycle { get; set; }
        public DbSet<RoyaltyMapping> RoyaltyMapping { get; set; }
        public DbSet<PromotionHistory> PromotionHistory { get; set; }
        public DbSet<CommissionHistory> CommissionHistory { get; set; }

        #endregion

        #region Common

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<QueryType> QueryType { get; set; }
        public DbSet<Queries> Queries { get; set; }
        public DbSet<FAQ> FAQ { get; set; }

        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}