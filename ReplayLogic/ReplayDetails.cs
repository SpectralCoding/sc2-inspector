using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.MPQLogic;
using SC2Inspector.Utilities;

namespace SC2Inspector.ReplayLogic {
	public class ReplayDetails {
		public PlayerDetails[] Players;
		public GameSpeed GameSpeed;
		public bool IsPublic;
		public string RealTeamSize;
		public string TeamSize;
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
				Players[i].ColorAlphaDetails = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[0].LongData);
				Players[i].ColorRedDetails = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[1].LongData);
				Players[i].ColorGreenDetails = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[2].LongData);
				Players[i].ColorBlueDetails = Convert.ToInt32(CurrentPlayer.SerialData[3].SerialData[3].LongData);
				Players[i].HandicapDetails = Convert.ToInt32(CurrentPlayer.SerialData[6].LongData);
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
		public int Id;
		public int RealId;
		public string LocalizedRaceName;
		public int ColorAlphaDetails;
		public int ColorRedDetails;
		public int ColorGreenDetails;
		public int ColorBlueDetails;
		public int HandicapDetails;
		public int Team;
		public Difficulty Difficulty;
		public Race StartingRace;
		public bool IsCPU;
		public int ColorIndex;
		public Color Color;
		public int HandicapAttrib;
	}

	public enum Difficulty {
		VeryEasy,
		Easy,
		Medium,
		Hard,
		VeryHard,
		Insane
	}

	public enum Race {
		Zerg,
		Terran,
		Protoss
	}

	public enum Color {
		Unknown,
		Red,
		Blue,
		Teal,
		Purple,
		Yellow,
		Orange,
		Green,
		LightPink,
		Violet,
		LightGray,
		DarkGreen,
		Brown,
		LightGreen,
		DarkGray,
		Pink
	}

	public enum GameSpeed {
		Unknown,
		Slower,
		Slow,
		Normal,
		Fast,
		Faster
	}

}
