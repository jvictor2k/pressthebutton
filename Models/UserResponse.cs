namespace PressTheButton.Models
{
    public class UserResponse
    {
        public Question Question { get; set; }
        public int UserResponseId { get; set; }
        public int QuestionId { get; set; }
        public bool YesOrNo { get; set; }
    }
}
