using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Enums;
using PressTheButton.Models;
using PressTheButton.Services.Interfaces;
using PressTheButton.ViewModels;

namespace PressTheButton.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationService _notificationService;

        public QuestionController(AppDbContext context, UserManager<IdentityUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var questions = await _context.Questions.Where(q => q.CreatedBy == userId).ToListAsync();
            return View(questions);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if (question != null)
                {
                    var userId = _userManager.GetUserId(User);
                    if (question.CreatedBy == userId)
                    {
                        return View(question);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //GET method Create
        public IActionResult Create()
        {
            return View();
        }

        //POST method Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    question.CreatedBy = _userManager.GetUserId(User);
                    question.Date = DateTime.Now;
                    question.Ativo = true;
                    _context.Add(question);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Lidar com exceções, se necessário
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar o item.");
            }

            return View(question);
        }

        //GET method Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if (question != null)
                {
                    var userId = _userManager.GetUserId(User);
                    if (question.CreatedBy == userId)
                    {
                        return View(question);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //POST method Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionId, Text, Negative")] Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var originalQuestion = await _context.Questions.FindAsync(id);

                        question.Ativo = originalQuestion.Ativo;
                        question.CreatedBy = originalQuestion.CreatedBy;
                        question.Date = DateTime.Now;

                        _context.Entry(originalQuestion).CurrentValues.SetValues(question);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!QuestionExists(question.QuestionId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(question);
            }
        }

        //GET method Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == id);

                if (question != null)
                {
                    return View(question);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }

        //POST method Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AllResponses()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var questionsWithResponses = _context.Questions
            .Where(q => q.UserResponses.Any(ur => ur.UserId == userId))
            .Select(q => new QuestionUserResponseViewModel
            {
                QuestionId = q.QuestionId,
                Text = q.Text,
                Negative = q.Negative,
                YesOrNo = q.UserResponses.FirstOrDefault(ur => ur.UserId == userId).YesOrNo
            })
            .ToList();

            return View(questionsWithResponses);
        }

        [HttpPost]
        public async Task<IActionResult> MakeComment(int questionId, string text)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
            if (question == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Text = text,
                CreatedBy = _userManager.GetUserId(User),
                Date = DateTime.Now,
                QuestionId = questionId
            };

            question.Comments ??= new List<Comment>();
            question.Comments.Add(comment);

            await _context.SaveChangesAsync();

            await _notificationService.MakeNotificationAsync(questionId, 0, comment.CommentId, User);

            return RedirectToAction("QuestionStats", "Home", new { questionId = questionId });
        }

        [HttpPost]
        public async Task<IActionResult> MakeReply(int commentId, string text, int questionId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var reply = new Reply
            {
                Text = text,
                CreatedBy = _userManager.GetUserId(User),
                Date = DateTime.Now,
                CommentId = commentId,
                QuestionId = questionId
            };

            comment.Replys ??= new List<Reply> { reply };
            comment.Replys.Add(reply);

            await _context.SaveChangesAsync();

            await _notificationService.MakeNotificationAsync(questionId, 1, reply.ReplyId, User);

            return RedirectToAction("QuestionStats", "Home", new { questionId = questionId });
        }

        [HttpPost]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return View("Error");
            }

            var userId = _userManager.GetUserId(User);

            var userRating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId &&
                                                      r.Type == CommentOrReply.Comment &&
                                                      r.TextId == commentId);

            if (userRating != null)
            {
                _context.Ratings.Remove(userRating);
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Type == CommentReplyOrRating.Rating
                                                                           && n.ElementId == commentId
                                                                           && n.SenderUserId == userId);
                if(notification != null)
                {
                    _context.Notifications.Remove(notification);
                }

                await _context.SaveChangesAsync();
            }

            if(userRating != null && userRating.Value == RatingValue.Like)
            {
                return Ok(new { liked = false });
            }

            comment.Ratings ??= new List<Rating>();

            var newRating = new Rating
            {
                UserId = userId,
                Value = RatingValue.Like,
                Type = CommentOrReply.Comment,
                TextId = commentId,
                Date = DateTime.Now
            };

            comment.Ratings.Add(newRating);
            await _context.SaveChangesAsync();
            await _notificationService.MakeNotificationAsync(comment.QuestionId, 2, comment.CommentId, User);
            return Ok(new {liked = true});
        }

        [HttpPost]
        public async Task<IActionResult> DislikeComment(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return View("Error");
            }

            var userId = _userManager.GetUserId(User);

            var userRating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == userId &&
                                                      r.Type == CommentOrReply.Comment &&
                                                      r.TextId == commentId);

            if (userRating != null)
            {
                _context.Ratings.Remove(userRating);
                var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Type == CommentReplyOrRating.Rating
                                                                           && n.ElementId == commentId
                                                                           && n.SenderUserId == userId);
                if(notification != null)
                {
                    _context.Notifications.Remove(notification);
                }

                await _context.SaveChangesAsync();
            }

            if(userRating != null && userRating.Value == RatingValue.Dislike)
            {
                return Ok(new { disliked = false });
            }

            comment.Ratings ??= new List<Rating>();

            var newRating = new Rating
            {
                UserId = userId,
                Value = RatingValue.Dislike,
                Type = CommentOrReply.Comment,
                TextId = commentId,
                Date = DateTime.Now
            };

            comment.Ratings.Add(newRating);
            await _context.SaveChangesAsync();
            return Ok(new { disliked = true });
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(q => q.QuestionId == id);
        }
    }
}
