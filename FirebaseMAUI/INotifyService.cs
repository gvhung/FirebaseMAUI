
namespace FirebaseMAUI
{
    public interface INotifyService
    {
        Task<bool> NotificationEnabled();

        void CreateNotification(string title, string message, IDictionary<string, object> data = null);

        void AskForNotificationPermission();
    }
}
