using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DataDogLib
{
	/// <summary>
	/// Represents a DataDog file from Mabinogi (extension .data).
	/// </summary>
	public class DataDogFile
	{
		private static readonly Regex TypesRegex = new Regex(@"(?<name>[a-z0-9_]+)%(?<size>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex FieldsRegex = new Regex(@"(?<typeName>[a-z0-9_]+)\.(?<fieldName>[a-z0-9_]+)%(?<offset>[0-9]+)(?<type>[\*\|#])(?<size>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ListRegex = new Regex(@"(?<name>[a-z0-9_]+)\[(?<count>[0-9]+)\]@(?<typeName>[a-z_]+)%(?<offset>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public Dictionary<string, DataTypeDefinition> Types { get; } = new Dictionary<string, DataTypeDefinition>();
		public Dictionary<string, DataObjectList> Lists { get; } = new Dictionary<string, DataObjectList>();
		public string DataDogInfo { get; private set; }

		/// <summary>
		/// Reads file from given path and returns it.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static DataDogFile Read(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				return Read(fs);
		}

		/// <summary>
		/// Reads file from given stream and returns it.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static DataDogFile Read(Stream stream)
		{
			var result = new DataDogFile();

			if (stream.Length < 40)
				throw new ArgumentException("Not a data dog file.");

			using (var br = new BinaryReader(stream))
			{
				var signature = Encoding.UTF8.GetString(br.ReadBytes(16));
				var version = signature.Substring(9).Replace("  ", " ").Replace(" ", ".");

				if (!signature.StartsWith("DDBINFILE"))
					throw new ArgumentException("Incorrect header.");

				var stringsOffset = br.ReadInt32();
				var stringsLength = br.ReadInt32();
				var dataSize = br.ReadInt32();
				var typesLength = br.ReadInt32();
				var fieldsLength = br.ReadInt32();
				var listsLength = br.ReadInt32();

				var typeDefinitions = Encoding.UTF8.GetString(br.ReadBytes(typesLength)).TrimEnd('\0');
				var fieldDefinitions = Encoding.UTF8.GetString(br.ReadBytes(fieldsLength)).TrimEnd('\0');
				var listDefinitions = Encoding.UTF8.GetString(br.ReadBytes(listsLength)).TrimEnd('\0');
				var stringBlock = br.ReadBytes(stringsLength);
				var data = br.ReadBytes(dataSize);

				var dataDogInfo = Encoding.GetEncoding("EUC-KR").GetString(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position))).TrimEnd('\0');
				var fieldVarTypes = FindVarTypes(dataDogInfo);

				result.DataDogInfo = dataDogInfo;

				var typeMatches = TypesRegex.Matches(typeDefinitions);

				foreach (Match match in typeMatches)
				{
					var name = match.Groups["name"].Value;
					var size = Convert.ToInt32(match.Groups["size"].Value);

					var strct = new DataTypeDefinition(name, size);
					result.Types[name] = strct;
				}

				var fieldMatches = FieldsRegex.Matches(fieldDefinitions);
				if (fieldMatches.Count != fieldDefinitions.Count(a => a == '|'))
					throw new FormatException("Invalid number of fields.");

				foreach (Match match in fieldMatches)
				{
					var typeName = match.Groups["typeName"].Value;
					if (!result.Types.TryGetValue(typeName, out var type))
						throw new TypeAccessException($"Type '{typeName}' not found for field.");

					var fieldName = match.Groups["fieldName"].Value;
					var offset = Convert.ToInt32(match.Groups["offset"].Value);
					var size = Convert.ToInt32(match.Groups["size"].Value);

					var typeStr = match.Groups["type"].Value;
					var fieldReadType = GetReadType(typeStr);

					var fullFieldName = (typeName + "." + fieldName).ToLowerInvariant();
					if (!fieldVarTypes.TryGetValue(fullFieldName, out var varType))
						throw new TypeAccessException($"Type not found for '{fullFieldName}'.");

					var field = new DataFieldDefinition(fieldName, offset, fieldReadType, size, varType);
					type.Fields[fieldName] = field;
				}

				using (var dataStream = new MemoryStream(data))
				using (var br2 = new BinaryReader(dataStream))
				{
					var listMatches = ListRegex.Matches(listDefinitions);
					var read = 0;

					for (var i = 0; i < listMatches.Count; ++i)
					{
						var listMatch = listMatches[i];

						var listName = listMatch.Groups["name"].Value;
						var listTypeName = listMatch.Groups["typeName"].Value;
						var listCount = Convert.ToInt32(listMatch.Groups["count"].Value);

						if (!result.Types.TryGetValue(listTypeName, out var type))
							throw new TypeAccessException($"Type '{listTypeName}' not found for data.");

						var list = new DataObjectList(listName, type);

						for (var j = 0; j < listCount; ++j)
						{
							var match = listMatches[++i];

							var objName = match.Groups["name"].Value;
							var typeName = match.Groups["typeName"].Value;
							var count = Convert.ToInt32(match.Groups["count"].Value);
							var offset = Convert.ToInt32(match.Groups["offset"].Value);

							dataStream.Seek(offset, SeekOrigin.Begin);
							var bytes = br2.ReadBytes(type.Size);

							var obj = new DataObject(objName, type);

							read += bytes.Length;

							foreach (var field in type.Fields.Values.OrderBy(a => a.Offset))
							{
								var objField = new DataField(field.Name, field.VarType);

								switch (field.VarType)
								{
									case DataVarType.Byte:
										objField.Value = bytes[field.Offset];
										break;

									case DataVarType.Bool:
										objField.Value = (bytes[field.Offset] != 0);
										break;

									case DataVarType.Integer:
										objField.Value = BitConverter.ToInt32(bytes, field.Offset);
										break;

									case DataVarType.Color:
										objField.Value = BitConverter.ToUInt32(bytes, field.Offset);
										break;

									case DataVarType.Float:
										objField.Value = BitConverter.ToSingle(bytes, field.Offset);
										break;

									case DataVarType.String:
									case DataVarType.Reference:
										var stringOffset = BitConverter.ToInt32(bytes, field.Offset);

										try
										{
											var nullIndex = Array.IndexOf(stringBlock, (byte)0, stringOffset);
											var str = Encoding.GetEncoding("EUC-KR").GetString(stringBlock, stringOffset, nullIndex - stringOffset);

											objField.Value = str;
										}
										catch (ArgumentOutOfRangeException)
										{
											Console.Write("Error: String not found.");
										}
										break;

									default:
										throw new InvalidDataException($"Unknown type '{field.VarType}'.");
								}

								obj.Fields[field.Name] = objField;
							}

							list.Objects.Add(obj);
						}

						result.Lists[list.Name] = list;
					}

					if (read != dataStream.Length)
						throw new FormatException("Data wasn't read completely.");
				}
			}

			return result;
		}

		/// <summary>
		/// Writes file to given stream.
		/// </summary>
		/// <param name="stream"></param>
		public void Write(Stream stream)
		{
			var typesSb = new StringBuilder();
			foreach (var type in this.Types.Values)
			{
				typesSb.AppendFormat("{0}%{1}|", type.Name, type.Size);
			}

			var fieldsSb = new StringBuilder();
			foreach (var type in this.Types.Values)
			{
				foreach (var field in type.Fields.Values)
				{
					var readTypeStr = GetReadTypeString(field.ReadType);
					fieldsSb.AppendFormat("{0}.{1}%{2}{3}{4}|", type.Name, field.Name, field.Offset, readTypeStr, field.Size);
				}
			}

			var listsSb = new StringBuilder();
			var listOffset = 0;
			foreach (var list in this.Lists.Values)
			{
				listsSb.AppendFormat("{0}[{1}]@{2}%{3}|", list.Name, list.Objects.Count, list.Type.Name, listOffset);

				foreach (var obj in list.Objects)
				{
					listsSb.AppendFormat("{0}[{1}]@{2}%{3}|", obj.Name, 0, obj.Type.Name, listOffset);
					listOffset += obj.Type.Size;
				}
			}

			var stringsSb = new StringBuilder();
			var stringOffsets = new Dictionary<string, int>();
			var index = 0;

			foreach (var list in this.Lists.Values)
			{
				foreach (var obj in list.Objects)
				{
					foreach (var field in obj.Fields.Values.Where(a => a.VarType == DataVarType.String || a.VarType == DataVarType.Reference))
					{
						var str = (string)field.Value;
						var len = Encoding.GetEncoding("EUC-KR").GetByteCount(str) + 1;

						stringOffsets[str] = index;
						stringsSb.AppendFormat("{0}\0", (string)field.Value);

						index += len;
					}
				}
			}

			var dataLst = new List<byte>();
			foreach (var list in this.Lists.Values)
			{
				foreach (var obj in list.Objects)
				{
					foreach (var field in obj.Fields.Values)
					{
						switch (field.VarType)
						{
							case DataVarType.Byte:
								dataLst.Add((byte)field.Value);
								break;

							case DataVarType.Bool:
								dataLst.Add((bool)field.Value ? (byte)1 : (byte)0);
								break;

							case DataVarType.Integer:
								dataLst.AddRange(BitConverter.GetBytes((int)field.Value));
								break;

							case DataVarType.Color:
								dataLst.AddRange(BitConverter.GetBytes((uint)field.Value));
								break;

							case DataVarType.Float:
								dataLst.AddRange(BitConverter.GetBytes((float)field.Value));
								break;

							case DataVarType.String:
							case DataVarType.Reference:
								// The reference for "commerceelephant/*/walk"
								// is different from devCAT's file because
								// that string exists twice and we simply use
								// one of them, but the result should be
								// the same.

								var str = (string)field.Value;
								var offset = stringOffsets[str];
								dataLst.AddRange(BitConverter.GetBytes(offset));
								break;
						}
					}
				}
			}

			var types = Encoding.GetEncoding("EUC-KR").GetBytes(typesSb.ToString() + '\0');
			var fields = Encoding.GetEncoding("EUC-KR").GetBytes(fieldsSb.ToString() + '\0');
			var lists = Encoding.GetEncoding("EUC-KR").GetBytes(listsSb.ToString() + '\0');
			var strings = Encoding.GetEncoding("EUC-KR").GetBytes(stringsSb.ToString());
			var data = dataLst.ToArray();

			using (var bw = new BinaryWriter(stream))
			{
				bw.WriteString("DDBINFILE2 0  48");

				bw.Write(types.Length + fields.Length + lists.Length + 6 * sizeof(int));
				bw.Write(strings.Length);
				bw.Write(data.Length);
				bw.Write(types.Length);
				bw.Write(fields.Length);
				bw.Write(lists.Length);

				bw.Write(types);
				bw.Write(fields);
				bw.Write(lists);
				bw.Write(strings);
				bw.Write(data);

				bw.WriteString(this.DataDogInfo);
			}
		}

		/// <summary>
		/// Writes entire file to given path in XML format.
		/// </summary>
		/// <param name="filePath"></param>
		public void ExportXml(string filePath)
		{
			var xmlSettings = new XmlWriterSettings();
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var sw = new StreamWriter(fs))
			using (var xmlWriter = XmlWriter.Create(fs, xmlSettings))
			{
				xmlWriter.WriteStartElement("DataDog");

				foreach (var list in this.Lists.Values)
					this.WriteList(xmlWriter, list);

				xmlWriter.WriteEndElement();
				sw.WriteLine();
			}
		}

		/// <summary>
		/// Writes only one of the lists from the file to the given path
		/// in XML format.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="listName"></param>
		public void ExportXml(string filePath, string listName)
		{
			if (!this.Lists.ContainsKey(listName))
				throw new ArgumentException("List not found.");

			var xmlSettings = new XmlWriterSettings();
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var sw = new StreamWriter(fs))
			using (var xmlWriter = XmlWriter.Create(fs, xmlSettings))
			{
				this.WriteList(xmlWriter, this.Lists[listName]);
				sw.WriteLine();
			}
		}

		/// <summary>
		/// Writes list to XML writer.
		/// </summary>
		/// <param name="xmlWriter"></param>
		/// <param name="list"></param>
		private void WriteList(XmlWriter xmlWriter, DataObjectList list)
		{
			xmlWriter.WriteStartElement(list.Name);

			foreach (var obj in list.Objects)
			{
				xmlWriter.WriteStartElement(obj.Type.Name);
				xmlWriter.WriteAttributeString("_ObjName", obj.Name);

				foreach (var field in obj.Fields.Values)
				{
					switch (field.VarType)
					{
						case DataVarType.Color: xmlWriter.WriteAttributeString(field.Name, ((uint)field.Value).ToString("X8")); break;
						case DataVarType.Float: xmlWriter.WriteAttributeString(field.Name, ((float)field.Value).ToString("0.0#", CultureInfo.InvariantCulture)); break;
						default: xmlWriter.WriteAttributeString(field.Name, field.Value.ToString()); break;
					}
				}

				xmlWriter.WriteEndElement();
			}

			xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// Returns read type based on given string from type definition.
		/// </summary>
		/// <param name="typeStr"></param>
		/// <returns></returns>
		private static DataFieldReadType GetReadType(string typeStr)
		{
			switch (typeStr)
			{
				case "*": return DataFieldReadType.String;
				case "#": return DataFieldReadType.Bin;

				default: throw new ArgumentException($"Unknown column type '{typeStr}'.");
			}
		}

		/// <summary>
		/// Returns string for given read type, to be used in type definition.
		/// </summary>
		/// <param name="readType"></param>
		/// <returns></returns>
		private static string GetReadTypeString(DataFieldReadType readType)
		{
			switch (readType)
			{
				case DataFieldReadType.String: return "*";
				case DataFieldReadType.Bin: return "#";

				default: throw new ArgumentException($"Unknown type '{readType}'.");
			}
		}

		/// <summary>
		/// Returns var type based on given string from field definition.
		/// </summary>
		/// <param name="dataDogInfo"></param>
		/// <param name="varTypeName"></param>
		/// <returns></returns>
		private static DataVarType GetVarType(string dataDogInfo, string varTypeName)
		{
			switch (varTypeName)
			{
				case "byt": return DataVarType.Byte;
				case "boo": return DataVarType.Bool;
				case "int": return DataVarType.Integer;
				case "col":
				case "rgb": return DataVarType.Color;
				case "flo": return DataVarType.Float;
				case "str": return DataVarType.String;
				case "ref": return DataVarType.Reference;

				default: throw new ArgumentException($"Unknown type '{varTypeName}'.");
			}
		}

		/// <summary>
		/// Searches given info block for variable types and returns them.
		/// </summary>
		/// <param name="dataDogInfo"></param>
		/// <returns></returns>
		private static Dictionary<string, DataVarType> FindVarTypes(string dataDogInfo)
		{
			var result = new Dictionary<string, DataVarType>();

			using (var sr = new StringReader(dataDogInfo))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (line.IndexOf('@') == 3 && line.IndexOf('@', 4) == -1 && line.IndexOf('.', 4) != -1 && line.IndexOf(':') == -1)
					{
						var typeName = line.Substring(0, 3).ToLowerInvariant();
						var fieldName = line.Substring(4).ToLowerInvariant();

						result[fieldName] = GetVarType(dataDogInfo, typeName);
					}
				}
			}

			return result;
		}
	}
}
