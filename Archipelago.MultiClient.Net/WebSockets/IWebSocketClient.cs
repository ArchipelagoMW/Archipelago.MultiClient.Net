using System;

#if !NET35
using System.Threading.Tasks;
#endif

namespace Archipelago.MultiClient.Net.Helpers
{
	/// <summary>
	/// Interface of basic low level websocket, allow custom implementation of websockets to be created uses 3rd party libaries
	/// </summary>
	public interface IClientWebSocket
	{
		/// <summary>
		/// Event handler to be called when a json message is received over from the host
		/// Should only be called for received json messages, not websocket control messages such a ping
		/// The argument should contain the json message
		/// </summary>
		event Action<string> OnMessageReceived;
		/// <summary>
		/// Event handler to be called when an error happens on the websocket connection
		/// The argument should contain the error in the form of an exception
		/// </summary>
		event Action<Exception> OnErrorReceived;
		/// <summary>
		/// Event handler to be called when a connection is closed
		/// The argument should contain the reason
		/// </summary>
		event Action<string> OnSocketClosed;
		/// <summary>
		/// Event handler to be called when a connection is opened
		/// </summary>
		event Action OnSocketOpened;

		/// <summary>
		/// Returns true if the socket believes it is connected to the host.
		/// Does not emit a ping to determine if the connection is stable.
		/// </summary>
		bool Connected { get; }

#if NET35
		/// <summary>
		/// blocking method to connect to the host, the uri to connect to was provided in the constructor
		/// </summary>
		/// <exception cref="Exception">An exception is thrown if connecting fails</exception>
		void Connect();
		/// <summary>
		/// blocking method to disconnect from the host
		/// </summary>
		void Disconnect();

		/// <summary>
		/// Sends json message to the host
		/// </summary>
		/// <param name="message">json string to send to the host</param>
		/// <param name="onSendComplete">an callback method to be called when the message was send, the underlying protocol should then make sure it reaches its destination in order</param>
		void SendAsync(string message, Action<bool> onSendComplete);
#else
		/// <summary>
		/// Non blocking method to connect to the host, the uri to connect to was provided in the constructor
		/// </summary>
		/// <returns>a Task that should complete when either the connection is established and Connected returns true or connecting failed</returns>
		Task ConnectAsync();
		/// <summary>
		/// Non blocking method to disconnect from the host
		/// </summary>
		/// <returns>a Task that should complete when the connection closed</returns>
		Task DisconnectAsync();

		/// <summary>
		/// Sends json message to the host
		/// </summary>
		/// <param name="message">json string to send to the host</param>
		/// <returns>a Task that should complete when the message was send, the underlying protocol should then make sure it reaches its destination in order</returns>
		Task SendAsync(string message);
#endif
	}
}
