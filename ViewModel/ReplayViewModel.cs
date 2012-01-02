using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SC2Inspector;
using SC2Inspector.ReplayLogic;
using SC2Inspector.MPQLogic;

namespace SC2Inspector.ViewModel {
	public class ReplayViewModel : ViewModelBase {
		public Replay Replay;
		public ReplayDetails ReplayDetails;
		public ReplayAttributesEvents ReplayAttributesEvents;
		public ReplayInitData ReplayInitData;
		public MPQArchive MPQArchive;

		public ReplayViewModel() {
			Replay = new Replay();
		}

		public void LoadReplay(string Filename) {
			Replay.Filename = Filename;
			MPQArchive = new MPQArchive(Filename);
			ReplayDetails = new ReplayDetails(MPQArchive.GetFile("replay.details"));
			ReplayAttributesEvents = new ReplayAttributesEvents(MPQArchive.GetFile("replay.attributes.events"), this);
			ReplayInitData = new ReplayInitData(MPQArchive.GetFile("replay.initData"), this);
		}

	}
}
