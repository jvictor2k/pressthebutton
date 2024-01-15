using PressTheButton.Models;

namespace PressTheButton.Repositories.Interfaces
{
    public interface IQuestionInterface
    {
        Question GetQuestion(int questionId);
    }
}
