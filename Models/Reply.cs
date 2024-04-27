using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("Replys")]
    public class Reply
    {
        [Key]
        public int ReplyId { get; set; }
        public string Text { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Date { get; set; }
        public int CommentId { get; set; }
        public int QuestionId { get; set; }
        public string ProfilePicturePath {  get; set; }
        public Comment Comment { get; set; }
        public Question Question { get; set; }
        public ProfilePicture ProfilePicture { get; set; }
        public List<Rating> Ratings { get; set; }
    }
}
