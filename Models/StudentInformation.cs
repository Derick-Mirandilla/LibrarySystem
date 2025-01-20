using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Models
{
    public class StudentInformation
    {
        [Key]
        [Column(TypeName = "nvarchar(12)")]
        [DisplayName("LRN")]
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "LRN must be exactly 12 characters.")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "LRN must consist of exactly 12 numeric characters.")]
        public string StudentLRN { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        [DisplayName("Name")]
        [Required(ErrorMessage = "This field is required.")]
        public string StudentName { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        [DisplayName("Grade & Section")]
        [Required(ErrorMessage = "This field is required.")]
        public string StudentGradeSection { get; set; }

        [Column(TypeName = "nvarchar(11)")]
        [DisplayName("Contact Number")]
        [Required(ErrorMessage = "This field is required.")]
        [MaxLength(11)]
        public string StudentContact { get; set; }

        [Column(TypeName = "nvarchar(150)")]
        [DisplayName("Email")]
        [Required(ErrorMessage = "This field is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string StudentEmail { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
    }
}
