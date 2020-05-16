using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BankAccounts.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public float Amount {get;set;}

        public int UserId {get;set;}

        public User Creator {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;
    }

    public class TransactionModel
    {
        // No other fields!
        [Display(Name = "Deposit/Withdraw:")]
        [Required(ErrorMessage = "Must enter an amount")]
        public float Amount {get;set;}
    }
}