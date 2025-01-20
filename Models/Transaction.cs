using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Models
{
    public class Transaction
    {
        [Key]
        [DisplayName("Transaction ID")]
        public int TransactionID { get; set; } // Primary Key

        [Required]
        [DisplayName("LRN")]
        public string LRN { get; set; } // Foreign Key for Students table

        [DisplayName("Student Name")]
        public string StudentName { get; set; } // Cached from Students table

        [DisplayName("Grade & Section")]
        public string GradeAndSection { get; set; } // Cached from Students table

        [DisplayName("Contact Number")]
        public string ContactNumber { get; set; } // Cached from Students table

        [DisplayName("Email")]
        public string Email { get; set; } // Cached from Students table

        [Required]
        [DisplayName("Book ID")]
        public int BookID { get; set; } // Foreign Key for Books table

        [DisplayName("Book Title")]
        public string BookTitle { get; set; } // Cached from Books table

        [DisplayName("Issue Date")]
        [Required]
        public DateTime IssueDate { get; set; }

        [DisplayName("Return Date")]
        public DateTime? ReturnDate { get; set; } // Nullable for pending returns

        [DisplayName("Transaction Status")]
        [Required]
        [DefaultValue("Active")]
        public string TransactionStatus { get; set; } = "Active";

    }
}
