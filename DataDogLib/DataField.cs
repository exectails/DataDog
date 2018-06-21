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
		}
	}
}
