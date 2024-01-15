using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Models;
using PressTheButton.Repositories.Interfaces;

namespace PressTheButton.Repositories
{
    public class QuestionRepository
    {
        public readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public Question GetQuestionId(int questionId)
        {
            return _context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
        }
    }
}
