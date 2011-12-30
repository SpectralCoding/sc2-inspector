using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQHash {
		public uint Name1 { get; private set; }
		public uint Name2 { get; private set; }
		public uint Locale { get; private set; }
		public uint BlockIndex { get; private set; }
		public static readonly uint Size = 16;

		/// <summary>
		/// Initializes and retrieves data pretaining the an entry in the MPQ Hash Table.
		/// </summary>
		/// <param name="DecryptedBinaryReader">BinaryReader containing decryped Hashtable information.</param>
		public MPQHash(BinaryReader DecryptedBinaryReader) {
			Name1 = DecryptedBinaryReader.ReadUInt32();
			Name2 = DecryptedBinaryReader.ReadUInt32();
			Locale = DecryptedBinaryReader.ReadUInt32();			// Normally 0 or UInt32.MaxValue (0xffffffff)
			BlockIndex = DecryptedBinaryReader.ReadUInt32();
		}
	}
}
