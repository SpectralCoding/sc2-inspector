using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SC2Inspector.MPQLogic {
	class MPQArchive {
		private Stream m_BaseStream;
		private BinaryReader m_BinaryReader;
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
			m_BinaryReader = new BinaryReader(m_BaseStream);
			m_MPQHashTable = GetMPQHashTable(m_BinaryReader);
			m_MPQBlockTable = GetMPQBlockTable(m_BinaryReader);
			ExtractFiles();
		}

		public void ExtractFiles() {
			#region Extract (listfile) and (attributes)
			MPQBlock TempBlock = GetFile("(listfile)");
			MPQHash TempHash = m_MPQHashTable.GetHashByFilename("(listfile)");
			m_MPQBlockTable.Remove(TempHash.BlockIndex);
			m_MPQBlockTable.Add("(listfile)", TempBlock);
			TempBlock = GetFile("(attributes)");
			TempHash = m_MPQHashTable.GetHashByFilename("(attributes)");
			m_MPQBlockTable.Remove(TempHash.BlockIndex);
			m_MPQBlockTable.Add("(attributes)", TempBlock);
			#endregion
			TempBlock = m_MPQBlockTable["(listfile)"];
			string[] ListFileSplit = System.Text.UTF8Encoding.UTF8.GetString(TempBlock.RawContents).Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string CurrentListFileEntry in ListFileSplit) {
				TempBlock = GetFile(CurrentListFileEntry);
				TempHash = m_MPQHashTable.GetHashByFilename(CurrentListFileEntry);
				m_MPQBlockTable.Remove(TempHash.BlockIndex);
				m_MPQBlockTable.Add(CurrentListFileEntry, TempBlock);
			}
		}

		/// <summary>
		/// Populates an MPQBlock with the decrypted data for the file.
		/// </summary>
		/// <param name="Filename">Internal MPQ filename (such as "(listfile)" or "replay.game.events").</param>
		/// 
		public MPQBlock GetFile(string Filename) {
			if (m_MPQBlockTable.ContainsKey(Filename)) {
				return m_MPQBlockTable[Filename];
			} else {
				m_BinaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
				MPQHash FileHash = m_MPQHashTable.GetHashByFilename(Filename);
				MPQBlock FileBlock = m_MPQBlockTable[FileHash.BlockIndex];
				if (FileBlock.RawContents == null) {
					FileBlock.PopulateFileContents(m_BinaryReader, m_MPQHeader.BlockSize);
				}
				return FileBlock;
			}
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
