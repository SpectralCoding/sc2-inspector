using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQHashTable : Hashtable {

		/// <summary>
		/// Initializes and populates the MPQ Hash Table
		/// </summary>
		/// <param name="HashTableData">Byte array containing the raw hash table data.</param>
		/// <param name="HashTableSize">Number of hashes in the array. Byte size should actually be this value multiplied by 16.</param>
		public MPQHashTable(byte[] HashTableData, int HashTableSize) {
			MPQUtilities.DecryptTable(HashTableData, "(hash table)");
			BinaryReader DecryptedBinaryReader = new BinaryReader(new MemoryStream(HashTableData));
			for (int i = 0; i < HashTableSize; i++) {
				MPQHash MPQHashObj = new MPQHash(DecryptedBinaryReader);
				this[i] = MPQHashObj;
			}
		}

		/// <summary>
		/// Retrieves a MPQHash object by filename.
		/// </summary>
		/// <param name="Filename">Filename of the hash to be retrieved.</param>
		/// <returns>A MPQHash object containing information about the file including BlockIndex.</returns>
		public MPQHash GetHashByFilename(string Filename) {
			uint IndexHash = MPQUtilities.HashString(Filename, 0);
			uint Name1 = MPQUtilities.HashString(Filename, 0x100);
			uint Name2 = MPQUtilities.HashString(Filename, 0x200);
			for (int i = 0; i < this.Count; i++) {
				MPQHash Temp = (MPQHash)this[i];
				if ((Temp.Name1 == Name1) && (Temp.Name2 == Name2)) {
					return Temp;
				}
			}
			return null;
		}

	}
}
