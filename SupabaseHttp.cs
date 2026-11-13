using Godot;
using System;
using System.Text;
using System.Threading.Tasks;

// HTTP requests live under /root, so a scene transition cannot destroy one mid-request.
public static class SupabaseHttp
{
    public sealed class Response
    {
        public Error StartError { get; set; } = Error.Ok;
        public long Result { get; set; }
        public long StatusCode { get; set; }
        public byte[] Body { get; set; } = Array.Empty<byte>();

        public bool IsSuccess => StartError == Error.Ok &&
            Result == (long)HttpRequest.Result.Success &&
            StatusCode >= 200 && StatusCode < 300;

        public string BodyText => Encoding.UTF8.GetString(Body);
    }

    public static async Task<Response> SendAsync(
        Node caller, string url, HttpClient.Method method,
        string payload = "", string accessToken = "", string preference = "")
    {
        // Pause is an overlay: the scene tree is paused while Save/Load is open.
        // HTTPRequest must keep processing, or the UI stays at "Checking...".
        var request = new HttpRequest
        {
            Timeout = 20.0,
            ProcessMode = Node.ProcessModeEnum.Always
        };
        caller.GetTree().Root.AddChild(request);
        var completion = new TaskCompletionSource<Response>();

        try
        {
            request.RequestCompleted += (result, status, headers, body) =>
                completion.TrySetResult(new Response
                {
                    Result = result,
                    StatusCode = status,
                    Body = body
                });

            var requestHeaders = new System.Collections.Generic.List<string>
            {
                "apikey: " + SupabaseConfig.PublishableKey,
                "Content-Type: application/json"
            };
            if (!string.IsNullOrEmpty(accessToken))
                requestHeaders.Add("Authorization: Bearer " + accessToken);
            if (!string.IsNullOrEmpty(preference))
                requestHeaders.Add("Prefer: " + preference);

            Error error = request.Request(url, requestHeaders.ToArray(), method, payload);
            if (error != Error.Ok)
                return new Response { StartError = error };

            return await completion.Task;
        }
        finally
        {
            if (GodotObject.IsInstanceValid(request))
                request.QueueFree();
        }
    }
}
