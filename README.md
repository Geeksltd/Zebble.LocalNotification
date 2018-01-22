[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.LocalNotification/master/Shared/NuGet/Icon.png "Zebble.LocalNotification"


## Zebble.LocalNotification

![logo]

A consistent and easy way to show local notifications in Zebble apps.


[![NuGet](https://img.shields.io/nuget/v/Zebble.LocalNotification.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.LocalNotification/)

> With local notifications, your app configures the notification details locally and passes those details to the system, which then handles the delivery of the notification when your app is not in the foreground. Local notifications are supported on iOS, Android and UWP.

<br>


### Setup
* Available on NuGet: [https://www.nuget.org/packages/Zebble.LocalNotification/](https://www.nuget.org/packages/Zebble.LocalNotification/)
* Install in your platform client projects.
* Available for iOS, Android and UWP.
<br>


### Api Usage
Call `Zebble.Device.LocalNotification` from any project to gain access to APIs.

##### Display a local notification immediately:
```csharp
await LocalNotification.Show("Test", "This is the body message");
```

##### Display a local notification at a scheduled time :
```csharp
await LocalNotification.Schedule("Test", "This is the body message", DateTime.Now.AddSeconds(30), 1);
```
<br>

### Platform Specific Notes
Some platforms require certain permissions or settings before it will display notifications.


#### Android
In android project of Zebble application you should call Initialize method in OnCreate method of MainActivity class like below code:
```csharp
await LocalNotification.Initialize(Intent);
```

So, your MainActivity will look like this:
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
 
#### iOS

Call Initialize method of LocalNotification in FinishedLaunching method of AppDelegate class:
```csharp
public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
{
     var result = base.FinishedLaunching(application, launchOptions);

     LocalNotification.Initialize(launchOptions);

     return result;
}
```
<!--
<br>


### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| id           | button+input | x       | x   | x       |
| title        | button+input | x       | x   | x       |
| launch       | button+input | x       | x   | x       |
| ui           | button+input |         | x   |         |
| needsAuth    | button+input |         | x   |         |
| icon         | button+input | x       |     |         |
| emptyText    | input        | x       | x   | x       |
| submitTitle  | input        |         | x   |         |
| editable     | input        | x       |     |         |
| choices      | input        | x       |     |         |
| defaultValue | input        |         |     | x       |-->


<br>


### Events
| Event             | Type                                          | Android | iOS | Windows |
| :-----------      | :-----------                                  | :------ | :-- | :------ |
| Tapped            | AsyncEvent<KeyValuePair<string, string>[]>    | x       | x   | x       |


<br>


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| Show         | Task<bool&gt;| title -> string<br> body -> string| x       | x   | x       |
| Schedule     | Task<bool&gt;| title -> string<br> body -> string <br>notifyTime -> DateTime<br> id -> int| x       | x   | x       |
| Cancel       | Task         | id -> int                        | x       | x   | x       |
| Initialize   | Task         | options -> object                        | x       | x   | x       |