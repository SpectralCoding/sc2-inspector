using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC2Inspector.ViewModel {
	public class InspectorViewModel : ViewModelBase {
		public ReplayViewModel ReplayViewModel;

		public InspectorViewModel() {
			ReplayViewModel = new ReplayViewModel();
		}
	}
}
