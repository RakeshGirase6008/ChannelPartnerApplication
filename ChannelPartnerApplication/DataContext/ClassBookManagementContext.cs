using Microsoft.EntityFrameworkCore;

namespace ChannelPartnerApplication.DataContext
{
    public class ClassBookManagementContext : DbContext
    {
        public ClassBookManagementContext(DbContextOptions<ClassBookManagementContext> options)
            : base(options)
        {
        }

        #region Common
        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
