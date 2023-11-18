using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace MartenApp.Etag;

public sealed class ETagMiddleware
{
	private readonly RequestDelegate next;

	public ETagMiddleware(RequestDelegate next)
	{
		this.next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		if (context.Request.Method == HttpMethod.Get.Method)
		{
			using var buffer = new MemoryStream();
			var stream = context.Response.Body;
			context.Response.Body = buffer;

			await next.Invoke(context);

			buffer.Seek(0, SeekOrigin.Begin);
			using var bufferReader = new StreamReader(buffer);
			var body = await bufferReader.ReadToEndAsync();

			if (context.Response.StatusCode == StatusCodes.Status200OK)
			{
				var key = GetKey(context.Request);
				var combinedKey = key + body;
				var combinedBytes = Encoding.UTF8.GetBytes(combinedKey);

				var ETAG = GenerateETag(combinedBytes);

				context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out StringValues requestEtag);

				if (requestEtag.ToString() == ETAG)
					context.Response.StatusCode = StatusCodes.Status304NotModified;
				else
					context.Response.Headers.ETag = ETAG;
			}

			buffer.Seek(0, SeekOrigin.Begin);
			await buffer.CopyToAsync(stream);
			context.Response.Body = stream;
		}
		else
		{
			await next.Invoke(context);
		}
	}

	private static string GenerateETag(byte[] data)
	{
		var hash = MD5.HashData(data);
		var hex = BitConverter.ToString(hash);
		var etag = hex.Replace("-", "");

		return etag;
	}

	private static string GetKey(HttpRequest request)
	{
		return UriHelper.GetDisplayUrl(request);
	}
}