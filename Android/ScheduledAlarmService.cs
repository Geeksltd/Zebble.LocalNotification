namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using System;

    [Service(Enabled = true)]
    [Preserve]
    public class ScheduledAlarmService : Service
    {
        static ScheduledAlarmHandler Handler = new ScheduledAlarmHandler();
        public override IBinder OnBind(Intent intent) => null;

        [Preserve]
        public ScheduledAlarmService() { }

        [Preserve]
        public ScheduledAlarmService(IntPtr javaReference, JniHandleOwnership jniHandle) : base(javaReference, jniHandle) { }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterReceiver(Handler, AsActionFilters());
            Handler.Shown += (x, y) => StartForeground(x.Id, y);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            try { UnregisterReceiver(Handler); } catch { }
        }

        internal static void Start()
        {
            if (OS.IsAtLeast(BuildVersionCodes.O))
            {
                try
                {
                    var serviceIntent = new Intent(UIRuntime.CurrentActivity, typeof(ScheduledAlarmService));

                    var me = new ActivityManager.RunningAppProcessInfo();
                    ActivityManager.GetMyMemoryState(me);
                    if (me.Importance == Importance.Foreground || me.Importance == Importance.Visible)
                    {
                        UIRuntime.CurrentActivity.StartService(serviceIntent);
                    }
                    else
                    {
                        UIRuntime.CurrentActivity.StartForegroundService(serviceIntent);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("app is in background")) return;
                    throw;
                }
            }
            else
            {
                UIRuntime.CurrentActivity.RegisterReceiver(Handler, AsActionFilters());
                App.Stopping += () =>
                {
                    try { UIRuntime.CurrentActivity?.UnregisterReceiver(Handler); }
                    catch { }
                };
            }
        }

        static IntentFilter AsActionFilters()
        {
            var result = new IntentFilter { Priority = 100 };

            result.AddAction("android.intent.action.SCREEN_ON");
            result.AddAction("android.intent.action.SCREEN_OFF");
            result.AddAction("android.intent.action.BOOT_COMPLETED");

            return result;
        }
    }
}