using System;

namespace DataDogLib
{
	public class DataFieldDefinition
	{
		public string Name { get; private set; }
		public int Offset { get; private set; }
		public DataFieldReadType ReadType { get; private set; }
		public int Size { get; private set; }
		public DataVarType VarType { get; private set; }

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

		public override string ToString()
		{
			return string.Format("{0}@{1}#{2},{3}", this.Name, this.Offset, this.Size, this.VarType);
		}
	}
}
