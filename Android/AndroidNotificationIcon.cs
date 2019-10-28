using Android.Content;

namespace Zebble
{
    public class AndroidNotificationIcon
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Package { get; set; }

        public AndroidNotificationIcon() { }

        public AndroidNotificationIcon(int iconId)
        {
            if (iconId == 0) return;

            var context = Renderer.Context;
            Name = context.Resources.GetResourceName(iconId);
            Type = context.Resources.GetResourceTypeName(iconId);
            Package = context.Resources.GetResourcePackageName(iconId);
        }

        public int ConvertToId(Context context)
        {
            return context.Resources.GetIdentifier(Name, Type, Package);
        }
    }
}