using System.Diagnostics;
using System.IO.Pipelines;

using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Http;

namespace Jering
{
    /// <summary>
    /// functions that invoke node.js requests.
    /// </summary>
    public static class NodejsInvokeExtensions
	{
#pragma warning disable IDE1006 // Naming Styles
		public static readonly ReadResult EmptyBodyResult = new(default, false, false);
#pragma warning restore IDE1006 // Naming Styles

		/// <summary>
		/// Invoke nodejs service with the given request.
		/// </summary>
		/// <param name="nodeJSService"><see cref="INodeJSService"/>.</param>
		/// <param name="options"><see cref="NodejsOptions"/>.</param>
		/// <param name="nodejsRequest"><see cref="INodejsRequest"/>.</param>
		/// <param name="overrides"><see cref="RequestOverrides"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
		/// <returns>html content.</returns>
		public static async Task<string> InvokeNodejsRequest(
			this INodeJSService nodeJSService,
			NodejsOptions options,
			ValueTask<INodejsRequest> nodejsRequest,
			RequestOverrides? overrides = null,
			CancellationToken cancellationToken = default)
		{
			Debug.Assert(nodeJSService != null);
			Debug.Assert(options != null);

			NodejsResponse? resp = await nodeJSService.InvokeNodejsRequestUsingValueTask(
				options,
				nodejsRequest,
				false,
				false,
				overrides,
				cancellationToken).ConfigureAwait(false);

			return resp != null && resp.Body != null ? resp.Body : string.Empty;
		}

		/// <summary>
		/// Invoke node.js requests with the given HttpContext.
		/// </summary>
		/// <param name="nodeJSService"><see cref="INodeJSService"/>.</param>
		/// <param name="options"><see cref="NodejsOptions"/>.</param>
		/// <param name="context"><see cref="HttpContext"/>.</param>
		/// <param name="shouldGzipCompress">should compress the response using gzip.</param>
		/// <param name="overrideBodyOnlyReply">instruct Jering Node to return only the response body; this is mainly for the component development.</param>
		/// <param name="overrides"><see cref="RequestOverrides"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
		/// <returns><see cref="NodejsResponse"/>.</returns>
		internal static async Task<NodejsResponse?> InvokeNodejsRequestUsingHttpContext(
			this INodeJSService nodeJSService,
			NodejsOptions options,
			HttpContext context,
			bool shouldGzipCompress,
			bool? overrideBodyOnlyReply = null,
			RequestOverrides? overrides = null,
			CancellationToken cancellationToken = default)
		{
			ValueTask<INodejsRequest> nodeReq = NodejsExtensions.SetupRequest(
					context,
					overrideBodyOnlyReply ?? options.BodyOnlyReply,
					overrides);

			return await nodeJSService.InvokeNodejsRequestUsingValueTask(
					options,
					nodeReq,
					shouldGzipCompress,
					overrideBodyOnlyReply,
					overrides,
					cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Invoke node.js requests with <see cref="INodejsRequest"/> interface.
		/// </summary>
		/// <param name="nodeJSService"><see cref="INodeJSService"/>.</param>
		/// <param name="options"><see cref="NodejsOptions"/>.</param>
		/// <param name="nodejsRequest"><see cref="INodejsRequest"/>.</param>
		/// <param name="shouldGzipCompress">should compress the response using gzip.</param>
		/// <param name="overrideBodyOnlyReply">instruct Jering Node to return only the response body; this is mainly for the component development.</param>
		/// <param name="overrides"><see cref="RequestOverrides"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
		/// <returns><see cref="NodejsResponse"/>.</returns>
		internal static async Task<NodejsResponse?> InvokeNodejsRequestUsingValueTask(
			this INodeJSService nodeJSService,
			NodejsOptions options,
			ValueTask<INodejsRequest> nodejsRequest,
			bool shouldGzipCompress,
			bool? overrideBodyOnlyReply = null,
			RequestOverrides? overrides = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(nameof(nodeJSService));
			ArgumentNullException.ThrowIfNull(nameof(options));

			// bool bodyOnlyReply = overrideBodyOnlyReply ?? options.BodyOnlyReply;
			object[] arguments = new object[]
			{
				await nodejsRequest.ConfigureAwait(false),
			};

			// TODO: BodyOnly option requires stream response from sveltekit,
			// which doesn't work atm.
			/* if (bodyOnlyReply == true)
			{
				Stream? streamResp = await nodeJSService.InvokeFromFileAsync<Stream>(
				   modulePath: options.ScriptPath,
				   args: arguments,
				   cancellationToken: cancellationToken).ConfigureAwait(false);

				return streamResp == null ? null : new NodejsBodyOnlyResponse(streamResp);
			}*/

			try
			{
				return await nodeJSService
					.InvokeFromFileAsync<NodejsResponse>(
						modulePath: options.ScriptPath,
						args: arguments,
						cancellationToken: cancellationToken)
					.ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}
	}
}
