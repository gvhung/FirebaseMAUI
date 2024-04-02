using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace FirebaseMAUI.Droid
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        INotifyService _notifyService;        

        public MyFirebaseMessagingService()
        {
            _notifyService = ServiceHelper.Current.GetRequiredService<INotifyService>();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            try
            {
                Console.WriteLine("From: " + message.From);
                if (message.GetNotification() != null)
                {
                    //These is how most messages will be received
                    Console.WriteLine("Notification Message Title: " + message.GetNotification().Title);
                    Console.WriteLine("Notification Message Body: " + message.GetNotification().Body);
                    //SendNotification(message.GetNotification().Body);
                    _notifyService.CreateNotification(message.GetNotification().Title, message.GetNotification().Body);
                }
                else
                {
                    _notifyService.CreateNotification("Workspace Booking", message.Data.Values.First());
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }

}