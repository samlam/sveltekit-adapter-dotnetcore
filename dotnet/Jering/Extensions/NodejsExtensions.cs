using System.Diagnostics;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Text;

using Jering.Javascript.NodeJS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        /// <param name="config">Application <see cref="NodejsOptions"/>.</param>
        /// <returns>Extension method return parameter.</returns>
        public static IServiceCollection ConfigureNodejsService(
            this IServiceCollection services, 
            NodejsOptions nodejsOptions)
        {
            if (nodejsOptions == null)
            {
                throw new ArgumentNullException(nameof(nodejsOptions));
            }

            services
                .AddNodeJS()
                .Configure<OutOfProcessNodeJSServiceOptions>(options =>
                {
                    options.Concurrency = nodejsOptions.Concurrency;
                    options.ConcurrencyDegree = nodejsOptions.ConcurrencyDegree;
                    options.TimeoutMS = nodejsOptions.NodejsConnectionTimeoutMS;

                    ApplyDebugJeringOptions(options);
                })
                .Configure<HttpNodeJSServiceOptions>(options => options.Version = HttpVersion.Version11)
                .Configure<NodeJSProcessOptions>(options =>
                {
                    options.NodeAndV8Options = nodejsOptions.NodeAndV8Options;
#if DEBUG
                    options.NodeAndV8Options += " --inspect ";
#endif
                    options.EnvironmentVariables = new Dictionary<string, string>
                    {
                        { "VITE_DotNetPort", $"{nodejsOptions.DotNetPort}"}, // this value needs to match the port # in launchSettings.json
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

        [Conditional("DEBUG")]
        private static void ApplyDebugJeringOptions(OutOfProcessNodeJSServiceOptions options)
        {
            options.Concurrency = Concurrency.None;
            options.EnableFileWatching = true;
            options.WatchPath = "./build/";
            options.TimeoutMS = -1; // -1 to wait forever (used for attaching node.js debugger)
        }

    }
}
