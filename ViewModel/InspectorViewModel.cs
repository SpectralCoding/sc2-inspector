using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC2Inspector.ViewModel {
	public class InspectorViewModel : ViewModelBase {
		public List<ReplayViewModel> ReplayList = new List<ReplayViewModel>();

		public InspectorViewModel() {
			
		}

		public void LoadReplay(string Filename) {
			ReplayViewModel tempRVM = new ReplayViewModel();
			tempRVM.LoadReplay(Filename);
			ReplayList.Add(tempRVM);
		}

	}
}
