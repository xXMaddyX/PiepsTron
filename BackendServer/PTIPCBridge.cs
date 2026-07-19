namespace PTSystem;

using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;

public class PTipcBridge(WebView2 wv2) {
    private WebView2 WV2 { get; set; } = wv2;
    public Dictionary<string, Action<string?>> IPCSendClient = [];
    public Dictionary<string, Func<string, string?>> IPCReceiver = [];

    private JsonSerializerOptions MyJsonSerializerOptions { get; set; } = new() {
        PropertyNameCaseInsensitive = true
    };

    public void AddSender(string eventName) {
        IPCSendClient.Add(eventName, WV2.CoreWebView2.PostWebMessageAsJson);
    }

    public void SendMessage(string msg, string targetAction) {
        IPCSendClient[targetAction].Invoke(msg);
    }

    public void AddReceiver(string receiverName, Func<string, string?> myconnection) {
        IPCReceiver.Add(receiverName, myconnection);
    }

    public void InitReceiver() {
        WV2.CoreWebView2.WebMessageReceived += (sender, e) => {
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
        };
    }
}

class ReceiverMSG(string name, string data) {
    public string Name { get; set; } = name;
    public string Data { get; set; } = data;
}