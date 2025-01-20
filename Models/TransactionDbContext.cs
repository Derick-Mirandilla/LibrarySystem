using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Models
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options)
            : base(options)
        { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<StudentInformation> StudentInformation { get; set; }
        public DbSet<BookInformation> BookInformation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StudentInformation>()
                .HasKey(s => s.StudentLRN);

            modelBuilder.Entity<Transaction>()
                .HasOne<StudentInformation>()
                .WithMany(s => s.Transactions)
                .HasForeignKey(t => t.LRN);

            modelBuilder.Entity<Transaction>()
                .HasOne<BookInformation>()
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BookID);
        }
    }
}
