using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("UserResponses")]
    public class UserResponse
    {
        public Question Question { get; set; }

        [Key]
        public int UserResponseId { get; set; }

        public int QuestionId { get; set; }

        public bool YesOrNo { get; set; }

        public string UserIdentifier { get; set; }

        public string UserId { get; set; }
    }
}
