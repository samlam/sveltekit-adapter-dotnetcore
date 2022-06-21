using System.Diagnostics;

using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Jering
{
    /// <summary>
    /// Nodejs middleware with Jering lib.
    /// </summary>
    public class NodejsMiddleware
	{
		private readonly bool _ShouldGzipCompress = true;
		private readonly RequestDelegate _Next;
		private readonly ILogger<NodejsMiddleware> _Logger;
		private readonly INodeJSService _NodeJSService;
		private readonly IOptionsMonitor<NodejsOptions> _NodejsOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="NodejsMiddleware"/> class.
		/// </summary>
		/// <param name="next">Next http request task.</param>
		/// <param name="logger">Logger for middleware.</param>
		/// <param name="nodeJSService"><see cref="INodeJSService"/> and should not be disposed.</param>
		/// <param name="options"><see cref="NodejsOptions"/>.</param>
		public NodejsMiddleware(
			RequestDelegate next, 
			ILogger<NodejsMiddleware> logger, 
			INodeJSService nodeJSService, 
			IOptionsMonitor<NodejsOptions> options)
		{
			_Next = next ?? throw new ArgumentNullException(nameof(next));
			_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_NodeJSService = nodeJSService
				?? throw new ArgumentNullException(nameof(nodeJSService));
			_NodejsOptions = options ?? throw new ArgumentNullException(nameof(options));
		}

		/// <summary>
		/// Invoke the middleware pipeline handler.
		/// </summary>
		/// <param name="context">HttpContext.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task InvokeAsync(HttpContext context)
		{
			if (context == null || context.Request == null || context.Request.Path == null)
				return;

			if (Debugger.IsAttached)
				_Logger.LogInformation($"{nameof(NodejsMiddleware)} is invoked for {context.Request.Path}");

			NodejsResponse? result = null;
			try 
			{
				result = await _NodeJSService
					.InvokeNodejsRequestUsingHttpContext(_NodejsOptions.CurrentValue, context, _ShouldGzipCompress, false, null, context.RequestAborted)
					.ConfigureAwait(false);

				if (result == null || result.Status == 404)
				{
					await _Next(context).ConfigureAwait(false);
					return;
				}
			}
			catch(Exception nodeEx)
			{
				_Logger.LogError(nodeEx, $"{nameof(NodejsMiddleware)} failed for {context.Request.Path}");
				await _Next(context).ConfigureAwait(false);
				return;
			}

			HttpResponse httpResp = context.Response;
			httpResp.StatusCode = result.Status;

			if (result.Headers != null && result.Headers.Count > 0)
			{
				foreach (KeyValuePair<string, string> keyValuePair in result.Headers)
				{
					httpResp.Headers.Append(keyValuePair.Key, new StringValues(keyValuePair.Value));
				}
			}

			if (string.IsNullOrWhiteSpace(result.Body))
			{
				await _Next(context).ConfigureAwait(false);
				return;
			}

			if (_NodejsOptions.CurrentValue.GzipCompressResponse && result.BodyStream != null)
			{
				using Stream body = await result.BodyStream.CompressContentAsync().ConfigureAwait(false);

				IHeaderDictionary headers = context.Response.Headers;
				headers.Add("Content-Encoding", new StringValues("gzip"));
				headers.Add("Content-Length", new StringValues(body.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)));
				await body.CopyToAsync(context.Response.Body, context.RequestAborted).ConfigureAwait(false);
				return;
			}
			else
			{
				await httpResp.WriteAsync(result.Body, context.RequestAborted).ConfigureAwait(false);
			}
		}
	}
}