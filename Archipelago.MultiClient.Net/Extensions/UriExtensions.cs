using System;

namespace Archipelago.MultiClient.Net.Extensions
{
	static class UriExtensions
	{
#if NET6_0
		static readonly string UriSchemeWss = Uri.UriSchemeWss;
		static readonly string UriSchemeWs = Uri.UriSchemeWs;
#else
		static readonly string UriSchemeWss = "wss";
	    static readonly string UriSchemeWs = "ws";
#endif

		public static Uri WithSchema(this Uri uri, string schema) => 
			new UriBuilder(uri) { Scheme = schema }.Uri;

		public static Uri AsWss(this Uri uri) => 
			WithSchema(uri, UriSchemeWss);

		public static Uri AsWs(this Uri uri) => 
			WithSchema(uri, UriSchemeWs);
	}
}
