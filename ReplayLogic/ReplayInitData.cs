using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.MPQLogic;
using SC2Inspector.Utilities;
using SC2Inspector.ViewModel;

namespace SC2Inspector.ReplayLogic {
	public class ReplayInitData {
		public string[] Players;
		public string AccountIdentifier;
		public string Realm;
		public string MapHash;
		public string MapName;

		public ReplayInitData(MPQBlock DetailsBlock, ReplayViewModel ParentRVM) {
			BinaryReader BinaryReader = new BinaryReader(new MemoryStream(DetailsBlock.RawContents));
			int NumPlayers = BinaryReader.ReadByte();
			int NameLen;
			Players = new string[NumPlayers];
			for (int i = 0; i < NumPlayers; i++) {
				NameLen = BinaryReader.ReadByte();
				if (NameLen > 0) {
					Players[i] = Encoding.Default.GetString(BinaryReader.ReadBytes(NameLen));
				} else {
					Players[i] = String.Empty;
				}
				BinaryReader.ReadBytes(5);				// Advance 5 bytes
			}
			BinaryReader.ReadBytes(6);					// Advance 6 unknown bytes
			BinaryReader.ReadBytes(4);					// Advance 4 bytes (string literal "Dflt")
			BinaryReader.ReadBytes(14);					// Advance 15 unknown bytes
			int AccountIdLen = BinaryReader.ReadByte();
			if (AccountIdLen > 0) {
				AccountIdentifier = Encoding.Default.GetString(BinaryReader.ReadBytes(AccountIdLen));
			}
			BinaryReader.ReadBytes(684);				// Some fixed length of data that changes based on the number of players
			string temp;
			while (true) {
				temp = Encoding.Default.GetString(BinaryReader.ReadBytes(4));
				if (temp != "s2ma") { BinaryReader.BaseStream.Position -= 4; break; }
				BinaryReader.ReadBytes(2);				// 0x00 0x00
				Realm = Encoding.Default.GetString(BinaryReader.ReadBytes(2));
				MapHash = Conversion.BytesToHexString(BinaryReader.ReadBytes(32)).ToLower();
				if (Conversion.MapHash.ContainsKey(MapHash)) {
					MapName = Conversion.MapHash[MapHash];
				} else {
					MapName = "Unknown";
				}
			}
			if ((MapName == "Unknown") && !Conversion.MapLocales.ContainsKey(ParentRVM.ReplayDetails.LocalizedMapName)) {
				foreach (KeyValuePair<string, Dictionary<string, string>> MapKVP in Conversion.MapLocales) {
					foreach (KeyValuePair<string, string> LocalesKVP in MapKVP.Value) {
						if (LocalesKVP.Value == ParentRVM.ReplayDetails.LocalizedMapName) {
							MapName = MapKVP.Value["enUS"];
						}
					}
				}
			}
			BinaryReader.ReadBytes(2);
			BinaryReader.ReadBytes(4);
		}

	}
}
