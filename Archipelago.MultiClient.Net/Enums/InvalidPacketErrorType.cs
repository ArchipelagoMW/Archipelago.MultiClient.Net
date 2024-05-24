
namespace Archipelago.MultiClient.Net.Enums
{
	/// <summary>
	/// Types of errors that can be returned by the server if your packet gets rejected
	/// </summary>
    public enum InvalidPacketErrorType
    {
		/// <summary>
		/// The command in your packet was invalid
		/// </summary>
        Cmd,
		/// <summary>
		/// Some of the arguments in your packet were invalid
		/// </summary>
        Arguments
    }
}
