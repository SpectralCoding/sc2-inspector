using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC2Inspector;
using SC2Inspector.ReplayLogic;
using SC2Inspector.MPQLogic;

namespace SC2Inspector.ViewModel {
	public class ReplayViewModel : ViewModelBase {
		private Replay m_Replay;
		private MPQArchive m_MPQArchive;

		public ReplayViewModel() {
			m_Replay = new Replay();
		}

		public void LoadReplay(string Filename) {
			m_Replay.Filename = Filename;
			m_MPQArchive = new MPQArchive(Filename);
		}

	}
}
