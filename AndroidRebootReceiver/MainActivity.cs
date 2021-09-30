using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using TaskStackBuilder = Android.App.TaskStackBuilder;

namespace AndroidRebootReceiver
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }
    }

    [BroadcastReceiver(
        Enabled = true, 
        Permission = "android.permission.RECEIVE_BOOT_COMPLETED", 
        Exported = true)]
    [IntentFilter(new []
    {
        "android.intent.action.BOOT_COMPLETED", 
        "android.intent.action.QUICKBOOT_POWERON",
        // HTC are special
        "com.htc.intent.action.QUICKBOOT_POWERON"
    }, Categories = new []{ "android.intent.category.DEFAULT" })]
    public class MyRebootReceiver : BroadcastReceiver
    {
        private const string NotificationChannelId = "boot_notifications";
        private const int NotificationId = 1000;
        
        public override void OnReceive(Context? context, Intent? intent)
        {
            Log.Info("MyRebootReceiver", "Got intent");
            
            var notificationManager = (NotificationManager) context?.GetSystemService(Context.NotificationService);
            SetupNotificationChannel(notificationManager);
            
            var resultIntent = new Intent(context, typeof(MainActivity));
            var stackBuilder = TaskStackBuilder.Create(context);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(resultIntent);

            // Create the PendingIntent with the back stack:
            var resultPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(context, NotificationChannelId)
                .SetContentTitle("Device Rebooted")
                .SetContentText("Your device rebooted")
                .SetSmallIcon(Resource.Drawable.ic_stat_accessibility_new)
                .SetContentIntent(resultPendingIntent);
                
            notificationManager?.Notify(NotificationId, notification.Build());
        }
        
        private static void SetupNotificationChannel(NotificationManager notificationManager)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

            var channel = new NotificationChannel(NotificationChannelId, "Boot Notifications",
                NotificationImportance.Default)
            {
                Description = "Channel for receiving boot notifications"
            };
                
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}