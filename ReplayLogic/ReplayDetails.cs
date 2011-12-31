using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.MPQLogic;
using SC2Inspector.Utilities;

namespace SC2Inspector.ReplayLogic {
	class ReplayDetails {
		public PlayerDetails[] Players;
		public string LocalizedMapName;
		public string MapPreviewFilename;
		public DateTime SaveTimeUTC;
		public int SaveUTCOffset;				// UTC offset in seconds (positive or negative)

		public ReplayDetails(MPQBlock DetailsBlock) {
			BinaryReader BinaryReader = new BinaryReader(new MemoryStream(DetailsBlock.RawContents));
			SerializedData DetailData = LowLevel.ParseSerializedData(BinaryReader);
			Players = new PlayerDetails[DetailData.SerialData[0].SerialData.Count];
			for (int i = 0; i < DetailData.SerialData[0].SerialData.Count; i++) {
				SerializedData CurrentPlayer = DetailData.SerialData[0].SerialData[i];
				Players[i] = new PlayerDetails();
				Players[i].Name = Encoding.Default.GetString(CurrentPlayer.SerialData[0].ByteArrData);
				Players[i].RealId = Convert.ToInt32(CurrentPlayer.SerialData[1].SerialData[4].LongData);
				Players[i].LocalizedRaceName = Encoding.Default.GetString(CurrentPlayer.SerialData[2].ByteArrData);
				Players[i].ColorAlpha = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[0].LongData);
				Players[i].ColorRed = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[1].LongData);
				Players[i].ColorGreen = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[2].LongData);
				Players[i].ColorBlue = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[3].LongData);
				Players[i].Handicap = Convert.ToInt32(CurrentPlayer.SerialData[6].LongData);
				Players[i].Team = Convert.ToInt32(CurrentPlayer.SerialData[7].LongData);
			}
			LocalizedMapName = Encoding.Default.GetString(DetailData.SerialData[1].ByteArrData);
			MapPreviewFilename = Encoding.Default.GetString(DetailData.SerialData[3].SerialData[0].ByteArrData);
			SaveTimeUTC = (new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(Math.Floor(((DetailData.SerialData[5].LongData - 116444735995904000.0)) / 10000000.0));
			SaveUTCOffset = Convert.ToInt32(Math.Floor(DetailData.SerialData[6].LongData / 10000000.0));
		}

	}

	public struct PlayerDetails {
		public string Name;
		public int RealId;
		public string LocalizedRaceName;
		public int ColorAlpha;
		public int ColorRed;
		public int ColorGreen;
		public int ColorBlue;
		public int Handicap;
		public int Team;
	}
}
