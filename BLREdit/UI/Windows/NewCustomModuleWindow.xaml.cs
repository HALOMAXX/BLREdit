using BLREdit.Game.Proxy;

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
using System.Windows.Shapes;

namespace BLREdit.UI.Windows {
	/// <summary>
	/// Interaction logic for NewCustomModuleWindow.xaml
	/// </summary>
	public partial class NewCustomModuleWindow : Window {
		public NewCustomModuleWindow() {
			InitializeComponent();
		}

		private void AllClientEnable_Click(object sender, RoutedEventArgs e) {
			if(DataContext is CustomModule customModule) {
				if(DataStorage.GameClients.Count <= 0) {
					MessageBox.Show("You currenlty have no clients to activate this module.\nOnce you added a client don't forget to add this module in the client config window,\nthe small button with a wrench as icon in the client panel within the Client Tab.");
					return;
				}
				foreach(var client in DataStorage.GameClients) {
					bool exists = false;
					foreach(var module in client.CustomModules) {
						if(module.ModuleName == customModule.ModuleName) {
							exists = true;
							break;
						}
					}
					if(!exists) {
						client.CustomModules.Add(customModule);
					} else {
						//TODO: Show error
					}
				}
			}
        }
    }
}
