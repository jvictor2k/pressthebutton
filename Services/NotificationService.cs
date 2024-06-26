﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Enums;
using PressTheButton.Models;
using PressTheButton.Services.Interfaces;
using System.Security.Claims;

namespace PressTheButton.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationService(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task MakeNotificationAsync(int questionId, int type, int elementId, ClaimsPrincipal user)
        {
            CommentReplyOrRating notificationType = (CommentReplyOrRating)type;

            Question question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);

            var senderUser = await _userManager.GetUserAsync(user);

            var newNotification = new Notification
            {
                Date = DateTime.Now,
                Type = notificationType,
                SenderUserId = senderUser.Id,
                ElementId = elementId,
                QuestionId = questionId,
                Readed = false
            };

            if (notificationType == CommentReplyOrRating.Comment)
            {
                newNotification.DestinataryUserId = question.CreatedBy;
            }

            if(notificationType == CommentReplyOrRating.Reply)
            {
                Reply reply = await _context.Replys.FirstOrDefaultAsync(r => r.ReplyId == elementId);
                var commentReplied = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == reply.CommentId);
                newNotification.DestinataryUserId = commentReplied.CreatedBy;
            }

            if (notificationType == CommentReplyOrRating.Rating)
            {
                Comment comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == elementId);
                newNotification.DestinataryUserId = comment.CreatedBy;
            }

            _context.Notifications.Add(newNotification);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetNotificationsCount(string userId)
        {
            return await _context.Notifications.Where(n => n.DestinataryUserId == userId
                                                        && !n.Readed).CountAsync();
        }

        public async Task<string> GetUserIdAsync(ClaimsPrincipal user)
        {
            var userId = await _userManager.GetUserAsync(user);
            return userId.Id;
        }
    }
}
