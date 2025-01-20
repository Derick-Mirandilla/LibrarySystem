using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LibrarySystem.Models
{
    public class BookInformation
    {
        [Key]
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookID { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        [DisplayName("Title")]
        [Required(ErrorMessage = "This field is required.")] // Required field
        public string BookTitle { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Author")]
        [Required(ErrorMessage = "This field is required.")] // Required field
        public string BookAuthor { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Classification")]
        public string? BookClassification { get; set; } // Nullable field

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Donor")]
        public string? BookDonor { get; set; } // Nullable field

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Category")]
        [Required(ErrorMessage = "This field is required.")] // Required field
        public string BookCategory { get; set; }

        [RegularExpression(@"^(Available|Unavailable)$", ErrorMessage = "Invalid Book Status. Valid options are 'Available' or 'Unavailable'.")]
        [DisplayName("Status")]
        public string BookStatus { get; set; } = "Available";

        public ICollection<Transaction> Transactions { get; set; }
    }
}
