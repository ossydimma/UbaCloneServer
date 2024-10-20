using Microsoft.EntityFrameworkCore;

namespace UbaClone.WebApi.Data
{
    public class DataContext: DbContext
    {
       
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        
        }

        public DbSet<Models.UbaClone> Users { get; set; }
        public DbSet<TransactionDetails> TransactionHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.UbaClone>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Models.UbaClone>()
               .HasMany(p => p.TransactionHistory)
               .WithOne(c => c.UbaCloneUser)
               .HasForeignKey(c => c.UbaCloneUserId);
        }

    }
}



