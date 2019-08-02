namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.Media;
    using Android.OS;
    using Java.Lang;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static partial class LocalNotification
    {
        static NotificationChannel CurrentChannel;
        internal const string LocalNotificationKey = "LocalNotification";
        internal static ScheduledAlarmHandler ReceiverInstance;
        public static int IconResourceId = Android.Resource.Drawable.IcNotificationOverlay;
        public static int TransParentIconResourceId = Android.Resource.Drawable.IcNotificationOverlay;
        public static Color TransparentIconColor = Colors.White;

        internal static NotificationManager NotificationManager => NotificationManager.FromContext(Application.Context);

        static AlarmManager AlarmManager => UIRuntime.GetService<AlarmManager>(Context.AlarmService);

        static int GetUniqueId => (int)Java.Lang.JavaSystem.CurrentTimeMillis() & 0xffffff;

        public static async Task<bool> Show(string title, string body, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var noti = CreateNotification(title, body, playSound, 0, parameters);
            await CreateNotification(noti, UIRuntime.CurrentActivity);
            return true;
        }

        static AndroidLocalNotification CreateNotification(string title, string body, bool playSound, int id, Dictionary<string, string> parameters)
        {
            var result = new AndroidLocalNotification
            {
                Title = title,
                Body = body,
                PlaySound = playSound,
                Id = id,
                IntentId = GetUniqueId,
                IconId = IconResourceId,
                TransparentIconId = TransParentIconResourceId,
                TransparentIconColor = TransparentIconColor,
                NotifyTime = DateTime.Now,
                Parameters = parameters.DicToString()
            };

            return result;
        }

        public static Task<bool> Schedule(string title, string body, DateTime notifyTime, int id, bool playSound = false, Dictionary<string, string> parameters = null)
        {
            var notification = CreateNotification(title, body, playSound, id, parameters);

            var intent = AsAlarmHandlerIntent(id)
                .PutExtra(LocalNotificationKey, JsonConvert.SerializeObject(notification));

            AlarmManager.Set(AlarmType.RtcWakeup, notifyTime.ToUnixEpoch(), intent.ToPendingBroadcast());

            return Task.FromResult(result: true);
        }

        public static Task Cancel(int id)
        {
            AlarmManager.Cancel(AsAlarmHandlerIntent(id).ToPendingBroadcast());
            NotificationManager.Cancel(id);

            return Task.CompletedTask;
        }



        public static void Destroy()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) return;

            if (ReceiverInstance != null)
            {
                try
                {
                    UIRuntime.CurrentActivity.UnregisterReceiver(ReceiverInstance);
                }
                catch (IllegalArgumentException)
                {
                    // Receiver not registered.
                }
            }
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

        public static void Configure(string name, string description, int iconResourceId,int transparentIconResourceId,Color transparentIconColor, bool sound, NotificationImportance importance = NotificationImportance.High)
        {
            IconResourceId = iconResourceId;
            TransParentIconResourceId = transparentIconResourceId;
            TransparentIconColor = transparentIconColor;

            CurrentChannel = new NotificationChannel(name.ToCamelCaseId().ToLower(), name, importance)
            {
                Description = description
            };

            if (sound) CurrentChannel.SetSound(GetSoundUri(), GetAudioAttributes());

            NotificationManager.CreateNotificationChannel(CurrentChannel);
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

        internal static AudioAttributes GetAudioAttributes() => new AudioAttributes.Builder().SetContentType(AudioContentType.Sonification).SetUsage(AudioUsageKind.Alarm).Build();

        internal static Android.Net.Uri GetSoundUri() => RingtoneManager.GetDefaultUri(RingtoneType.Notification);

        internal static Task CreateNotification(AndroidLocalNotification notification, Context context)
        {
            if (CurrentChannel == null)
                throw new System.Exception("In MainActivity.OnCreate() call LocalNotification.CreateChannel(...).");

            NotificationManager.Notify(notification.Id, notification.Render(context, CurrentChannel.Id));

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

        static Intent AsAlarmHandlerIntent(int id)
        {
            return new Intent(Application.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }
    }
}