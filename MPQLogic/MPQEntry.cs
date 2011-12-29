using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQEntry {

		public uint CompressedSize { get; private set; }
		public uint FileSize { get; private set; }
		public MPQFileFlags Flags { get; internal set; }
		public uint EncryptionSeed { get; internal set; }
		private uint m_FileOffset; // Relative to the header offset
		internal uint FilePos { get; private set; } // Absolute position in the file
		private string m_Filename;

		public MPQEntry(BinaryReader br, uint headerOffset) {
			m_FileOffset = br.ReadUInt32();
			FilePos = headerOffset + m_FileOffset;
			CompressedSize = br.ReadUInt32();
			FileSize = br.ReadUInt32();
			Flags = (MPQFileFlags)br.ReadUInt32();
			EncryptionSeed = 0;
		}

	}
}
