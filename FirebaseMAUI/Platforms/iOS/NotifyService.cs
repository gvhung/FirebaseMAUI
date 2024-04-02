using Foundation;
using UIKit;
using UserNotifications;

[assembly: Dependency(typeof(FirebaseMAUI.iOS.NotifyService))]

namespace FirebaseMAUI.iOS
{
    public class NotifyService : INotifyService
    {
        public NotifyService()
        {            
        }

        public Task<bool> NotificationEnabled() => MainThread.InvokeOnMainThreadAsync(() =>
        {
            var tcs = new TaskCompletionSource<bool>();

            UNUserNotificationCenter.Current.GetNotificationSettings(notificationSettings =>
            {
                var hasAlertNeverBeenRequested = notificationSettings.AlertSetting is UNNotificationSetting.NotSupported;
                var hasBadgeNeverBeenRequested = notificationSettings.BadgeSetting is UNNotificationSetting.NotSupported;
                var hasSoundsNeverBeenRequested = notificationSettings.SoundSetting is UNNotificationSetting.NotSupported;

                var isAlertEnabled = notificationSettings.AlertSetting is UNNotificationSetting.Enabled;
                var isBadgeEnabled = notificationSettings.BadgeSetting is UNNotificationSetting.Enabled;
                var isSoundsEnabled = notificationSettings.SoundSetting is UNNotificationSetting.Enabled;

                if (isAlertEnabled || isBadgeEnabled || isSoundsEnabled)
                    tcs.SetResult(true);
                else if (hasAlertNeverBeenRequested && hasBadgeNeverBeenRequested && hasSoundsNeverBeenRequested)
                    tcs.SetResult(false);
                else
                    tcs.SetResult(false);
            });

            return tcs.Task;
        });

        public async void CreateNotification(string title, string message, IDictionary<string, object> data = null)
        {
            try
            {
                UNNotification[] uNNotifications = await UNUserNotificationCenter.Current.GetDeliveredNotificationsAsync().ConfigureAwait(false);
                int _badge = uNNotifications.Length + 1;

                UNMutableNotificationContent notifyContent = new UNMutableNotificationContent()
                {
                    LaunchImageName = "AppIcons.png",
                    CategoryIdentifier = "DefaultChannel",
                    ThreadIdentifier = "General",
                    Title = title,
                    Body = message,
                    SummaryArgument = title,
                    Sound = UNNotificationSound.Default,
                    //Badge = new NSNumber(_badge++)
                };

                if (data != null)
                {
                    try
                    {
                        NSDictionary userInfo = NSDictionary.FromObjectsAndKeys(data.Values.ToArray(), data.Keys.ToArray());
                        notifyContent.UserInfo = userInfo;
                    }
                    catch (Exception exx)
                    {
                        Console.WriteLine(exx.ToString());
                    }
                }

                // create a time-based trigger, interval is in seconds and must be greater than 0. quarter-second used, close to realtime
                UNTimeIntervalNotificationTrigger trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.1, false);

                string requestID = System.Guid.NewGuid().ToString();
                UNNotificationRequest request = UNNotificationRequest.FromIdentifier(requestID, notifyContent, trigger);

                UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
                {
                    if (err != null)
                    {
                        Console.WriteLine($"Error: {err.LocalizedDescription ?? ""}");
                    }
                });

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void AskForNotificationPermission()
        {
            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine(error.ToString());
                    }
                });
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }

    }
}