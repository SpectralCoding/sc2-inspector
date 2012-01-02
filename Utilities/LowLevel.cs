using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SC2Inspector.Utilities {

	public static class LowLevel {

		// This was edited to use long instead of int. You can view the int version on r5.
		[DebuggerStepThrough]
		public static long ParseVLFNumber(BinaryReader BinaryReader) {
			long Number = 0;
			var First = true;
			long Multiplier = 1;
			long Bytes = 0;
			while (true) {
				var i = BinaryReader.ReadByte();
				Number += (i & 0x7F) * (long)Math.Pow(2, Bytes * 7);
				if (First) {
					if ((Number & 1) != 0) {
						Multiplier = -1;
						Number--;
					}
					First = false;
				}
				if ((i & 0x80) == 0) {
					break;
				}
				Bytes++;
			}
			return (Number / 2) * Multiplier;
		}

		public static SerializedData ParseSerializedData(BinaryReader BinaryReader) {
			SerializedData returnSD = new SerializedData();
			byte DataTypeByte = BinaryReader.ReadByte();
			SerialDataType TempSDT = (SerialDataType)DataTypeByte;
			returnSD.DataType = TempSDT;
			int NumberOfElements;
			int Index;
			Dictionary<int, SerializedData> InnerSDArray = new Dictionary<int, SerializedData>(); ;
			switch (TempSDT) {
				case SerialDataType.BinaryData:
					int DataLen = Convert.ToInt32(ParseVLFNumber(BinaryReader));
					returnSD.ByteArrData = BinaryReader.ReadBytes(DataLen);
					break;
				case SerialDataType.SimpleArray:
					BinaryReader.ReadBytes(2);
					NumberOfElements = Convert.ToInt32(ParseVLFNumber(BinaryReader));
					Index = 0;
					while (NumberOfElements > 0) {
						InnerSDArray.Add(Index, ParseSerializedData(BinaryReader));
						Index++;
						NumberOfElements--;
					}
					returnSD.SerialData = InnerSDArray;
					break;
				case SerialDataType.ArrayWithKeys:
					NumberOfElements = Convert.ToInt32(ParseVLFNumber(BinaryReader));
					while (NumberOfElements > 0) {
						Index = Convert.ToInt32(ParseVLFNumber(BinaryReader));
						InnerSDArray.Add(Index, ParseSerializedData(BinaryReader));
						NumberOfElements--;
					}
					returnSD.SerialData = InnerSDArray;
					break;
				case SerialDataType.NumberOfOneByte:
					returnSD.ByteData = BinaryReader.ReadByte();
					break;
				case SerialDataType.NumberOfFourBytes:
					returnSD.UIntData = BinaryReader.ReadUInt32();
					break;
				case SerialDataType.NumberInVLF:
					returnSD.LongData = ParseVLFNumber(BinaryReader);
					break;
				default:
					throw new Exception("Unknown Datatype.");
			}
			return returnSD;
		}

	}

	public struct SerializedData {
		public SerialDataType DataType;
		public byte[] ByteArrData;
		public byte ByteData;
		public uint UIntData;
		public long LongData;
		public Dictionary<int, SerializedData> SerialData;
	}

	public enum SerialDataType {
		BinaryData = 0x02,
		SimpleArray = 0x04,
		ArrayWithKeys = 0x05,
		NumberOfOneByte = 0x06,
		NumberOfFourBytes = 0x07,
		NumberInVLF = 0x09
	}

}
