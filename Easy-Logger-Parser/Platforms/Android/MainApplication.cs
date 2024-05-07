using Android.App;
using Android.Runtime;

namespace Easy_Logger_Parser;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication(handle, ownership)
{
	/// <inheritdoc />
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
