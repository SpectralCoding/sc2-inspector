using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.MPQLogic;
using SC2Inspector.ViewModel;
using SC2Inspector.Utilities;

namespace SC2Inspector.ReplayLogic {
	public class ReplayAttributesEvents {

		public ReplayAttributesEvents(MPQBlock DetailsBlock, ReplayViewModel ParentRVM) {
			BinaryReader BinaryReader = new BinaryReader(new MemoryStream(DetailsBlock.RawContents));
			Dictionary<uint, Dictionary<int, string>> AttribDict = new Dictionary<uint, Dictionary<int, string>>();
			BinaryReader.ReadBytes(4);						// Skip 4 Byte header
			if (ParentRVM.MPQArchive.MPQHeader.VersionBuild >= 17326) {
				BinaryReader.ReadByte();
			}
			#region Enumerate all attributes into a nested Dictionary<uint, Dictionary<int, string>>
			uint NumAttribs = BinaryReader.ReadUInt32();
			uint AttribHeader;
			uint AttribId;
			int PlayerId;
			string AttribVal;
			int NumSlots;
			for (int i = 0; i < NumAttribs; i++) {
				AttribHeader = BinaryReader.ReadUInt32();
				AttribId = BinaryReader.ReadUInt32();
				PlayerId = BinaryReader.ReadByte();
				AttribVal = Conversion.ReverseString(Encoding.Default.GetString(BinaryReader.ReadBytes(4))).Replace("\0", String.Empty);
				if (!AttribDict.ContainsKey(AttribId)) {
					AttribDict.Add(AttribId, new Dictionary<int, string>());
				}
				AttribDict[AttribId].Add(PlayerId, AttribVal);
			}
			if (NumAttribs == 0) {
				throw new Exception("Zero attributes.");
			}
			#endregion
			#region Set Player Ids
			if (AttribDict.ContainsKey(0x01F4)) {
				PlayerId = 0;
				int i;
				for (i = 1; AttribDict[0x01F4].ContainsKey(i); i++) {
					if (AttribDict[0x01F4][i] == "Open") {
						throw new Exception("\"Open\" in Attributes Events!");
					}
					ParentRVM.ReplayDetails.Players[i - 1].Id = i;
				}
				NumSlots = i;
			} else {
				NumSlots = 0;
			}
			#endregion
			#region Get team locations within file
			uint TeamAttribLocation;
			switch (AttribDict[0x07D1][0x10]) {
				case "1v1": TeamAttribLocation = 0x07D2; break;
				case "2v2": TeamAttribLocation = 0x07D3; break;
				case "3v3": TeamAttribLocation = 0x07D4; break;
				case "4v4": TeamAttribLocation = 0x07D5; break;
				case "FFA": TeamAttribLocation = 0x07D6; break;
				default:
					throw new Exception("Unknown game mode in Attibutes Events!");
			}
			// Custom games have different values. Not tested thoroughly, may be wrong.
			switch (AttribDict[0x07D0][0x10]) {
				case "Cust":
					if (AttribDict[0x03E9][0x10] == "no") {
						TeamAttribLocation += 10;
					}
					break;
			}
			#endregion
			Hashtable TempTeamSize = new Hashtable();
			int CurPlayerNum = -1;
			for (int i = 1; i < NumSlots; i++) {
				#region Find this Player
				for (int x = 0; x < ParentRVM.ReplayDetails.Players.Length; x++) {
					if (ParentRVM.ReplayDetails.Players[x].Id == i) {
						CurPlayerNum = i;
					}
				}
				if (CurPlayerNum == -1) { continue; }
				PlayerDetails CurPlayer = ParentRVM.ReplayDetails.Players[CurPlayerNum - 1];
				#endregion
				CurPlayer.HandicapAttrib = Convert.ToInt32(AttribDict[0x0BBB][i]);
				#region Difficulty
				switch (AttribDict[0x0BBC][i]) {
					case "VyEy": CurPlayer.Difficulty = Difficulty.VeryEasy; break;
					case "Easy": CurPlayer.Difficulty = Difficulty.Easy; break;
					case "Medi": CurPlayer.Difficulty = Difficulty.Medium; break;
					case "Hard": CurPlayer.Difficulty = Difficulty.Hard; break;
					case "VyHd": CurPlayer.Difficulty = Difficulty.VeryHard; break;
					case "Insa": CurPlayer.Difficulty = Difficulty.Insane; break;
				}
				#endregion
				#region Race
				switch (AttribDict[0x0BB9][i]) {
					case "Zerg": CurPlayer.StartingRace = Race.Zerg; break;
					case "Terr": CurPlayer.StartingRace = Race.Terran; break;
					case "Prot": CurPlayer.StartingRace = Race.Protoss; break;
					default: throw new Exception("Unknown Race.");
				}
				#endregion
				if (AttribDict[0x01F4][i] == "Comp") { CurPlayer.IsCPU = true; } else { CurPlayer.IsCPU = false; }
				#region Color
				CurPlayer.ColorIndex = Convert.ToInt32(AttribDict[0x0BBA][i].Substring(2));
				switch (CurPlayer.ColorIndex) {
					case 1: CurPlayer.Color = Color.Red; break;
					case 2: CurPlayer.Color = Color.Blue; break;
					case 3: CurPlayer.Color = Color.Teal; break;
					case 4: CurPlayer.Color = Color.Purple; break;
					case 5: CurPlayer.Color = Color.Yellow; break;
					case 6: CurPlayer.Color = Color.Orange; break;
					case 7: CurPlayer.Color = Color.Green; break;
					case 8: CurPlayer.Color = Color.LightPink; break;
					case 9: CurPlayer.Color = Color.Violet; break;
					case 10: CurPlayer.Color = Color.LightGray; break;
					case 11: CurPlayer.Color = Color.DarkGreen; break;
					case 12: CurPlayer.Color = Color.Brown; break;
					case 13: CurPlayer.Color = Color.LightGreen; break;
					case 14: CurPlayer.Color = Color.DarkGray; break;
					case 15: CurPlayer.Color = Color.Pink; break;
					default: CurPlayer.Color = Color.Unknown; break;
				}
				#endregion
				#region Team
				CurPlayer.Team = Convert.ToInt32(AttribDict[TeamAttribLocation][i].Substring(1));
				if (!TempTeamSize.ContainsKey(CurPlayer.Team)) {
					TempTeamSize[CurPlayer.Team] = 1;
				} else {
					TempTeamSize[CurPlayer.Team] = (int)TempTeamSize[CurPlayer.Team] + 1;
				}
				#endregion
				ParentRVM.ReplayDetails.Players[CurPlayerNum - 1] = CurPlayer;
				CurPlayerNum = -1;
			}
			#region Overall Team Size
			foreach (DictionaryEntry Item in TempTeamSize) {
				ParentRVM.ReplayDetails.RealTeamSize += "v" + Item.Value;
			}
			if (ParentRVM.ReplayDetails.RealTeamSize.Length > 0) {
				ParentRVM.ReplayDetails.RealTeamSize = ParentRVM.ReplayDetails.RealTeamSize.Substring(1);
			} else {
				ParentRVM.ReplayDetails.RealTeamSize = "0v0";
			}
			#endregion
			ParentRVM.ReplayDetails.TeamSize = AttribDict[0x07D1][0x10];
			#region Game Speed
			switch (AttribDict[0x0BB8][0x10]) {
				case "Slor": ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Slower; break;
				case "Slow": ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Slow; break;
				case "Norm": ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Normal; break;
				case "Fast": ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Fast; break;
				case "Fasr": ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Faster; break;
				default: ParentRVM.ReplayDetails.GameSpeed = GameSpeed.Unknown; break;
			}
			#endregion
			if (AttribDict[0x0BC1][0x10] == "Priv") {
				ParentRVM.ReplayDetails.IsPublic = false;
			} else {
				ParentRVM.ReplayDetails.IsPublic = true;
			}
		}
	}

}
