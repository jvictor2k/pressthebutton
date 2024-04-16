using PressTheButton.Models;

namespace PressTheButton.ViewModels
{
    public class QuestionStatsViewModel
    {
        public Question Question { get; set; }
        public int TotalResponses { get; set; }
        public double YesPercentage { get; set; }
        public double NoPercentage { get; set; }
        public List<(Comment, string)> CommentsWithUserNames { get; set; }
    }
}
