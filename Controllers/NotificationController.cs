using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Context;
using PressTheButton.Enums;
using PressTheButton.Models;
using PressTheButton.Services.Interfaces;

namespace PressTheButton.Controllers
{
    [Authorize]
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
            var userId = _userManager.GetUserId(User);

            List<Notification> notifications = _context.Notifications.Where(n => n.DestinataryUserId == userId).ToList();

            return View(notifications);
        }
    }
}
