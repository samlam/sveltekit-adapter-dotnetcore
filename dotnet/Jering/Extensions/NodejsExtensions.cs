using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Text;

using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Jering
{
    /// <summary>
    /// Custom middleware to use the capability of Jering lib.
    /// </summary>
    public static class NodejsExtensions
    {
        public static readonly ReadResult EmptyBodyResult = new ReadResult(default, false, false);
        /// <summary>
        /// AddJering method to registed in Startup.
        /// </summary>
        /// <param name="services">Services <see cref="IServiceCollection"/> collection.</param>
        /// <param name="config">Application <see cref="IConfiguration"/>.</param>
        /// <returns>Extension method return parameter.</returns>
        public static IServiceCollection ConfigureNodejsService(this IServiceCollection services)
        {
            services
                .AddNodeJS()
                .Configure<OutOfProcessNodeJSServiceOptions>(options =>
                {
#if DEBUG
                    options.Concurrency = Concurrency.None;
                    options.EnableFileWatching = true;
                    options.WatchPath = "./build/";
                    options.TimeoutMS = -1; // -1 to wait forever (used for attaching debugger, which needs to be set in code)
#else
                    options.Concurrency = Concurrency.MultiProcess;
                    options.ConcurrencyDegree = 2;
                    options.TimeoutMS = 1000;
#endif
                })
                .Configure<HttpNodeJSServiceOptions>(options => options.Version = HttpVersion.Version20)
                .Configure<NodeJSProcessOptions>(options =>
                {
#if DEBUG
                    options.NodeAndV8Options = "--inspect --es-module-specifier-resolution=node --experimental-vm-modules";
#else
					options.NodeAndV8Options = "--es-module-specifier-resolution=node --experimental-vm-modules ";
#endif
                    options.EnvironmentVariables = new Dictionary<string, string>
                    {
                        { "VITE_ForgePort", "5004"}, // this value needs to match the port # in launchSettings.json
                        { "NODE_ENV", "development"}
                    };
                });

            return services;
        }

        /// <summary>
        /// UseJering method to start using in Startup.
        /// </summary>
        /// <param name="app">Application request pipeline.</param>
        /// <param name="hostEnvironment">Hosting environment.</param>
        /// <returns>Extension method return parameter.</returns>
        public static IApplicationBuilder UseNodejsService(
            this IApplicationBuilder app,
            IHostEnvironment hostEnvironment,
            string? assetsPath)
        {
            string staticAssetsPath = assetsPath ?? "./build/client";
            return app
                .UseStaticFiles(new StaticFileOptions()
                {
                    HttpsCompression = HttpsCompressionMode.Compress,
                    FileProvider = new PhysicalFileProvider(Path.Join(hostEnvironment?.ContentRootPath, staticAssetsPath))
                });
        }

        /// <summary>
        /// Invoke nodejs service with the given request.
        /// </summary>
        /// <param name="nodeJSService"><see cref="INodeJSService"/>.</param>
        /// <param name="options"><see cref="NodejsOptions"/>.</param>
        /// <param name="nodejsRequest"><see cref="INodejsRequest"/>.</param>
        /// <param name="overrides"><see cref="RequestOverrides"/>.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>html content.</returns>
        public static async Task<string> InvokeNodejsServiceAsync(
            this INodeJSService nodeJSService,
            NodejsOptions options,
            ValueTask<INodejsRequest> nodejsRequest,
            RequestOverrides? overrides = null,
            CancellationToken cancellationToken = default)
        {
            Debug.Assert(nodeJSService != null);
            Debug.Assert(options != null);

            NodejsResponse? resp = await nodeJSService.InvokeNodejsService(
                options,
                nodejsRequest,
                false,
                false,
                overrides,
                cancellationToken).ConfigureAwait(false);

            return resp == null ? string.Empty : resp.Body;
        }

        /// <summary>
        /// Setup nodejs request.
        /// </summary>
        /// <param name="context"><see cref="HttpContet"/>.</param>
        /// <param name="bodyOnlyReply">instruct the nodejs server to return body and ignore headers and status.</param>
        /// <param name="overrides">overrides the script path on the server.</param>
        /// <returns><see cref="INodejsRequest"/>.</returns>
        public static async ValueTask<INodejsRequest> SetupRequest(
            HttpContext context,
            bool bodyOnlyReply,
            RequestOverrides? overrides = null)
        {
            Debug.Assert(context != null);
            ReadResult bodyResult = EmptyBodyResult;
            HttpRequest request = context.Request;
            IDictionary<string, string> headers = context.Request.Headers
                .ToDictionary(k => k.Key.StartsWith(':') ? k.Key[1..] : k.Key, v => v.Value.ToString());

            if (request.ContentLength > 0)
            {
                bodyResult = await request.BodyReader.ReadAsync().ConfigureAwait(false);
            }

            return SetupRequest(
                httpMethod: request.Method,
                headers: headers,
                queryString: request.QueryString.ToString(),
                hostname: request.Host.ToString(),
                routePath: overrides == null ? request.Path : overrides.Path,
                requestBody: bodyResult.Buffer,
                scheme: request.Scheme,
                bodyOnlyReply);
        }

        /// <summary>
        /// Setup nodejs request.
        /// </summary>
        /// <param name="httpMethod">Http Method.</param>
        /// <param name="headers">Http Headers.</param>
        /// <param name="queryString">Query string in the request.</param>
        /// <param name="hostname">Hostname in the request.</param>
        /// <param name="routePath">Request path.</param>
        /// <param name="requestBody">Request payload.</param>
        /// <param name="bodyOnlyReply">Instruct nodejs service to return just the body (without headers and status).</param>
        /// <returns><see cref="INodejsRequest"/>.</returns>
        public static INodejsRequest SetupRequest(
            string httpMethod,
            IDictionary<string, string>? headers,
            string? queryString,
            string hostname,
            string routePath,
            System.Buffers.ReadOnlySequence<byte> requestBody,
            string scheme,
            bool bodyOnlyReply)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>
                {
                    { "Accept", "text/html" }
                };
            }

            if (bodyOnlyReply)
            {
                headers.Add("x-component", "true");
            }

            NodejsDefaultRequest req = new(
                httpMethod,
                headers,
                routePath,
                queryString!,
                hostname,
                scheme
                );

            req.Body = Encoding.UTF8.GetString(requestBody);

            req.BodyOnlyReply = bodyOnlyReply;

            return req;
        }

        internal static async Task<NodejsResponse?> InvokeNodejsService(
            this INodeJSService nodeJSService,
            NodejsOptions options,
            HttpContext context,
            bool? overrideBodyOnlyReply = null,
            CancellationToken cancellationToken = default)
        {
            return await InvokeNodejsService(nodeJSService, options, context, false, overrideBodyOnlyReply, null, cancellationToken)
                .ConfigureAwait(false);
        }

        internal static async Task<NodejsResponse?> InvokeNodejsService(
            this INodeJSService nodeJSService,
            NodejsOptions options,
            HttpContext context,
            bool shouldGzipCompress,
            bool? overrideBodyOnlyReply = null,
            CancellationToken cancellationToken = default)
        {
            return await InvokeNodejsService(nodeJSService, options, context, shouldGzipCompress, overrideBodyOnlyReply, null, cancellationToken)
                .ConfigureAwait(false);
        }

        internal static async Task<NodejsResponse?> InvokeNodejsService(
            this INodeJSService nodeJSService,
            NodejsOptions options,
            HttpContext context,
            bool shouldGzipCompress,
            bool? overrideBodyOnlyReply = null,
            RequestOverrides? overrides = null,
            CancellationToken cancellationToken = default)
        {
            ValueTask<INodejsRequest> nodeReq = SetupRequest(
                    context,
                    overrideBodyOnlyReply ?? options.BodyOnlyReply,
                    overrides);

            return await nodeJSService.InvokeNodejsService(
                    options,
                    nodeReq,
                    shouldGzipCompress,
                    overrideBodyOnlyReply,
                    overrides,
                    cancellationToken).ConfigureAwait(false);
        }

        internal static async Task<NodejsResponse?> InvokeNodejsService(
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

            bool bodyOnlyReply = overrideBodyOnlyReply ?? options.BodyOnlyReply;

            object[] arguments = new object[]
            {
                await nodejsRequest.ConfigureAwait(false),
            };

            if (bodyOnlyReply == true)
            {
                Stream? streamResp = await nodeJSService.InvokeFromFileAsync<Stream>(
                   modulePath: options.ScriptPath,
                   args: arguments,
                   cancellationToken: cancellationToken).ConfigureAwait(false);

                return streamResp == null ? null : new NodejsBodyOnlyResponse(streamResp);
            }

            return await nodeJSService.InvokeFromFileAsync<NodejsResponse>(
                    modulePath: options.ScriptPath,
                    args: arguments,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        internal static async Task<Stream> CompressContentAsync(this Stream input)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            GZipStream compressor = new(new MemoryStream(), CompressionLevel.Fastest);
#pragma warning restore CA2000 // Dispose objects before losing scope
            await input.CopyToAsync(compressor).ConfigureAwait(false);
            await compressor.FlushAsync().ConfigureAwait(false);
            input.Close();
            compressor.BaseStream.Position = 0;
            return compressor.BaseStream;
        }
    }
}
