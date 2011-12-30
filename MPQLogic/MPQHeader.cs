using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQHeader {
		// From http://wiki.devklog.net/index.php?title=The_MoPaQ_Archive_Format#Archive_Header :
		/// <summary>00h: char(4) Magic - Indicates that the file is a MoPaQ archive. Must be ASCII "MPQ" 1Ah.</summary>
		public uint Id { get; private set; }				// Signature. Should be 0x1a51504d
		public uint UserDataMaxSize { get; private set; }
		public uint HeaderOffset { get; private set; }
		public uint UserDataSize { get; private set; }
		/// <summary>04h: int32 HeaderSize - Size of the archive header.</summary>
		public uint DataOffset { get; private set; }
		/// <summary>08h: int32 ArchiveSize - Size of the whole archive, including the header. Does not include the strong digital signature, if present. This size is used, among other things, for determining the region to hash in computing the digital signature. This field is deprecated in the Burning Crusade MoPaQ format, and the size of the archive is calculated as the size from the beginning of the archive to the end of the hash table, block table, or extended block table (whichever is largest).</summary>
		public uint ArchiveSize { get; private set; }
		/// <summary>0Ch: int16 FormatVersion - MoPaQ format version. MPQAPI will not open archives where this is negative. Known versions: 0000h - Original format. HeaderSize should be 20h, and large archives are not supported. 0001h - Burning Crusade format. Header size should be 2Ch, and large archives are supported.</summary>
		public ushort MPQVersion { get; private set; }
		/// <summary>0Eh: int8 SectorSizeShift Power of two exponent specifying the number of 512-byte disk sectors in each logical sector in the archive. The size of each logical sector in the archive is 512 * 2^SectorSizeShift. Bugs in the Storm library dictate that this should always be 3 (4096 byte sectors).</summary>
		public ushort BlockSize { get; private set; }
		/// <summary>Should always read "Scarcraft II Replay 11</summary>
		public byte[] StarCraftII { get; private set; }
		public int VersionMajor { get; private set; }
		public int VersionMinor { get; private set; }
		public int VersionPatch { get; private set; }
		public int VersionRevision { get; private set; }
		public int VersionBuild { get; private set; }
		/// <summary>10h: int32 HashTableOffset - Offset to the beginning of the hash table, relative to the beginning of the archive.</summary>
		public uint HashTablePos { get; private set; }
		/// <summary>14h: int32 BlockTableOffset - Offset to the beginning of the block table, relative to the beginning of the archive.</summary>
		public uint BlockTablePos { get; private set; }
		/// <summary>18h: int32 HashTableEntries - Number of entries in the hash table. Must be a power of two, and must be less than 2^16 for the original MoPaQ format, or less than 2^20 for the Burning Crusade format.</summary>
		public uint HashTableSize { get; private set; }
		/// <summary>1Ch: int32 BlockTableEntries - Number of entries in the block table.</summary>
		public uint BlockTableSize { get; private set; }
		public string UserData { get; private set; }
		/// <summary>20h: int64 ExtendedBlockTableOffset - Offset to the beginning of the extended block table, relative to the beginning of the archive.</summary>
		public Int64 ExtendedBlockTableOffset { get; private set; }
		/// <summary>28h: int16 HashTableOffsetHigh - High 16 bits of the hash table offset for large archives.</summary>
		public short HashTableOffsetHigh { get; private set; }
		/// <summary>2Ah: int16 BlockTableOffsetHigh - High 16 bits of the block table offset for large archives.</summary>
		public short BlockTableOffsetHigh { get; private set; }

		public static readonly uint MPQId1A = 0x1A51504D;
		public static readonly uint MPQId1B = 0x1B51504D;
		public static readonly uint Size = 32;

		/// <summary>
		/// Parses all of the MPQ Headers: Parses MPQ1B (if it exists), then MPQ1A.
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
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
				// We end at position 68 (44h). There appears to be some data beyond this point?
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
				// SectorSizeShift is Power of two exponent specifying the number of 512-byte disk sectors in each logical sector in the archive.
				// The size of each logical sector in the archive is 512 * 2^SectorSizeShift.
				// Bugs in the Storm library dictate that this should always be 3 (4096 byte sectors).
				BlockSize = Convert.ToUInt16(0x200 << BinaryReader.ReadUInt16());	// Raise to power of two and multiply by 512 (0x200)
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
