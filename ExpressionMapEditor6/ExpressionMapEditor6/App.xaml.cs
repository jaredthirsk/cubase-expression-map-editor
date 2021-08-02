using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace ExpressionMapEditor6
{
	public partial class App : Application
	{
		protected override Window CreateWindow(IActivationState activationState)
		{
			var window = new Window(new MainPage());
			//window.SetValue(Window.StyleProperty)
			return window;
		}

		//public App()
		//{
		//	InitializeComponent();

		//	MainPage = new MainPage();
		//}
	}
}
