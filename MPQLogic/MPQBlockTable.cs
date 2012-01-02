using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.Utilities;

namespace SC2Inspector.MPQLogic {
	public class MPQBlockTable : Hashtable {

		public new MPQBlock this[object key] {
			get { return (MPQBlock)base[key]; }
			set { base[key] = value; }
		}

		/// <summary>
		/// Initializes and populates the MPQ Block Table.
		/// </summary>
		/// <param name="BlockTableData">Byte array containing the raw block table data.</param>
		/// <param name="BlockTableSize">Number of blocks in the array. Byte size should actually be this value multiplied by 16.</param>
		/// <param name="HeaderOffset">Offset of the header. Should always be 1024.</param>
		public MPQBlockTable(byte[] BlockTableData, int BlockTableSize, uint HeaderOffset) {
			MPQ.DecryptTable(BlockTableData, "(block table)");
			BinaryReader DecryptedBinaryReader = new BinaryReader(new MemoryStream(BlockTableData));
			for (uint i = 0; i < BlockTableSize; i++) {
				MPQBlock MPQBlockObj = new MPQBlock(DecryptedBinaryReader, HeaderOffset);
				this[i] = MPQBlockObj;
			}
		}

	}
}
