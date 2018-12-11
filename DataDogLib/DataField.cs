using System;
using System.Globalization;

namespace DataDogLib
{
	public class DataField
	{
		public string Name { get; private set; }
		public DataVarType VarType { get; private set; }
		public object Value { get; set; }

		public DataField(string name, DataVarType type)
		{
			this.Name = name;
			this.VarType = type;
			this.Value = this.GetDefaultValue();
		}

		public object GetDefaultValue()
		{
			switch (this.VarType)
			{
				case DataVarType.Bool: return false;
				case DataVarType.Byte: return (byte)0;
				case DataVarType.Color: return (uint)0;
				case DataVarType.Float: return 0.0f;
				case DataVarType.Integer: return 0;
				case DataVarType.Reference:
				case DataVarType.String: return "";
			}

			throw new Exception($"Unknown var type '{this.VarType}'.");
		}

		public bool ValidateStringValue(string value)
		{
			switch (this.VarType)
			{
				case DataVarType.Bool:
					value = value.ToLowerInvariant();
					return (value == "true" || value == "false");

				case DataVarType.Byte:
					return byte.TryParse(value, out var _);

				case DataVarType.Color:
					return uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var _);

				case DataVarType.Float:
					return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);

				case DataVarType.Integer:
					return int.TryParse(value, out var _);

				case DataVarType.Reference:
				case DataVarType.String:
					return true;
			}

			return false;
		}
	}
}
