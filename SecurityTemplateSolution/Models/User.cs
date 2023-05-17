using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleAPIWithJWT.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(60)")]
        public string Login { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
    }
}
