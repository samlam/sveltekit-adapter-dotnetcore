namespace Jering
{
    /// <summary>
    /// Standard node.js response.
    /// </summary>
    internal class NodejsResponse
	{
		/// <summary>
		/// Gets or sets Http status code.
		/// </summary>
		public int Status { get; set; }

		/// <summary>
		/// Gets or sets Http headers.
		/// </summary>
		public Dictionary<string, string>? Headers { get; set; }

		/// <summary>
		/// Gets or sets Body.
		/// </summary>
		public string? Body { get; set; }

		/// <summary>
		/// Gets body byte array.
		/// </summary>
		public virtual Stream? BodyStream => string.IsNullOrWhiteSpace(Body)
			? null
			: new MemoryStream(System.Text.Encoding.UTF8.GetBytes(Body));
	}
}