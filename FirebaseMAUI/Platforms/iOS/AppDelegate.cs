using CommunityToolkit.Maui.Core.Views;
using CoreData;
using Firebase.CloudMessaging;
using Foundation;
using UIKit;
using UserNotifications;

namespace FirebaseMAUI
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        //private Messaging _messaging;
        //private IMessenger _messenger;

        protected override MauiApp CreateMauiApp()
        {
            var app = MauiProgram.CreateMauiApp(ConfigurePlatformServices);

            //_messaging = app.Services.GetRequiredService<Messaging>();
            //_logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger<AppDelegate>();
            //_messenger = app.Services.GetRequiredService<IMessenger>();

            return app;
        }

        private MauiAppBuilder ConfigurePlatformServices(MauiAppBuilder builder)
        {
            if (Messaging.SharedInstance == null)
            {
                string plistPath = Path.Combine(NSBundle.MainBundle.BundlePath, "GoogleService-Info.plist");
                Firebase.Core.App.Configure(new Firebase.Core.Options(plistPath));
                //Firebase.Core.App.Configure();

                var foo = Firebase.Core.Configuration.SharedInstance;
            }

            Messaging.SharedInstance!.AutoInitEnabled = true;

            //builder.Services.AddSingleton(Messaging.SharedInstance);

            return builder;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            UNUserNotificationCenter.Current.Delegate = this;
            ServiceHelper.Current.GetRequiredService<INotifyService>().AskForNotificationPermission();
            Messaging.SharedInstance.Delegate = this;

            application.ApplicationIconBadgeNumber = new IntPtr(0);

            return base.FinishedLaunching(application, launchOptions);
        }

        #region Export Application

        //[Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
        //public void DidRegisterForRemoteNotificationsWithDeviceToken(UIApplication application, NSData apnsToken)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Remote notifications registration completed: [{apnsToken}].");
        //    //_messaging.SetApnsToken(apnsToken, ApnsTokenType.Unknown);
        //}

        //[Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
        //public void DidFailToRegisterForRemoteNotificationsWithError(UIApplication application, NSError error)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Remote notifications registration failed: {error?.LocalizedDescription}.");
        //}

        //[Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
        //public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Remote notification received in background: [{NSJsonSerialization.Serialize(userInfo, 0, out _).ToString()}].");
        //    completionHandler(UIBackgroundFetchResult.NewData);
        //}

        #endregion

        #region Export Messaging

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine($"Remote notifications token received: {fcmToken}");
        }

        [Export("messaging:didRefreshRegistrationToken:")]
        public void DidRefreshRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine($"FCM Token: {fcmToken}");
        }

        #endregion

        #region Export UserNotificationCenter(Push Notification)

        void ProcessNotification(NSDictionary dataDictionary)
        {
            try
            {
                // Check to see if the dictionary has the aps key.  This is the notification payload you would have sent
                if (null != dataDictionary && dataDictionary.ContainsKey(new NSString("aps")))
                {
                    System.Diagnostics.Debug.WriteLine($"dataDictionary: [{NSJsonSerialization.Serialize(dataDictionary, 0, out _).ToString()}].");

                    //Get the aps dictionary
                    NSDictionary aps = dataDictionary.ObjectForKey(new NSString("aps")) as NSDictionary;

                    string alert = string.Empty;

                    if (aps.ContainsKey(new NSString("alert")))
                        alert = (aps[new NSString("alert")] as NSString).ToString();

                    if (!string.IsNullOrEmpty(alert))
                    {
                        INotifyService _notifyService = ServiceHelper.Current.GetRequiredService<INotifyService>();
                        _notifyService.CreateNotification("Workspace Booking", alert);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, System.Action<UNNotificationPresentationOptions> completionHandler)
        {
            try
            {
                /*System.Console.WriteLine("WillPresentNotification");
                var test = notification as NSDictionary;
                var title = notification.Request.Content.Title;
                var body = notification.Request.Content.Body;
                ShowAlert(title, body);*/
                //completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge);
                completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Sound | UNNotificationPresentationOptions.Badge | UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, System.Action completionHandler)
        {
            if (response.IsDefaultAction)
            {
                System.Diagnostics.Debug.WriteLine("ACTION: Default");
                ProcessNotification(response.Notification.Request.Content.UserInfo);
            }
            completionHandler();
        }

        #endregion

    }
}
