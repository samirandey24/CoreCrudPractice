using System.ComponentModel.DataAnnotations;

namespace CoreCrudWithJwt.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
