using Microsoft.EntityFrameworkCore;

namespace UbaClone.WebApi.Data
{
    public class DataContext: DbContext
    {
       
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        
        }

        public DbSet<Models.UbaClone> ubaClones { get; set; }
        public DbSet<TransactionDetails> TransactionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.UbaClone>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Models.UbaClone>()
                .HasMany(u => u.TransactionHistory)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

    }
}



