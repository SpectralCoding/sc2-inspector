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
				int NumberOfElements = MPQUtilities.ParseVLFNumber(BinaryReader);
				int Index = MPQUtilities.ParseVLFNumber(BinaryReader);
				DataType = BinaryReader.ReadByte();								// Should be 0x2 (Binary Data)
				NumberOfElements = MPQUtilities.ParseVLFNumber(BinaryReader);
				StarCraftII = BinaryReader.ReadBytes(NumberOfElements);
				Index = MPQUtilities.ParseVLFNumber(BinaryReader);
				DataType = BinaryReader.ReadByte();
				NumberOfElements = MPQUtilities.ParseVLFNumber(BinaryReader);
				int[] Version = new int[NumberOfElements];
				while (NumberOfElements > 0) {
					Index = MPQUtilities.ParseVLFNumber(BinaryReader);
					DataType = BinaryReader.ReadByte();
					if (DataType == 0x09) { Version[Index] = MPQUtilities.ParseVLFNumber(BinaryReader); }
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

	}
}
