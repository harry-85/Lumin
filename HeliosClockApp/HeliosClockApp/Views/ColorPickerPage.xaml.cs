using System;
using HeliosClockApp.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HeliosClockApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ColorPickerPage : ContentPage
	{
		
		public ColorPickerPage()
		{
			InitializeComponent();
			ColorPicker.PickedColorChanged += ColorPicker_PickedColorChanged;
		}

		private void ColorPicker_PickedColorChanged(object sender, System.Drawing.Color e)
		{
			var viewModel = (ColorPickerModel)BindingContext;

			viewModel.SelectedColor = e;

			viewModel.SetColorFromPickerCommand.Execute(e);
		}
	}
}