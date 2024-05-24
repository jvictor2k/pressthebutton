using Microsoft.AspNetCore.Mvc;
using PressTheButton.Services.Interfaces;
using System.Security.Claims;

namespace PressTheButton.Components
{
    public class NotificationCountViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;


        public NotificationCountViewComponent(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = await _notificationService.GetUserIdAsync((ClaimsPrincipal)User);

            if(string.IsNullOrWhiteSpace(userId))
            {
                return Content(string.Empty);
            }

            var count = await _notificationService.GetNotificationsCount(userId);
            return View(count);
        }
    }
}
