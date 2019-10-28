namespace Zebble.Device
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Newtonsoft.Json;
    using System;

    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Plugin Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent /* Alarm Handler Intent*/)
        {
            var extra = intent.GetStringExtra(LocalNotification.LocalNotificationKey);
            if (extra.LacksValue()) return;

            try
            {
                var notification = JsonConvert.DeserializeObject<AndroidLocalNotification>(extra);
                if (notification != null)
                {
                    LocalNotification.CreateNotification(notification, context.ApplicationContext);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
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