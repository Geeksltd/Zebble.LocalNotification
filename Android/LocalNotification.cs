namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.OS;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class LocalNotification
    {
        internal const string LocalNotificationKey = "LocalNotification";
        internal static ScheduledAlarmHandler ReceiverInstance;

        internal static int NotificationIconId { get; set; }

        internal static NotificationManager NotificationManager => NotificationManager.FromContext(Application.Context);

        static AlarmManager AlarmManager => UIRuntime.GetService<AlarmManager>(Context.AlarmService);

        static int GetUniqueId => (int)Java.Lang.JavaSystem.CurrentTimeMillis() & 0xffffff;

        public static async Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            await CreateNotification(new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                PlaySound = playSound,
                Id = 0,
                IntentId = GetUniqueId,
                IconId = UIRuntime.NotificationSmallIcon,
                NotifyTime = DateTime.Now,
                Parameters = parameters.DicToString()
            });
            return true;
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var intent = CreateIntent(id);
            var packageName = UIRuntime.CurrentActivity.ApplicationContext.PackageName;

            var localNotification = new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                Id = id,
                IntentId = GetUniqueId,
                IconId = UIRuntime.NotificationSmallIcon,
                NotifyTime = notifyTime,
                PlaySound = playSound,
                Parameters = parameters.DicToString()
            };

            if (NotificationIconId != 0) localNotification.IconId = NotificationIconId;

            var serializedNotification = JsonConvert.SerializeObject(localNotification);
            intent.PutExtra(LocalNotificationKey, serializedNotification);

            AlarmManager.Set(AlarmType.RtcWakeup,
               triggerAtMillis: localNotification.NotifyTime.ToUnixEpoch(),
               operation: intent.ToPendingBroadcast());

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(CreateIntent(id).ToPendingBroadcast());
            NotificationManager.Cancel(id);

            return Task.CompletedTask;
        }

        public static Task Initialize(Intent intent, Action<Notification> OnTapped = null)
        {
            UIRuntime.OnNewIntent.Handle(async mainIntent =>
             {
                 await OnNotificationTapped(intent, OnTapped);

                 if (OS.IsAtLeast(BuildVersionCodes.O))
                 {
                     var serviceIntent = new Intent(UIRuntime.CurrentActivity, typeof(ScheduledAlarmService));
                     UIRuntime.CurrentActivity.StartService(serviceIntent);
                 }
                 else
                 {
                     var intentFilter = await SetActionFilters();
                     UIRuntime.CurrentActivity.RegisterReceiver(ReceiverInstance, intentFilter);
                 }
             });

            return Task.CompletedTask;
        }

        public static void Destroy()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) return;

            if (ReceiverInstance != null)
                UIRuntime.CurrentActivity.UnregisterReceiver(ReceiverInstance);
        }

        internal static Task<IntentFilter> SetActionFilters()
        {
            var intentFilter = new IntentFilter();
            intentFilter.AddAction("android.intent.action.SCREEN_ON");
            intentFilter.AddAction("android.intent.action.SCREEN_OFF");
            intentFilter.AddAction("android.intent.action.BOOT_COMPLETED");

            intentFilter.Priority = 100;

            ReceiverInstance = new ScheduledAlarmHandler();

            return Task.FromResult(intentFilter);
        }

        internal static Task CreateNotification(AndroidLocalNotification notification, Context context = null)
        {
            var currentcontext = context ?? UIRuntime.CurrentActivity;

            var builder = new Android.App.Notification.Builder(Application.Context)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetAutoCancel(autoCancel: true)
                .SetSmallIcon(notification.IconId);

            if (OS.IsAtLeast(BuildVersionCodes.O))
            {
                var channelId = Guid.NewGuid().ToString();
                var channel = new NotificationChannel(channelId, "app_channel_name", NotificationImportance.Default);
                NotificationManager.CreateNotificationChannel(channel);

                builder.SetChannelId(channelId);
            }

            if (notification.PlaySound) builder.SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));

            if (NotificationIconId != 0) builder.SetSmallIcon(NotificationIconId);

            var resultIntent = UIRuntime.LauncherActivity;
            resultIntent.PutExtra(LocalNotificationKey, JsonConvert.SerializeObject(notification));
            var resultPendingIntent = PendingIntent.GetActivity(currentcontext, notification.IntentId, resultIntent, PendingIntentFlags.UpdateCurrent);

            builder.SetContentIntent(resultPendingIntent);
            NotificationManager.Notify(notification.Id, builder.Build());

            return Task.CompletedTask;
        }

        async static Task OnNotificationTapped(Intent intent, Action<Notification> OnTapped = null)
        {
            if (OnTapped == null) return;

            var extra = intent.GetStringExtra(LocalNotificationKey);
            if (extra.LacksValue())
            {
                var launcherIntentData = UIRuntime.LauncherActivity.GetStringExtra(LocalNotificationKey);
                if (launcherIntentData.LacksValue()) return;
            }

            var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
            OnTapped.Invoke(new Notification
            {
                Title = notification.Title,
                Body = notification.Body,
                Id = notification.Id,
                NotifyTime = notification.NotifyTime,
                Parameters = notification.Parameters.StringToDic()
            });
        }

        static Intent CreateIntent(int id)
        {
            return new Intent(Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }
    }
}