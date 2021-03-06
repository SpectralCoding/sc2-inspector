﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using SC2Inspector.Utilities;

namespace SC2Inspector.MPQLogic {
	public class MPQHashTable : Hashtable {

		public new MPQHash this[object key] {
			get { return (MPQHash)base[key]; }
			set { base[key] = value; }
		}

		/// <summary>
		/// Initializes and populates the MPQ Hash Table
		/// </summary>
		/// <param name="HashTableData">Byte array containing the raw hash table data.</param>
		/// <param name="HashTableSize">Number of hashes in the array. Byte size should actually be this value multiplied by 16.</param>
		public MPQHashTable(byte[] HashTableData, int HashTableSize) {
			MPQ.DecryptTable(HashTableData, "(hash table)");
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
		/// <returns>A MPQHash object containing information about the file</returns>
		public MPQHash GetHashByFilename(string Filename) {
			uint IndexHash = MPQ.HashString(Filename, 0);
			uint Name1 = MPQ.HashString(Filename, 0x100);
			uint Name2 = MPQ.HashString(Filename, 0x200);
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
