using Microsoft.AspNetCore.Mvc;
using PressTheButton.Models;
using System.Diagnostics;
using PressTheButton.Context;
using Microsoft.EntityFrameworkCore;
using PressTheButton.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace PressTheButton.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
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
            var unansweredQuestions = new List<Question>();

            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserId(User);

                unansweredQuestions = _context.Questions
                .Where(q => !q.UserResponses.Any(ur => ur.UserId == userId))
                .ToList();
            }
            else
            {
                string userIdentifier = GetUserIdentifier();

                unansweredQuestions = _context.Questions
                .Where(q => !q.UserResponses.Any(ur => ur.UserIdentifier == userIdentifier))
                .ToList();
            }

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
        }

        [HttpPost]
        public IActionResult ApertarBotao(int questionId)
        {
            var userResponse = new UserResponse
            {
                QuestionId = questionId,
                YesOrNo = true
            };

            if (User.Identity.IsAuthenticated)
            {
                userResponse.UserId = _userManager.GetUserId(User);
            }
            else
            {
                userResponse.UserIdentifier = GetUserIdentifier();
            }

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
                YesOrNo = false
            };

            if (User.Identity.IsAuthenticated)
            {
                userResponse.UserId = _userManager.GetUserId(User);
            }
            else
            {
                userResponse.UserIdentifier = GetUserIdentifier();
            }

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
            .Include(q => q.Comments)
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
