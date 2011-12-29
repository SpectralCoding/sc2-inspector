using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQHashTable : Hashtable {

		public MPQHashTable(byte[] i_HashTableData, int i_HashTableSize) {
			MPQUtilities.DecryptTable(i_HashTableData, "(hash table)");
			BinaryReader DecryptedBinaryReader = new BinaryReader(new MemoryStream(i_HashTableData));
			for (int i = 0; i < i_HashTableSize; i++) {
				MPQHash MPQHashObj = new MPQHash(DecryptedBinaryReader);
				this[i] = MPQHashObj;
				//Console.WriteLine("{0} - {1} - {2} - {3}", MPQHashObj.BlockIndex, MPQHashObj.Locale, MPQHashObj.Name1, MPQHashObj.Name2);
			}
		}

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
