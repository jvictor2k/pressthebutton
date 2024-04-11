﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PressTheButton.Models
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        public string Text { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Date {  get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}