using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Jering
{
	[HtmlTargetElement("nodejs")]
	public class NodejsTagHelper : TagHelper
	{
		private readonly INodeJSService _NodeJSService;
		private readonly NodejsOptions _NodejsOptions;
		private readonly MemoryStream _Buffer = new();

#pragma warning disable IDE0051 // Remove unused private members
		private HttpRequest Request => ViewContext.HttpContext.Request;
#pragma warning restore IDE0051 // Remove unused private members

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName("reset-content")]
		public bool ResetContent { get; set; } = true;
		
		[HtmlAttributeName("route-path")]
		public string? RoutePath { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public NodejsTagHelper(INodeJSService nodeJSService, IOptionsMonitor<NodejsOptions> options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			_NodeJSService = nodeJSService ?? throw new ArgumentNullException(nameof(nodeJSService));
			_NodejsOptions = options != null ? options.CurrentValue : new NodejsOptions();
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			Debug.Assert(context != null && RoutePath != null);
			Debug.Assert(output != null);

			output.TagName = null;
			output.TagMode = TagMode.StartTagAndEndTag;

			//NOTE: this clears the slot
			if (ResetContent)
				output.Content.Clear();

			// TODO: HttpMethod, Body, querystring should be optional and
			// can be overriden
			ValueTask<INodejsRequest> nodeReq = ValueTask.FromResult(
				NodejsExtensions.SetupRequest(
					httpMethod: "GET",
					headers: null,
					queryString: null,
					hostname: ViewContext.HttpContext.Request.Host.Value,
					routePath: RoutePath,
					requestBody: NodejsExtensions.EmptyBodyResult.Buffer,
					scheme: ViewContext.HttpContext.Request.Scheme,
					false));

			NodejsResponse? nodejsOutput = await _NodeJSService
				.InvokeNodejsRequestUsingValueTask(
					options: _NodejsOptions,
					nodejsRequest: nodeReq,
					shouldGzipCompress: false,
					overrideBodyOnlyReply: true)
				.ConfigureAwait(false);

			if (nodejsOutput != null && nodejsOutput.BodyStream != null)
			{
				await nodejsOutput.BodyStream.CopyToAsync(_Buffer).ConfigureAwait(false);
				output.Content.SetHtmlContent(new StreamHtmlContent(_Buffer));
			}
		}
	}
}
