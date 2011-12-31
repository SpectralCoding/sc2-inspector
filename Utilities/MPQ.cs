using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.Utilities {

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

	static class MPQ {

		public static uint[] StormBuffer;

		/// <summary>
		/// Calculates the encryption key based on some assumptions we can make about the headers for encrypted files.
		/// </summary>
		/// <param name="Value0"></param>
		/// <param name="Value1"></param>
		/// <param name="Decrypted"></param>
		/// <returns></returns>
		public static uint DetectFileSeed(uint Value0, uint Value1, uint Decrypted) {
			uint Temp = (Value0 ^ Decrypted) - 0xeeeeeeee;
			for (int i = 0; i < 0x100; i++) {
				uint Seed1 = Temp - StormBuffer[0x400 + i];
				uint Seed2 = 0xeeeeeeee + StormBuffer[0x400 + (Seed1 & 0xff)];
				uint Result = Value0 ^ (Seed1 + Seed2);
				if (Result != Decrypted) { continue; }
				uint SaveSeed1 = Seed1;
				// Test this result against the 2nd value
				Seed1 = ((~Seed1 << 21) + 0x11111111) | (Seed1 >> 11);
				Seed2 = Result + Seed2 + (Seed2 << 5) + 3;
				Seed2 += StormBuffer[0x400 + (Seed1 & 0xff)];
				Result = Value1 ^ (Seed1 + Seed2);
				if ((Result & 0xfffc0000) == 0) { return SaveSeed1; }
			}
			return 0;
		}

		public static uint HashString(string Input, int Offset) {
			uint Seed1 = 0x7fed7fed;
			uint Seed2 = 0xeeeeeeee;
			foreach (char c in Input) {
				int Val = (int)char.ToUpper(c);
				Seed1 = StormBuffer[Offset + Val] ^ (Seed1 + Seed2);
				Seed2 = (uint)Val + Seed1 + Seed2 + (Seed2 << 5) + 3;
			}
			return Seed1;
		}

		/// <summary>
		/// Decrypts Hash Tables and Block Tables
		/// </summary>
		/// <param name="Data">The data to be decrypted.</param>
		/// <param name="Key">The key to use for the decryption.</param>
		public static void DecryptTable(byte[] Data, string Key) {
			StormBuffer = BuildStormBuffer();
			DecryptBlock(Data, HashString(Key, 0x300));
		}

		public static void DecryptBlock(byte[] Data, uint Seed1) {
			uint Seed2 = 0xeeeeeeee;
			// NB: If the block is not an even multiple of 4,
			// the remainder is not encrypted
			for (int i = 0; i < Data.Length - 3; i += 4) {
				Seed2 += StormBuffer[0x400 + (Seed1 & 0xff)];
				uint Result = BitConverter.ToUInt32(Data, i);
				Result ^= (Seed1 + Seed2);
				Seed1 = ((~Seed1 << 21) + 0x11111111) | (Seed1 >> 11);
				Seed2 = Result + Seed2 + (Seed2 << 5) + 3;
				Data[i + 0] = ((byte)(Result & 0xff));
				Data[i + 1] = ((byte)((Result >> 8) & 0xff));
				Data[i + 2] = ((byte)((Result >> 16) & 0xff));
				Data[i + 3] = ((byte)((Result >> 24) & 0xff));
			}
		}

		public static void DecryptBlock(uint[] Data, uint Seed1) {
			uint Seed2 = 0xeeeeeeee;
			for (int i = 0; i < Data.Length; i++) {
				Seed2 += StormBuffer[0x400 + (Seed1 & 0xff)];
				uint Result = Data[i];
				Result ^= Seed1 + Seed2;
				Seed1 = ((~Seed1 << 21) + 0x11111111) | (Seed1 >> 11);
				Seed2 = Result + Seed2 + (Seed2 << 5) + 3;
				Data[i] = Result;
			}
		}


		private static uint[] BuildStormBuffer() {
			uint Seed = 0x100001;
			uint[] Result = new uint[0x500];
			for (uint Index1 = 0; Index1 < 0x100; Index1++) {
				uint Index2 = Index1;
				for (int i = 0; i < 5; i++, Index2 += 0x100) {
					Seed = (Seed * 125 + 3) % 0x2aaaab;
					uint Temp = (Seed & 0xffff) << 16;
					Seed = (Seed * 125 + 3) % 0x2aaaab;
					Result[Index2] = Temp | (Seed & 0xffff);
				}
			}
			return Result;
		}
	}
}
