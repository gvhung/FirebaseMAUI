using FirebaseAdmin.Messaging;

[assembly: Dependency(typeof(FirebaseMAUI.WinUI.NotifyService))]

namespace FirebaseMAUI.WinUI
{
    public class NotifyService : INotifyService
    {
        public NotifyService()
        {
        }

        public Task<bool> NotificationEnabled() => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var tcs = new TaskCompletionSource<bool>();

            

            return tcs.Task;
        });

        public void CreateNotification(string title, string message, IDictionary<string, object> data = null)
        {
            try
            {
                

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void AskForNotificationPermission()
        {
            
        }

    }
}