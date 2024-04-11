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

        public string CreatedBy { get; set; }

        public DateTime Date { get; set; }

        public bool Ativo {  get; set; }

        public List<UserResponse> UserResponses { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
