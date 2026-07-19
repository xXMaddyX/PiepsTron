namespace PTSystem;

using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;

/// <summary>
/// IPC bridge between the WPF host and the WebView2 frontend, modeled after
/// Electron's <c>ipcMain</c>/<c>ipcRenderer</c> messaging pattern. Channels are
/// identified by name: register a sender or receiver for a channel once, then
/// use that name to push or handle messages on it.
/// </summary>
/// <param name="wv2">The WebView2 control this bridge talks to. Its <see cref="WebView2.CoreWebView2"/> must already be initialized (see <c>EnsureCoreWebView2Async</c>) before calling any method on this bridge.</param>
public class PTipcBridge(WebView2 wv2) {
    private WebView2 WV2 { get; set; } = wv2;
    /// <summary>Registered outgoing channels (backend → frontend), keyed by channel name.</summary>
    public Dictionary<string, Action<string>> IPCSendClient = [];
    /// <summary>Registered incoming channels (frontend → backend), keyed by channel name.</summary>
    public Dictionary<string, Action<string>> IPCReceiver = [];

    /// <summary>JSON options used to deserialize incoming messages (property names matched case-insensitively).</summary>
    private JsonSerializerOptions MyJsonSerializerOptions { get; set; } = new() {
        PropertyNameCaseInsensitive = true
    };
//#################################################################################################################
//--------------------------------------->>>>ADD IPC SENDER AND RECEIVER<<<<--------------------------------------#
//#################################################################################################################
    /// <summary>
    /// Registers an outgoing channel: <see cref="SendMessage"/> can then post JSON to the
    /// frontend under <paramref name="eventName"/> via <c>CoreWebView2.PostWebMessageAsJson</c>.
    /// </summary>
    public void AddSender(string eventName) {
        IPCSendClient.Add(eventName, WV2.CoreWebView2.PostWebMessageAsJson);
    }
//#################################################################################################################
    /// <summary>
    /// Registers a callback for an incoming channel. Whenever the frontend posts a message
    /// whose <c>Name</c> is <paramref name="receiverName"/>, <paramref name="myconnection"/>
    /// is invoked with the message's <c>Data</c> payload.
    /// </summary>
    public void AddReceiver(string receiverName, Action<string> myconnection) {
        IPCReceiver.Add(receiverName, myconnection);
    }
//#################################################################################################################
//------------------------------------->>>>IPC SEND MESSAGES TO FONTEND<<<<---------------------------------------#
//#################################################################################################################
    /// <summary>
    /// Sends a JSON message to the frontend on a previously registered outgoing channel.
    /// </summary>
    /// <param name="msg">The JSON payload to post.</param>
    /// <param name="targetAction">Channel name previously registered via <see cref="AddSender"/>.</param>
    public void SendMessage(string msg, string targetAction) {
        if (!IPCSendClient.TryGetValue(targetAction, out var Handler)) {
            Console.WriteLine($"Error no Sender: {targetAction}");
            return;
        }
        try {
            Handler.Invoke(msg);
        } catch (Exception ex) {
            Console.WriteLine(ex);
            Handler.Invoke("""{"msg": "Error not JSON"}""");
        }
    }
//#################################################################################################################
//---------------------------------------->>>>IPC RECEIVER EVENT LISTENER<<<<-------------------------------------#
//#################################################################################################################
    /// <summary>
    /// Subscribes to <c>CoreWebView2.WebMessageReceived</c> and routes each incoming message
    /// to the receiver registered under its <c>Name</c> (see <see cref="AddReceiver"/>).
    /// Must be called once, after <c>EnsureCoreWebView2Async</c> has completed.
    /// </summary>
    public void InitReceiver() {
        WV2.CoreWebView2.WebMessageReceived += (sender, e) => {
            try {
                ReceiverMSG? ResMSG = JsonSerializer.Deserialize<ReceiverMSG>(e.WebMessageAsJson, MyJsonSerializerOptions);
                if (ResMSG == null) {
                    Console.WriteLine($"Error @ ReceiveMSG from {sender}");
                    return;
                }
                if (!IPCReceiver.TryGetValue(ResMSG.Name, out var handler)) {
                    Console.WriteLine($"No receiver for {ResMSG.Name}");
                    return;
                }
                handler.Invoke(ResMSG.Data);
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        };
    }
}
//#################################################################################################################
//#################################################################################################################
/// <summary>
/// JSON envelope for IPC messages: <c>{ "Name": "...", "Data": "..." }</c>.
/// <c>Name</c> identifies the channel to route to, <c>Data</c> is the payload
/// handed to that channel's callback.
/// </summary>
class ReceiverMSG(string name, string data) {
    public string Name { get; set; } = name;
    public string Data { get; set; } = data;
}