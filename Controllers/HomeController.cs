using Microsoft.AspNetCore.Mvc;
using PressTheButton.Models;
using System.Diagnostics;
using PressTheButton.Context;
using Microsoft.EntityFrameworkCore;
using PressTheButton.ViewModels;
using Microsoft.AspNetCore.Identity;
using PressTheButton.Migrations;

namespace PressTheButton.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private string _filePath;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _filePath = env.WebRootPath;
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

        public async Task<IActionResult> QuestionStats(int questionId)
        {
            var question = _context.Questions
            .Include(q => q.UserResponses)
            .Include(q => q.Comments)
            .Include(q => q.Replys)
            .FirstOrDefault(q => q.QuestionId == questionId);

            if (question == null)
            {
                return NotFound();
            }

            var totalResponses = question.UserResponses.Count();
            var yesResponses = question.UserResponses.Count(ur => ur.YesOrNo == true);
            var noResponses = totalResponses - yesResponses;

            var commentsWithUserNames = new List<(Comment comment, string userName)>();

            var replysWithUserNames = new List<(Reply reply, string userName)>();

            foreach (var comment in question.Comments)
            {
                var user = await _userManager.FindByIdAsync(comment.CreatedBy);
                if(user != null)
                {
                    string userName = user.UserName;

                    var profilePicture = await _context.ProfilePictures.FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if(profilePicture == null || profilePicture.Path == null)
                    {
                        comment.ProfilePicturePath = "profile.jpg";
                        commentsWithUserNames.Add((comment, userName));
                    }
                    else
                    {
                        var filePath = Path.Combine(_filePath, "images", "profilePicture", profilePicture.Path);

                        if (System.IO.File.Exists(filePath))
                        {
                            comment.ProfilePicturePath = filePath;
                            commentsWithUserNames.Add((comment, userName));
                        }
                        else
                        {
                            comment.ProfilePicturePath = "profile.jpg";
                            commentsWithUserNames.Add((comment, userName));
                        }
                    }
                }
                else
                {
                    comment.ProfilePicturePath = "profile.jpg";
                    commentsWithUserNames.Add((comment, "Anônimo"));
                }
            }

            foreach (Reply reply in question.Replys)
            {
                var replyUser = await _userManager.FindByIdAsync(reply.CreatedBy);
                if (replyUser != null)
                {
                    string replyUserName = replyUser.UserName;

                    var profilePicture = await _context.ProfilePictures.FirstOrDefaultAsync(p => p.UserId == replyUser.Id);

                    if(profilePicture == null || profilePicture.Path == null)
                    {
                        reply.ProfilePicturePath = "profile.jpg";
                        replysWithUserNames.Add((reply, replyUserName));
                    }
                    else
                    {
                        var filePath = Path.Combine(_filePath, "images", "profilePicture", profilePicture.Path);

                        if(System.IO.File.Exists(filePath))
                        {
                            reply.ProfilePicturePath = filePath;
                            replysWithUserNames.Add((reply, replyUserName));
                        }
                        else
                        {
                            reply.ProfilePicturePath = "profile.jpg";
                            replysWithUserNames.Add((reply, replyUserName));
                        }
                    }
                }
                else
                {
                    reply.ProfilePicturePath = "profile.jpg";
                    replysWithUserNames.Add((reply, "Anônimo"));
                }
            }

            var viewModel = new QuestionStatsViewModel
            {
                Question = question,
                TotalResponses = totalResponses,
                YesPercentage = totalResponses > 0 ? (yesResponses / (double)totalResponses) * 100 : 0,
                NoPercentage = totalResponses > 0 ? (noResponses / (double)totalResponses) * 100 : 0,
                CommentsWithUserNames = commentsWithUserNames,
                ReplysWithUserNames = replysWithUserNames
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
