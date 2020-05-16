using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Models
{
    public class YourContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public YourContext(DbContextOptions<YourContext> options) : base(options) { }
        public DbSet<Transaction> Transactions {get;set;}
        public DbSet<User> Users {get;set;}
    }
}