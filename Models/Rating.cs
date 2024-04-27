using PressTheButton.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("Ratings")]
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public RatingValue Value { get; set; }
        public CommentOrReply Type { get; set; }
        public int TextId { get; set; }
        public DateTime Date { get; set; }
        public Comment Comment { get; set; }
        public Reply Reply { get; set; }
    }
}
