## LocalNotification
A notification is a message that you show it to user when something happens in user account, profile and lots of other things that an application need to inform user to see it in application by tapping on the notification which show on the top of screen. LocalNotification is a Zebble implementation of Xamarin for all platforms and it is available on NuGet.
### How to use LocalNotification in Zebble?
To use this plugin in your Zebble application you need to install it from NuGet and set some configuration for Android and IOS platform and you can use the Show and Schedule methods of it as you need to show a notification. Furthermore, you are able to use Tap event to handle an action when user tap on your notification.
##### Show() :
To show a notification for your application at the moment you can use this method like below:
```csharp
await LocalNotification.Show("Test", "This is the body message");
```
The first parameter of this method related to the title of notification and the second one show your message into the body of notification. Also, you can use this method anywhere of your Zebble application that you want.
##### Schedule() :
This method is able to schedule your notification and you can use it like below:
```csharp
await LocalNotification.Schedule("Test", "This is the body message", DateTime.Now.AddSeconds(30), 1);
```
The first and second parameters of this method are related to the title and body of your notification respectively and third one get your schedule date and time to show your notification at that date and time. The last parameter is use for getting an identification for your notification and each one should have a unique identifier.

#### Android configuration:
In android project of Zebble application you should call Initialize method in OnCreate method of MainActivity class like below code:
```csharp
await LocalNotification.Initialize(Intent);
```
For instance, you can see the complete code of MainActivity:
```csharp
protected override async void OnCreate(Bundle bundle)
{
    base.OnCreate(bundle);

    SetContentView(Resource.Layout.Main);

    await LocalNotification.Initialize(Intent);

    Setup.Start(FindViewById<FrameLayout>(Resource.Id.Main_Layout),this).RunInParallel();

    await (StartUp.Current = new UI.StartUp()).Run();
}
```
 
#### IOS configuration:

Like android platform you need to call Initialize method of LocalNotification in IOS project of your Zebble application in FinishLaunching method of AppDelegate class like the code that is mentioned below:
```csharp
public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
{
     var result = base.FinishedLaunching(application, launchOptions);

     LocalNotification.Initialize(launchOptions);

     return result;
}
```

