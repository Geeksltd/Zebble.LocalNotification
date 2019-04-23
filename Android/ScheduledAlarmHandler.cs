namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Newtonsoft.Json;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(LocalNotification.LocalNotificationKey);
            var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);

            if (notification == null) return;

            LocalNotification.CreateNotification(notification, context);
        }
    }

    [Service(Enabled = true)]
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