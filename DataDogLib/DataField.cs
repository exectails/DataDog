using System;
using System.Globalization;

namespace DataDogLib
{
	/// <summary>
	/// Represents a field in and object.
	/// </summary>
	[Serializable]
	public class DataField
	{
		/// <summary>
		/// Returns the field's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the field's variable type.
		/// </summary>
		public DataVarType VarType { get; private set; }

		/// <summary>
		/// Returns the field's boxed value.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Creates new instance with Value being set to the type's default.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		public DataField(string name, DataVarType type)
		{
			this.Name = name;
			this.VarType = type;
			this.Value = this.GetDefaultValue();
		}

		/// <summary>
		/// Returns the default value for this instance's variable type.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Returns true if the given string is a valid value for this field.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
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
