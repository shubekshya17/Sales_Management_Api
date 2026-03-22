using Microsoft.EntityFrameworkCore;
using SalesManagement.Models;

namespace SalesManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<SalesCollection> SalesCollections { get; set; }
        public DbSet<SalesDetail> SalesDetail { get; set; }
        public DbSet<CategoryRange> CategoryRanges { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SalesCollection>(entity =>
            {
                entity.HasIndex(e => new { e.Date, e.Invoice })
                      .IsUnique()
                      .HasDatabaseName("UQ_SalesCollection_Date_Invoice");
            });
            modelBuilder.Entity<SalesDetail>(entity =>
            {
                entity.HasIndex(e => new { e.TRNDATE, e.VCHRNO, e.ItemCode })
                      .IsUnique()
                      .HasDatabaseName("UQ_SalesDetail_TRNDATE_VCHRNO_ITEMCODE");
            });
        }
    }
}
