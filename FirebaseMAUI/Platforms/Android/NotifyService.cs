using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

[assembly: Dependency(typeof(FirebaseMAUI.Droid.NotifyService))]

namespace FirebaseMAUI.Droid
{

    public class NotifyService : INotifyService
    {
        private int _lastId = 1;
        private int GetNextNotificationId() => ++_lastId;

        private long[] vibrationPattern = { 100, 200, 300, 400, 500, 400, 300, 200, 400 };

        private AudioAttributes alarmAttributes = new AudioAttributes.Builder().SetContentType(AudioContentType.Sonification).SetUsage(AudioUsageKind.Notification).Build();
        private Android.Net.Uri notificationSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        private PendingIntentFlags pendingIntentFlags = (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent;

        private NotificationManager notifyManager = Android.App.Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

        private string channelId = string.Empty;
        private NotificationCompat.Builder builder;

        public NotifyService()
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    NotificationChannel notifyChannel = new NotificationChannel("DefaultChannel", "General", NotificationImportance.High);
                    notifyChannel.Description = "Workspace Notification Channel";
                    notifyChannel.Importance = NotificationImportance.High;
                    notifyChannel.LockscreenVisibility = NotificationVisibility.Secret;
                    //notifyChannel.LockscreenVisibility = NotificationVisibility.Public;
                    notifyChannel.SetShowBadge(false);
                    notifyChannel.EnableLights(true);
                    notifyChannel.EnableVibration(true);
                    notifyChannel.SetSound(notificationSoundUri, alarmAttributes);
                    notifyChannel.SetShowBadge(true);
                    notifyChannel.CanBypassDnd();
                    notifyChannel.SetVibrationPattern(vibrationPattern);
                    notifyManager.CreateNotificationChannel(notifyChannel);
                    channelId = Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O ? notifyChannel.Id : string.Empty;
                }

                builder = new NotificationCompat.Builder(Android.App.Application.Context, channelId);

                builder.SetGroup(channelId);
                builder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));
                builder.SetVibrate(vibrationPattern);
                builder.SetDefaults((int)NotificationDefaults.All);
                builder.SetSmallIcon(GetIcon("icon"));
                //builder.SetLargeIcon(BitmapFactory.DecodeResource(Android.App.Application.Context.Resources, Resource.Drawable.icon));
                builder.SetAutoCancel(true);
                builder.SetShowWhen(false);
                builder.SetOnlyAlertOnce(true);

                builder.SetColor(Android.Graphics.Color.Argb(255, 0, 130, 95));

                //builder.SetCategory(Notification.CategoryStatus);
                builder.SetCategory(Notification.CategoryEvent);
                //builder.SetCategory(Notification.CategoryService);

                builder.SetPriority(NotificationCompat.PriorityHigh);

                builder.SetVisibility((int)NotificationVisibility.Public);
                //builder.SetVisibility((int)NotificationVisibility.Secret);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public Task<bool> NotificationEnabled()
        {
            bool isPushNotificationEnabled = false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && Android.App.Application.Context.GetSystemService(Context.NotificationService) is NotificationManager notificationManager)
            {
                isPushNotificationEnabled = notificationManager.AreNotificationsEnabled();
            }
            else
            {
                isPushNotificationEnabled = NotificationManagerCompat.From(Android.App.Application.Context).AreNotificationsEnabled();
            }

            return Task.FromResult<bool>(isPushNotificationEnabled);
        }

        public void CreateNotification(string title, string message, IDictionary<string, object> data = null)
        {
            try
            {
                var newId = GetNextNotificationId();
                Intent notifyIntent = new Intent(Android.App.Application.Context, typeof(MainActivity));
                notifyIntent.AddFlags(ActivityFlags.ClearTop);

                if (data != null)
                {
                    foreach (string key in data.Keys)
                    {
                        notifyIntent.PutExtra(key, data[key].ToString());
                    }
                }

                PendingIntent pendingIntent = PendingIntent.GetActivity(Android.App.Application.Context, 0, notifyIntent, pendingIntentFlags);

                builder.SetContentText(message);
                builder.SetContentIntent(pendingIntent);

                if (!string.IsNullOrEmpty(title))
                {
                    builder.SetTicker(title);
                    builder.SetContentTitle(title);
                }

                builder.MActions.Clear();

                notifyManager.Notify(newId, builder.Build());

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private int GetIcon(string iconName)
        {
            var iconId = 0;

            if (string.IsNullOrWhiteSpace(iconName) == false)
            {
                iconId = Android.App.Application.Context.Resources.GetIdentifier(iconName, "drawable", Android.App.Application.Context.PackageName);
            }

            if (iconId != 0)
            {
                return iconId;
            }

            iconId = Android.App.Application.Context.ApplicationInfo.Icon;
            if (iconId == 0)
            {
                iconId = Android.App.Application.Context.Resources.GetIdentifier("icon", "drawable", Android.App.Application.Context.PackageName);
            }

            return iconId;
        }

        public void AskForNotificationPermission()
        {
            try
            {
                string[] notiPermission =
                {
                    Manifest.Permission.PostNotifications
                };

                if ((int)Build.VERSION.SdkInt < 33) return;

                if (Android.App.Application.Context.CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    Platform.CurrentActivity.RequestPermissions(notiPermission, 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }            
        }
    }
}