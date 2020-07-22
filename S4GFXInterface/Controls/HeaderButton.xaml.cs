using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace S4GFXInterface.Controls
{
	/// <summary>
	/// Interaktionslogik für HeaderButton.xaml
	/// </summary>
	public partial class HeaderButton : UserControl
	{
		public object indexItem;
		public TabControl control;

		public void Refresh() {
			if (control.SelectedItem == indexItem) {
				Butt.BorderBrush = (Brush)Application.Current.Resources["HeaderHighlightColor"];
				Butt.BorderThickness = new Thickness(1, 1, 1, 0);

				Butt.Background = (Brush)Application.Current.Resources["HeaderHighlightBackColor"];
			} else {
				Butt.BorderBrush = (Brush)Application.Current.Resources["HeaderStandardColor"];
				Butt.BorderThickness = new Thickness(1, 1, 1, 1);

				Butt.Background = (Brush)Application.Current.Resources["HeaderStandardBackColor"];
			}
		}

		public Action OnClick;

		public HeaderButton() {
			InitializeComponent();
			this.DataContext = this;
		}

		public string Heading { get; set; }
		public string ImageSource { get; set; }

		private void Butt_Click(object sender, RoutedEventArgs e) {
			control.SelectedItem = indexItem;

			OnClick?.Invoke();
		}
	}
}
