using PressTheButton.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string SenderUserId { get; set; }
        public string DestinataryUserId { get; set; }
        public DateTime Date {  get; set; }
        public CommentReplyOrRating Type { get; set; }
        public int ElementId { get; set; }
        public int QuestionId { get; set; }
        public bool Readed { get; set; }
        public Question Question { get; set; }
        public Comment Comment { get; set; }
        public Reply Reply { get; set; }
        public Rating Rating { get; set; }
    }
}
