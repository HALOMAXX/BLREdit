using BLREdit.UI;
using System.Collections.ObjectModel;

namespace BLREdit.Game.Proxy {
	public sealed class CustomModule {
		public string ModuleName { get; set; } = "CustomModule";
		public string InstallModuleName {
			get {
				return $"Custom-{ModuleName}";
			}
		}
		public UIBool Client { get; set; } = new UIBool(false);
		public UIBool Server { get; set; } = new UIBool(false);
		public ObservableCollection<ProxyModuleSetting>? ModuleSettings { get; set; } = [];
	}
}
