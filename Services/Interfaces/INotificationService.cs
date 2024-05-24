using System.Security.Claims;

namespace PressTheButton.Services.Interfaces
{
    public interface INotificationService
    {
        Task MakeNotificationAsync(int questionId, int type, int elementId, ClaimsPrincipal user);
        Task<int> GetNotificationsCount(string userId);
        Task<string> GetUserIdAsync(ClaimsPrincipal user);
    }
}
