namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// The type of DataStorage operation
	/// </summary>
    public enum OperationType
    {
#pragma warning disable CS1591
		Add,
        Mul,
        Max,
        Min,
        Replace,
        Default,
        Mod,
        Pow,
        Xor,
        Or,
        And,
        LeftShift,
        RightShift,
		Remove,
		Pop,
		Update,
		Floor,
		Ceil
#pragma warning restore CS1591
    }
}