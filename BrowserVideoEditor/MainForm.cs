using BrowserVideoEditor.WebSockets;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.IO;
using System.Windows.Forms;
using WebSocketSharp.Server;

namespace BrowserVideoEditor
{
    public partial class MainForm : Form
    {
        //private ChromiumWebBrowser webBrowser {  get; set; }
        private WebSocketServer server;
        public MainForm()
        {
            Cef.Initialize(new CefSettings());

            InitializeComponent();
            

        }

        private void InitWebSocket()
        {
            server = new WebSocketServer("ws://localhost:5843");
            server.AddWebSocketService<ProcessBehavior>("/videoEdit");
            server.Start();
        }
        private void InitWebBrowser()
        {
            string relativePath = "../../WebUIs/video_editor.html";
            string fullPath = Path.GetFullPath(relativePath);
            string fileUri = new Uri(fullPath).AbsoluteUri;
            webBrowser.Load(fileUri);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitWebSocket();
            InitWebBrowser();
        }
    }
}
