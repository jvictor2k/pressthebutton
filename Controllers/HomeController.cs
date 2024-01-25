using Microsoft.AspNetCore.Mvc;
using PressTheButton.Models;
using System.Diagnostics;
using PressTheButton.Context;
using Microsoft.EntityFrameworkCore;
using PressTheButton.ViewModels;

namespace PressTheButton.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            Question randomQuestion = GetRandomQuestion();

            if(randomQuestion != null)
            {
                return View(randomQuestion);
            }
            else
            {
                return View("NoMoreQuestions");
            }
        }

        private Question GetRandomQuestion()
        {
            string userIdentifier = GetUserIdentifier();

            var unansweredQuestions = _context.Questions
                .Where(q => !q.UserResponses.Any(ur => ur.UserIdentifier == userIdentifier))
                .ToList();

            int totalUnansweredQuestions = unansweredQuestions.Count();

            if (totalUnansweredQuestions > 0)
            {
                Random random = new Random();
                int randomIndex = random.Next(totalUnansweredQuestions);

                return unansweredQuestions[randomIndex];
            }
            else
            {
                return null;
            }

            //int totalQuestions = _context.Questions.Count();

            //if (totalQuestions > 0)
            //{
            //    Random random = new Random();
            //    int randomIndex = random.Next(totalQuestions);
            //
            //    return _context.Questions.Skip(randomIndex).FirstOrDefault();
            //}
            //else
            //{
            //    return null;
            //}
        }

        [HttpPost]
        public IActionResult ApertarBotao(int questionId)
        {
            var userResponse = new UserResponse
            {
                QuestionId = questionId,
                YesOrNo = true,
                UserIdentifier = GetUserIdentifier()
            };

            _context.UserResponses.Add(userResponse);
            _context.SaveChanges();

            return RedirectToAction("QuestionStats", new { questionId });
        }

        [HttpPost]
        public IActionResult NaoApertarBotao(int questionId)
        {
            var userResponse = new UserResponse
            {
                QuestionId = questionId,
                YesOrNo = false,
                UserIdentifier = GetUserIdentifier()
            };

            _context.UserResponses.Add(userResponse);
            _context.SaveChanges();

            return RedirectToAction("QuestionStats", new { questionId });
        }

        private string GetUserIdentifier()
        {
            if(HttpContext.Request.Cookies.ContainsKey("userIdentifier"))
            {
                return HttpContext.Request.Cookies["userIdentifier"];
            }
            else
            {
                string identifier = Guid.NewGuid().ToString();
                HttpContext.Response.Cookies.Append("userIdentifier", identifier);
                return identifier;
            }
        }

        public IActionResult QuestionStats(int questionId)
        {
            var question = _context.Questions
            .Include(q => q.UserResponses)
            .FirstOrDefault(q => q.QuestionId == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var totalResponses = question.UserResponses.Count();
            var yesResponses = question.UserResponses.Count(ur => ur.YesOrNo == true);
            var noResponses = totalResponses - yesResponses;

            var viewModel = new QuestionStatsViewModel
            {
                Question = question,
                TotalResponses = totalResponses,
                YesPercentage = totalResponses > 0 ? (yesResponses / (double)totalResponses) * 100 : 0,
                NoPercentage = totalResponses > 0 ? (noResponses / (double)totalResponses) * 100 : 0
            };

            return View("QuestionStats", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
