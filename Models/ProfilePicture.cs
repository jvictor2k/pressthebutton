using System.ComponentModel.DataAnnotations;

namespace PressTheButton.Models
{
    public class ProfilePicture
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Path { get; set; }
    }
}
