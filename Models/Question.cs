namespace PressTheButton.Models
{
    public class Question
    {
        public Question(int questionId, string text, string negative)
        {
            QuestionId = questionId;
            Text = text;
            Negative = negative;
        }

        public int QuestionId { get; set; }
        public string Text { get; set; }
        public string Negative {  get; set; }
        public List<UserResponse> UserResponses { get; set; }
    }
}
