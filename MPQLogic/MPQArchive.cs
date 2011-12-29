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
		private List<MPQEntry> m_MPQBlockTable;

		public MPQArchive(string Filename) {
			m_BaseStream = File.Open(Filename, FileMode.Open, FileAccess.Read);
			Parse();
		}

		public void Parse() {
			m_MPQHeader = GetMPQHeader();
			BinaryReader BinaryReader = new BinaryReader(m_BaseStream);
			m_MPQHashTable = GetMPQHashTable(BinaryReader);
			m_MPQBlockTable = GetMPQBlockTable(BinaryReader);
			GetListFile(BinaryReader);
		}

		public void GetListFile(BinaryReader BinaryReader) {
			BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
			MPQHash ListFileHash = m_MPQHashTable.GetHashByFilename("(listfile)");
			// Next step is to figure out how to get the data out of the file. The listfile is compressed (See MPQExtractor).
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

		public List<MPQEntry> GetMPQBlockTable(BinaryReader BinaryReader) {
			List<MPQEntry> returnLst = new List<MPQEntry>();
			m_BaseStream.Seek(m_MPQHeader.BlockTablePos, SeekOrigin.Begin);
			byte[] BlockTableRawData = BinaryReader.ReadBytes(Convert.ToInt32(m_MPQHeader.BlockTableSize) * 16);
			BinaryReader DecryptedBinaryReader = new BinaryReader(new MemoryStream(BlockTableRawData));
			MPQEntry MPQEntryObj;
			for (int i = 0; i < m_MPQHeader.BlockTableSize; i++) {
				MPQEntryObj = new MPQEntry(DecryptedBinaryReader, m_MPQHeader.HeaderOffset);
				returnLst.Add(MPQEntryObj);
			}
			return returnLst;
		}

	}
}
