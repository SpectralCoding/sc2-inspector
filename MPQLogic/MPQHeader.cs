using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQHeader {
		public uint Id { get; private set; }				// Signature. Should be 0x1a51504d
		public uint UserDataMaxSize { get; private set; }
		public uint HeaderOffset { get; private set; }
		public uint UserDataSize { get; private set; }
		public uint DataOffset { get; private set; }		// Offset of the first file.  AKA Header size
		public uint ArchiveSize { get; private set; }
		public ushort MPQVersion { get; private set; }		// Most are 0.  Burning Crusade = 1
		public ushort BlockSize { get; private set; }		// Size of file block is 0x200 << BlockSize
		public byte[] StarCraftII { get; private set; }		// This should always read "Starcraft II Replay 11"
		public int VersionMajor { get; private set; }
		public int VersionMinor { get; private set; }
		public int VersionPatch { get; private set; }
		public int VersionRevision { get; private set; }
		public int VersionBuild { get; private set; }
		public uint HashTablePos { get; private set; }
		public uint BlockTablePos { get; private set; }
		public uint HashTableSize { get; private set; }
		public uint BlockTableSize { get; private set; }
		public string UserData { get; private set; }
		// Version 1 fields
		// The extended block table is an array of Int16 - higher bits of the offests in the block table.
		public Int64 ExtendedBlockTableOffset { get; private set; }
		public short HashTableOffsetHigh { get; private set; }
		public short BlockTableOffsetHigh { get; private set; }

		public static readonly uint MPQId1A = 0x1A51504D;
		public static readonly uint MPQId1B = 0x1B51504D;
		public static readonly uint Size = 32;

		public MPQHeader(BinaryReader BinaryReader) {
			uint id = BinaryReader.ReadUInt32();
			if (id == MPQId1B) {
				Id = id;
				UserDataMaxSize = BinaryReader.ReadUInt32();
				HeaderOffset = BinaryReader.ReadUInt32();
				UserDataSize = BinaryReader.ReadUInt32();
				int DataType = BinaryReader.ReadByte();							// Should be 0x05 (Array with Keys)
				int NumberOfElements = ParseVLFNumber(BinaryReader);
				int Index = ParseVLFNumber(BinaryReader);
				DataType = BinaryReader.ReadByte();								// Should be 0x2 (Binary Data)
				NumberOfElements = ParseVLFNumber(BinaryReader);
				StarCraftII = BinaryReader.ReadBytes(NumberOfElements);
				Index = ParseVLFNumber(BinaryReader);
				DataType = BinaryReader.ReadByte();
				NumberOfElements = ParseVLFNumber(BinaryReader);
				int[] Version = new int[NumberOfElements];
				while (NumberOfElements > 0) {
					Index = ParseVLFNumber(BinaryReader);
					DataType = BinaryReader.ReadByte();
					if (DataType == 0x09) { Version[Index] = ParseVLFNumber(BinaryReader); }
					else if (DataType == 0x06) { Version[Index] = BinaryReader.ReadByte(); }
					else if (DataType == 0x07) { Version[Index] = BitConverter.ToInt32(BinaryReader.ReadBytes(4), 0); }
					NumberOfElements--;
				}
				VersionMajor = Version[0];
				VersionMinor = Version[1];
				VersionPatch = Version[2];
				VersionRevision = Version[3];
				VersionBuild = Version[4];
				// We end at position 68 (44h). Their appears to be some data beyond this point?
				// A possible option is below (multiple replays):
				// 0409040609FE9E05 = 04-09 04-06 09-FE9E05
				// 0409040609E88306 = 04-09 04-06 09-E88306
				// 040904060996F403 = 04-09 04-06 09-96F403
				BinaryReader.ReadBytes(Convert.ToInt32(HeaderOffset - BinaryReader.BaseStream.Position));
			}
			id = BinaryReader.ReadUInt32();
			if (id == MPQId1A) {
				Id = id;
				DataOffset = BinaryReader.ReadUInt32() + HeaderOffset;
				ArchiveSize = BinaryReader.ReadUInt32();
				MPQVersion = BinaryReader.ReadUInt16();
				BlockSize = BinaryReader.ReadUInt16();
				HashTablePos = BinaryReader.ReadUInt32() + HeaderOffset;
				BlockTablePos = BinaryReader.ReadUInt32() + HeaderOffset;
				HashTableSize = BinaryReader.ReadUInt32();
				BlockTableSize = BinaryReader.ReadUInt32();
				if (MPQVersion == 1) {
					ExtendedBlockTableOffset = BinaryReader.ReadInt64();
					HashTableOffsetHigh = BinaryReader.ReadInt16();
					BlockTableOffsetHigh = BinaryReader.ReadInt16();
				}
			}
		}

		/*
		public Dictionary<int, object> ParseSerializedData(BinaryReader BinaryReader) {
			Dictionary<int, object> returnLst = new Dictionary<int, object>();
			Byte DataType = BinaryReader.ReadByte();
			int NumberOfElements;
			switch (DataType) {
				case 0x02:
					returnLst.Add(returnLst.Count - 1, ParseVLFNumber(BinaryReader));
					return returnLst;
				case 0x04:
					BinaryReader.ReadBytes(2);
					NumberOfElements = ParseVLFNumber(BinaryReader);
					while (NumberOfElements > 0) {
						returnLst.Add(returnLst.Count - 1, ParseSerializedData(BinaryReader));
						NumberOfElements--;
					}
					return returnLst;
				case 0x05:
					NumberOfElements = ParseVLFNumber(BinaryReader);
					while (NumberOfElements > 0) {
						int index = ParseVLFNumber(BinaryReader);
						returnLst.Add(index, ParseSerializedData(BinaryReader));
						NumberOfElements--;
					}
					return returnLst;
				case 0x06:
					returnLst.Add(returnLst.Count - 1, BinaryReader.ReadByte());
					return returnLst;
				case 0x07:
					returnLst.Add(returnLst.Count - 1, BinaryReader.ReadUInt32());
					return returnLst;
				case 0x09:
					returnLst.Add(returnLst.Count - 1, ParseVLFNumber(BinaryReader));
					return returnLst;
				default:
					throw new Exception("Unknown DataType!");
			}
			return returnLst;
		}
		*/

		private static int ParseVLFNumber(BinaryReader reader) {
			var bytes = 0;
			var first = true;
			var number = 0;
			var multiplier = 1;
			while (true) {
				var i = reader.ReadByte();
				number += (i & 0x7F) * (int)Math.Pow(2, bytes * 7);
				if (first) {
					if ((number & 1) != 0) {
						multiplier = -1;
						number--;
					}
					first = false;
				}
				if ((i & 0x80) == 0) {
					break;
				}
				bytes++;
			}
			return (number / 2) * multiplier;
		}
	}
}
