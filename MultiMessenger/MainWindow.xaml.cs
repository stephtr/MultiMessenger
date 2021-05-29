using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Web;
using CommunityToolkit.WinUI.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Windows.UI.Notifications;
using WinUIExtensions.Desktop;

namespace MultiMessenger
{
    public class WebNotification
    {
        public string title { get; set; }
        public string body { get; set; }
        public string icon { get; set; }
    }

    public sealed partial class MainWindow : DesktopWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, out RECT ptr, int size);

        public MainWindow()
        {
            this.InitializeComponent();

            this.Closing += MainWindow_Closing;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomDragRegion);
            var DWMWA_CAPTION_BUTTON_BOUNDS = 5;
            if (DwmGetWindowAttribute(Hwnd, DWMWA_CAPTION_BUTTON_BOUNDS, out var rc, Marshal.SizeOf(typeof(RECT))) != 0)
            {
                throw new Exception("DwmGetWindowAttribute failed");
            }
            CustomDragRegion.MinWidth = rc.right - rc.left;
            CustomDragRegion.MinHeight = rc.bottom - rc.top;

            webView1.CoreWebView2Initialized += CoreWebView2Initialized;
            webView2.CoreWebView2Initialized += CoreWebView2Initialized;
            webView3.CoreWebView2Initialized += CoreWebView2Initialized;
        }

        private void MainWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            e.TryCancel();
            Minimize();
        }

        private static string webViewInjection = Resources.WebViewInjection;
        private void CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            sender.CoreWebView2.Settings.IsWebMessageEnabled = true;
            _ = sender.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(webViewInjection);
            sender.WebMessageReceived += WebMessageReceived;
            sender.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
        }

        private void CoreWebView2_PermissionRequested(CoreWebView2 sender, CoreWebView2PermissionRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void WebMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var message = JsonSerializer.Deserialize<WebNotification>(args.WebMessageAsJson);

            var whatsAppIconPrefix = "https://web.whatsapp.com/pp?e=";
            if (message.icon.StartsWith(whatsAppIconPrefix))
            {
                message.icon = HttpUtility.UrlDecode(message.icon.Substring(whatsAppIconPrefix.Length));

            }

            var notificationContent = new ToastContentBuilder()
                .AddAppLogoOverride(new Uri(message.icon))
                .AddText(message.title)
                .AddText(message.body)
                .AddAttributionText("WhatsApp")
                .GetToastContent();
            var notification = new ToastNotification(notificationContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
