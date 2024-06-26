﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Enums;
using PressTheButton.Migrations;
using PressTheButton.Models;
using PressTheButton.Services.Interfaces;
using PressTheButton.ViewModels;
using System.IO;

namespace PressTheButton.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        public readonly UserManager<IdentityUser> _userManager;
        public readonly AppDbContext _context;
        private string _filePath;

        public NotificationController(UserManager<IdentityUser> userManager, AppDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _filePath = env.WebRootPath;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            List<Notification> notifications = await _context.Notifications.Where(n => n.DestinataryUserId == userId)
                                                                           .OrderBy(n => n.Readed)
                                                                           .ThenByDescending(n => n.Date).ToListAsync();

            List<NotificationViewModel> notificationsViewModel = new List<NotificationViewModel>();

            foreach(Notification notification in notifications)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == notification.QuestionId);

                var profilePic = await _context.ProfilePictures.FirstOrDefaultAsync(p => p.UserId == notification.SenderUserId);

                var user = await _userManager.FindByIdAsync(notification.SenderUserId);

                string profileName = user.UserName;

                var notificationVMItem = new NotificationViewModel
                {
                    Notification = notification,
                    Question = question,
                    ProfileName = profileName
                };

                if (profilePic != null)
                {
                    notificationVMItem.ProfilePicPath = profilePic.Path;
                    var filePath = Path.Combine(_filePath, "images", "profilePicture", notificationVMItem.ProfilePicPath);

                    if(!System.IO.File.Exists(filePath))
                    {
                        notificationVMItem.ProfilePicPath = "profile.jpg";
                    }
                }
                else
                {
                    notificationVMItem.ProfilePicPath = "profile.jpg";
                }

                if(notificationVMItem.ProfileName == null)
                {
                    notificationVMItem.ProfileName = "Anônimo";
                }

                if (notification.Type == CommentReplyOrRating.Comment)
                {
                    var senderComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == notification.ElementId);
                    notificationVMItem.TextSender = senderComment.Text;

                    var destinataryQuestion = await _context.Questions.FirstOrDefaultAsync(q =>  q.QuestionId == notification.QuestionId);
                    notificationVMItem.TextDestinatary = destinataryQuestion.Text + " Mas... " + destinataryQuestion.Negative;

                    notificationVMItem.Url = Url.Action("QuestionStats", "Home", new { questionId = notification.QuestionId }) + "#comment-" + notification.ElementId;
                }

                if(notification.Type == CommentReplyOrRating.Reply)
                {
                    var senderReply = await _context.Replys.FirstOrDefaultAsync(r => r.ReplyId == notification.ElementId);
                    notificationVMItem.TextSender = senderReply.Text;

                    var destinataryComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == senderReply.CommentId);
                    notificationVMItem.TextDestinatary = destinataryComment.Text;

                    notificationVMItem.Url = Url.Action("QuestionStats", "Home", new { questionId = notification.QuestionId }) + "#comment-" + senderReply.CommentId;
                }

                if(notification.Type == CommentReplyOrRating.Rating)
                {
                    var destinataryComment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == notification.ElementId);
                    notificationVMItem.TextDestinatary = destinataryComment.Text;

                    notificationVMItem.Url = Url.Action("QuestionStats", "Home", new { questionId = notification.QuestionId }) + "#comment-" + notification.ElementId;
                }

                notificationVMItem.IsNew = notification.Readed == true ? false : true;

                notificationsViewModel.Add(notificationVMItem);
            }

            await ReadNotifications(userId);

            return View(notificationsViewModel);
        }

        private async Task ReadNotifications(string userId)
        {
            var notifications = await _context.Notifications.Where(n => n.DestinataryUserId == userId
                                                               && !n.Readed).ToListAsync();

            foreach(Notification notification in notifications)
            {
                notification.Readed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
