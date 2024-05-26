using PressTheButton.Models;
using PressTheButton.Enums;

namespace PressTheButton.ViewModels
{
    public class NotificationViewModel
    {
        public Notification Notification { get; set; }
        public Question Question { get; set; }
        public string ProfilePicPath { get; set; }
        public string ProfileName { get; set; }
        public string TextSender { get; set; }
        public string TextDestinatary { get; set; }
        public bool IsNew { get; set; }
    }
}
