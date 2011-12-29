using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC2Inspector.MPQLogic {

	[Flags]
	public enum MPQFileFlags : uint {
		CompressedPK = 0x100,					// AKA Imploded
		CompressedMulti = 0x200,
		Compressed = 0xff00,
		Encrypted = 0x10000,
		BlockOffsetAdjustedKey = 0x020000,		// AKA FixSeed
		SingleUnit = 0x1000000,
		FileHasMetadata = 0x04000000,			// Appears in WoW 1.10 or newer.  Indicates the file has associated metadata.
		Exists = 0x80000000
	}

	static class MPQUtilities {

		public static uint[] sStormBuffer;

		public static uint HashString(string input, int offset) {
			uint seed1 = 0x7fed7fed;
			uint seed2 = 0xeeeeeeee;

			foreach (char c in input) {
				int val = (int)char.ToUpper(c);
				seed1 = sStormBuffer[offset + val] ^ (seed1 + seed2);
				seed2 = (uint)val + seed1 + seed2 + (seed2 << 5) + 3;
			}
			return seed1;
		}

		// Used for Hash Tables and Block Tables
		public static void DecryptTable(byte[] data, string key) {
			sStormBuffer = BuildStormBuffer();
			DecryptBlock(data, HashString(key, 0x300));
		}

		public static void DecryptBlock(byte[] data, uint seed1) {
			uint seed2 = 0xeeeeeeee;

			// NB: If the block is not an even multiple of 4,
			// the remainder is not encrypted
			for (int i = 0; i < data.Length - 3; i += 4) {
				seed2 += sStormBuffer[0x400 + (seed1 & 0xff)];

				uint result = BitConverter.ToUInt32(data, i);
				result ^= (seed1 + seed2);

				seed1 = ((~seed1 << 21) + 0x11111111) | (seed1 >> 11);
				seed2 = result + seed2 + (seed2 << 5) + 3;

				data[i + 0] = ((byte)(result & 0xff));
				data[i + 1] = ((byte)((result >> 8) & 0xff));
				data[i + 2] = ((byte)((result >> 16) & 0xff));
				data[i + 3] = ((byte)((result >> 24) & 0xff));
			}
		}

		private static uint[] BuildStormBuffer() {
			uint seed = 0x100001;

			uint[] result = new uint[0x500];

			for (uint index1 = 0; index1 < 0x100; index1++) {
				uint index2 = index1;
				for (int i = 0; i < 5; i++, index2 += 0x100) {
					seed = (seed * 125 + 3) % 0x2aaaab;
					uint temp = (seed & 0xffff) << 16;
					seed = (seed * 125 + 3) % 0x2aaaab;

					result[index2] = temp | (seed & 0xffff);
				}
			}

			return result;
		}



	}
}
