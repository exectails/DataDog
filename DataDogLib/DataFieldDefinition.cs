using System;

namespace DataDogLib
{
	/// <summary>
	/// Represents a field definition for a type.
	/// </summary>
	[Serializable]
	public class DataFieldDefinition
	{
		/// <summary>
		/// Returns the field's name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the offset in the type's struct.
		/// </summary>
		public int Offset { get; private set; }

		/// <summary>
		/// Returns the read type, specifying how the data is stored.
		/// </summary>
		public DataFieldReadType ReadType { get; private set; }

		/// <summary>
		/// Returns the byte size of this field.
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		/// Returns the field's variable type.
		/// </summary>
		public DataVarType VarType { get; private set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="offset"></param>
		/// <param name="readType"></param>
		/// <param name="size"></param>
		/// <param name="varType"></param>
		public DataFieldDefinition(string name, int offset, DataFieldReadType readType, int size, DataVarType varType)
		{
			this.Name = name;
			this.Offset = offset;
			this.ReadType = readType;
			this.Size = size;
			this.VarType = varType;

			if (
				(readType == DataFieldReadType.Bin && (varType == DataVarType.String || varType == DataVarType.Reference)) ||
				(readType == DataFieldReadType.String && (varType != DataVarType.String && varType != DataVarType.Reference))
			)
				throw new ArgumentException($"Type mismatch, {readType} and {varType} not compatible.");
		}

		/// <summary>
		/// Returns string representation of this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("{0}@{1}#{2},{3}", this.Name, this.Offset, this.Size, this.VarType);
		}
	}
}
