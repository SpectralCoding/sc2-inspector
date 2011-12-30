using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQArchive {
		private Stream m_BaseStream;
		private MPQHeader m_MPQHeader;
		private MPQHashTable m_MPQHashTable;
		private MPQBlockTable m_MPQBlockTable;

		/// <summary>
		/// Initializes the MPQArchive, retriving of all the meta information about the file.
		/// </summary>
		/// <param name="Filename">Filename of the MPQ file to be opened</param>
		public MPQArchive(string Filename) {
			m_BaseStream = File.Open(Filename, FileMode.Open, FileAccess.Read);
			m_MPQHeader = GetMPQHeader();
			BinaryReader BinaryReader = new BinaryReader(m_BaseStream);
			m_MPQHashTable = GetMPQHashTable(BinaryReader);
			m_MPQBlockTable = GetMPQBlockTable(BinaryReader);
		}

		/// <summary>
		/// Populates an MPQBlock with the decrypted data for the file.
		/// </summary>
		/// <param name="Filename">Internal MPQ filename (such as "(listfile)" or "replay.game.events").</param>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		public void GetFile(string Filename, BinaryReader BinaryReader) {
			BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
			MPQHash FileHash = m_MPQHashTable.GetHashByFilename(Filename);
			MPQBlock FileBlock = (MPQBlock)m_MPQBlockTable[FileHash.BlockIndex];
			FileBlock.PopulateFileContents(BinaryReader, m_MPQHeader.BlockSize);
		}

		/// <summary>
		/// Populates the MPQ Header data.
		/// </summary>
		/// <returns>An MPQHeader object containing the MPQ Header data.</returns>
		public MPQHeader GetMPQHeader() {
			BinaryReader BinaryReader = new BinaryReader(m_BaseStream);
			m_BaseStream.Seek(0, SeekOrigin.Begin);
			MPQHeader returnMPQH = new MPQHeader(BinaryReader);
			return returnMPQH;
		}

		/// <summary>
		/// Populates the MPQ Hash Table
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		/// <returns>A MPQHashTable object containing the Hashtable information from the MPQ.</returns>
		public MPQHashTable GetMPQHashTable(BinaryReader BinaryReader) {
			m_BaseStream.Seek(m_MPQHeader.HashTablePos, SeekOrigin.Begin);
			byte[] HashTableRawData = BinaryReader.ReadBytes(Convert.ToInt32(m_MPQHeader.HashTableSize) * 16);
			return new MPQHashTable(HashTableRawData, Convert.ToInt32(m_MPQHeader.HashTableSize));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BinaryReader">BinaryReader used to manipulate the data stream.</param>
		/// <returns>A MPQBlockTable object containing the Block Table information from the MPQ.</returns>
		public MPQBlockTable GetMPQBlockTable(BinaryReader BinaryReader) {
			m_BaseStream.Seek(m_MPQHeader.BlockTablePos, SeekOrigin.Begin);
			byte[] BlockTableRawData = BinaryReader.ReadBytes(Convert.ToInt32(m_MPQHeader.BlockTableSize) * 16);
			return new MPQBlockTable(BlockTableRawData, Convert.ToInt32(m_MPQHeader.BlockTableSize), m_MPQHeader.HeaderOffset);
		}

	}
}
