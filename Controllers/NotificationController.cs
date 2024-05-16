using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PressTheButton.Context;
using PressTheButton.Enums;
using PressTheButton.Models;

namespace PressTheButton.Controllers
{
    public class NotificationController : Controller
    {
        public readonly UserManager<IdentityUser> _userManager;
        public readonly AppDbContext _context;

        public NotificationController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public void MakeNotification(int questionId, int type, int elementId)
        {
            CommentReplyOrRating notificationType = (CommentReplyOrRating)type;

            Question question = _context.Questions.FirstOrDefault(q => q.QuestionId == questionId);

            var newNotification = new Notification
            {
                Date = DateTime.Now,
                Type = notificationType,
                SenderUserId = _userManager.GetUserId(User),
                Readed = false
            };

            if(notificationType == CommentReplyOrRating.Comment)
            {
                newNotification.DestinataryUserId = question.CreatedBy;
                newNotification.ElementId = questionId;
            }

            if(notificationType == CommentReplyOrRating.Reply ||
               notificationType == CommentReplyOrRating.Rating)
            {
                newNotification.ElementId = elementId;

                Comment comment = _context.Comments.FirstOrDefault(c => c.CommentId == elementId);
                newNotification.DestinataryUserId = comment.CreatedBy;
            }

            _context.Notifications.Add(newNotification);
            _context.SaveChanges();
        }
    }
}
