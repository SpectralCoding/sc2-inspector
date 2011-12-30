using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace SC2Inspector.MPQLogic {
	class MPQBlock {

		public uint CompressedSize { get; private set; }
		public uint FileSize { get; private set; }
		public MPQFileFlags Flags { get; internal set; }
		public uint EncryptionSeed { get; internal set; }
		public byte[] CompressedContents { get; private set; }
		public byte[] RawContents { get; private set; }
		private uint m_FileOffset;							// Relative to the header offset
		internal uint FilePos { get; private set; }			// Absolute position in the file
		private uint[] BlockPositions;

		public bool IsCompressed { get { return (Flags & MPQFileFlags.Compressed) != 0; } }
		public bool IsEncrypted { get { return (Flags & MPQFileFlags.Encrypted) != 0; } }
		public bool Exists { get { return Flags != 0; } }
		public bool IsSingleUnit { get { return (Flags & MPQFileFlags.SingleUnit) != 0; } }

		/// <summary>
		/// Initializes and retrieves Block metadata.
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		/// <param name="HeaderOffset">Offset of the header. Should always be 1024.</param>
		public MPQBlock(BinaryReader BinaryReader, uint HeaderOffset) {
			m_FileOffset = BinaryReader.ReadUInt32();
			FilePos = HeaderOffset + m_FileOffset;
			CompressedSize = BinaryReader.ReadUInt32();
			FileSize = BinaryReader.ReadUInt32();
			Flags = (MPQFileFlags)BinaryReader.ReadUInt32();
			EncryptionSeed = 0;
		}

		/// <summary>
		/// Retrieves and decompresses the internal file within the MPQ.
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		/// <param name="BlockSize">Size of each block of data. Should always be 4096.</param>
		public void PopulateFileContents(BinaryReader BinaryReader, uint BlockSize) {
			BinaryReader.BaseStream.Seek(FilePos, SeekOrigin.Begin);
			if (IsCompressed && !IsSingleUnit) {
				// Come back to this when we need to deal with files which meet the following:
				//     File is split into sectors rather than stored as a single unit.
				// Edit: No SC2 replay files are split up this way.
				LoadBlockPositions(BinaryReader, BlockSize);
			}
			CompressedContents = BinaryReader.ReadBytes(Convert.ToInt32(CompressedSize));
			RawContents = Decompress(FileSize);
		}

		/// <summary>
		/// Decompresses the contentsd of CompressedContents.
		/// </summary>
		/// <param name="OutputLength">Expected length of the decompressed output.</param>
		/// <returns>A byte array containing the decompressed data. Should be of length OutputLength.</returns>
		public byte[] Decompress(uint OutputLength) {
			byte[] Decompressed = new byte[OutputLength];
			Stream Stream = new MemoryStream(CompressedContents);
			byte CompressionType = (byte)Stream.ReadByte();
			switch (CompressionType) {
				case 0x02:
					// GZip Compressed
					Stream ZlibStream = new InflaterInputStream(Stream);
					int Offset = 0;
					while (OutputLength > 0) {
						int size = ZlibStream.Read(Decompressed, Offset, Convert.ToInt32(OutputLength));
						if (size == 0) break;
						Offset += size;
						OutputLength -= (uint)size;
					}
					break;
				case 0x10:
					// BZip2 Compressed
					MemoryStream OutMemoryStream = new MemoryStream(Convert.ToInt32(OutputLength));
					BZip2.Decompress(Stream, OutMemoryStream, false);
					Decompressed = OutMemoryStream.ToArray();
					break;
				default:
					throw new Exception("Unknown Compression Type.");
			}
			return Decompressed;
		}

		/// <summary>
		/// For multi-locationed files this enumerates the block locations of each piece. Not in use.
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		/// <param name="BlockSize">Size of each block of data. Should always be 4096.</param>
		public void LoadBlockPositions(BinaryReader BinaryReader, uint BlockSize) {
			// This method shouldn't actually be necessary for SC2Replays but i'll leave it here because it is done.
			uint BlockPositionSize;
			int BlockPositionCount = Convert.ToInt32((FileSize + BlockSize - 1) / BlockSize) + 1;
			// Files with metadata have an extra block containing block checksums
			if ((Flags & MPQFileFlags.FileHasMetadata) != 0) {
				BlockPositionCount++;
			}
			BlockPositions = new uint[BlockPositionCount];
			BinaryReader.BaseStream.Seek(FilePos, SeekOrigin.Begin);
			for (int i = 0; i < BlockPositionCount; i++) {
				BlockPositions[i] = BinaryReader.ReadUInt32();
			}
			BlockPositionSize = Convert.ToUInt32(BlockPositionCount * 4);
			if (IsEncrypted) {
				if (EncryptionSeed == 0) {
					EncryptionSeed = MPQUtilities.DetectFileSeed(BlockPositions[0], BlockPositions[1], BlockPositionSize) + 1;
					if (EncryptionSeed == 1) {
						throw new Exception("Unable to determine encryption seed.");
					}
				}
				MPQUtilities.DecryptBlock(BlockPositions, EncryptionSeed - 1);
				if (BlockPositions[0] != BlockPositionSize) { throw new Exception("Decryption Failed."); }
				if (BlockPositions[1] > (BlockSize + BlockPositionSize)) { throw new Exception("Decryption Failed."); }
			}
		}
	}
}
