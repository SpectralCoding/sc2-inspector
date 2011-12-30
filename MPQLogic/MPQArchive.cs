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

		public MPQArchive(string Filename) {
			m_BaseStream = File.Open(Filename, FileMode.Open, FileAccess.Read);
			Parse();
		}

		public void Parse() {
			m_MPQHeader = GetMPQHeader();
			BinaryReader BinaryReader = new BinaryReader(m_BaseStream);
			m_MPQHashTable = GetMPQHashTable(BinaryReader);
			m_MPQBlockTable = GetMPQBlockTable(BinaryReader);
			GetFile("(listfile)", BinaryReader);
		}

		public void GetFile(string Filename, BinaryReader BinaryReader) {
			BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
			MPQHash ListFileHash = m_MPQHashTable.GetHashByFilename(Filename);
			MPQBlock ListFileEntry = (MPQBlock)m_MPQBlockTable[ListFileHash.BlockIndex];
		}

		public MPQHeader GetMPQHeader() {
			BinaryReader BinaryReader = new BinaryReader(m_BaseStream);
			m_BaseStream.Seek(0, SeekOrigin.Begin);
			MPQHeader returnMPQH = new MPQHeader(BinaryReader);
			return returnMPQH;
		}

		public MPQHashTable GetMPQHashTable(BinaryReader BinaryReader) {
			m_BaseStream.Seek(m_MPQHeader.HashTablePos, SeekOrigin.Begin);
			byte[] HashTableRawData = BinaryReader.ReadBytes(Convert.ToInt32(m_MPQHeader.HashTableSize) * 16);
			return new MPQHashTable(HashTableRawData, Convert.ToInt32(m_MPQHeader.HashTableSize));
		}

		public MPQBlockTable GetMPQBlockTable(BinaryReader BinaryReader) {
			m_BaseStream.Seek(m_MPQHeader.BlockTablePos, SeekOrigin.Begin);
			byte[] BlockTableRawData = BinaryReader.ReadBytes(Convert.ToInt32(m_MPQHeader.BlockTableSize) * 16);
			return new MPQBlockTable(BlockTableRawData, Convert.ToInt32(m_MPQHeader.BlockTableSize), m_MPQHeader.HeaderOffset);
		}

	}
}
