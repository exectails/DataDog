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
	public class DataDogFile
	{
		private static readonly Regex TypesRegex = new Regex(@"(?<name>[a-z0-9_]+)%(?<size>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex FieldsRegex = new Regex(@"(?<typeName>[a-z0-9_]+)\.(?<fieldName>[a-z0-9_]+)%(?<offset>[0-9]+)(?<type>[\*\|#])(?<size>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ListRegex = new Regex(@"(?<name>[a-z0-9_]+)\[(?<count>[0-9]+)\]@(?<typeName>[a-z_]+)%(?<offset>[0-9]+)\|", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public Dictionary<string, DataTypeDefinition> Types { get; } = new Dictionary<string, DataTypeDefinition>();
		public Dictionary<string, DataObjectList> Lists { get; } = new Dictionary<string, DataObjectList>();

		public static DataDogFile Read(string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				return Read(fs);
		}

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

				var dataDogInfo = Encoding.UTF8.GetString(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position))).TrimEnd('\0');
				var fieldVarTypes = FindVarTypes(dataDogInfo);

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

		private static DataFieldReadType GetReadType(string typeStr)
		{
			switch (typeStr)
			{
				case "*": return DataFieldReadType.String;
				case "#": return DataFieldReadType.Bin;

				default: throw new ArgumentException($"Unknown column type '{typeStr}'.");
			}
		}

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
