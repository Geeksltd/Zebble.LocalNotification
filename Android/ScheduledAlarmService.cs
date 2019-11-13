namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;

    [Service(Enabled = true)]
    [Preserve]
    public class ScheduledAlarmService : Service
    {
        public override IBinder OnBind(Intent intent) => null;

        public override async void OnCreate()
        {
            base.OnCreate();

            var intentFilter = await LocalNotification.SetActionFilters();
            RegisterReceiver(LocalNotification.ReceiverInstance, intentFilter);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (LocalNotification.ReceiverInstance != null) UnregisterReceiver(LocalNotification.ReceiverInstance);
        }
    }
}