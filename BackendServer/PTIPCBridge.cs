namespace PTSystem;

using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;

public class PTipcBridge(WebView2 wv2) {
    private WebView2 WV2 { get; set; } = wv2;
    public Dictionary<string, Action<string>> IPCSendClient = [];
    public Dictionary<string, Action<string>> IPCReceiver = [];

    private JsonSerializerOptions MyJsonSerializerOptions { get; set; } = new() {
        PropertyNameCaseInsensitive = true
    };
//#################################################################################################################
//--------------------------------------->>>>ADD IPC SENDER AND RECEIVER<<<<--------------------------------------#
//#################################################################################################################
    public void AddSender(string eventName) {
        IPCSendClient.Add(eventName, WV2.CoreWebView2.PostWebMessageAsJson);
    }
//#################################################################################################################
    public void AddReceiver(string receiverName, Action<string> myconnection) {
        IPCReceiver.Add(receiverName, myconnection);
    }
//#################################################################################################################
//------------------------------------->>>>IPC SEND MESSAGES TO FONTEND<<<<---------------------------------------#
//#################################################################################################################
    public void SendMessage(string msg, string targetAction) {
        if (!IPCSendClient.TryGetValue(targetAction, out var Handler)) {
            Console.WriteLine($"Error no Sender: {targetAction}");
            return;
        }
        Handler.Invoke(msg);
    }
//#################################################################################################################
//---------------------------------------->>>>IPC RECEIVER EVENT LISTENER<<<<-------------------------------------#
//#################################################################################################################
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
class ReceiverMSG(string name, string data) {
    public string Name { get; set; } = name;
    public string Data { get; set; } = data;
}