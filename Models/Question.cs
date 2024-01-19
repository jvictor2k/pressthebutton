using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        public string Text { get; set; }

        public string Negative {  get; set; }

        public List<UserResponse> UserResponses { get; set; }
    }
}
