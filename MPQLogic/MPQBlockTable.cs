using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQBlockTable : Hashtable {

		public MPQBlockTable(byte[] i_BlockTableData, int i_BlockTableSize, uint i_HeaderOffset) {
			MPQUtilities.DecryptTable(i_BlockTableData, "(block table)");
			BinaryReader DecryptedBinaryReader = new BinaryReader(new MemoryStream(i_BlockTableData));
			for (uint i = 0; i < i_BlockTableSize; i++) {
				MPQBlock MPQBlockObj = new MPQBlock(DecryptedBinaryReader, i_HeaderOffset);
				this[i] = MPQBlockObj;
			}
		}

	}
}
