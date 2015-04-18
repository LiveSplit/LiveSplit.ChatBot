using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.Web;
using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace LiveSplit.ChatBot
{
    public class Component : IComponent
    {
        public Settings Settings { get; set; }

        protected LiveSplitState State { get; set; }
        public Dictionary<String, CSharpScript> Commands { get; set; }

        public float PaddingTop { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }


        public string ComponentName
        {
            get { return "Twitch Chatbot"; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; protected set; }

        public Component()
        {
            Settings = new Settings();
            Commands = new Dictionary<string, CSharpScript>();

            ContextMenuControls = new Dictionary<String, Action>();
            ContextMenuControls.Add("Start Chatbot", Start);
        }

        public void Start()
        {
            if (!Twitch.Instance.IsLoggedIn)
            {
                var thread = new Thread(() => Twitch.Instance.VerifyLogin()) { ApartmentState = ApartmentState.STA };
                thread.Start();
                thread.Join();
            }

            Twitch.Instance.ConnectToChat();
            Twitch.Instance.Chat.OnMessage += OnMessage;

            Reload();
        }

        public String objectToString(object x)
        {
            return x.ToString();
        }

        public void Reload()
        {
            Commands.Clear();
            ContextMenuControls.Clear();

            ContextMenuControls.Add("Reload Commands", Reload);

            if (!string.IsNullOrEmpty(Settings.Path))
            {
                foreach (var file in Directory.EnumerateFiles(Settings.Path))
                {
                    try
                    {
                        var command = new CSharpScript(File.ReadAllText(file));
                        var name = Path.GetFileNameWithoutExtension(file);
                        Commands.Add(name, command);
                        if (name != "anymessage")
                            ContextMenuControls.Add(name, () => ExecuteCommand(command));
                    }
                    catch { }
                }
            }
        }

        private void ExecuteCommand(CSharpScript script, TwitchChat.User user = null, String arguments = "")
        {
            try
            {
                script.Run(State, user, arguments);
            }
            catch (Exception ex)
            { 
            }
        }

        private void OnMessage(object sender, TwitchChat.Message message)
        {
            if (message.Text.StartsWith("!"))
            {
                try
                {
                    var splits = message.Text.Substring(1).Split(new char[] { ' ' }, 2);
                    ExecuteCommand(Commands[splits[0]], message.User, splits.Length > 1 ? splits[1] : "");
                }
                catch { }
            }
            try
            {
                ExecuteCommand(Commands["anymessage"], message.User, message.Text);
            }
            catch { }
        }

        private void PrepareDraw(LiveSplitState state)
        {
            State = state;
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            PrepareDraw(state);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            PrepareDraw(state);
        }

        public float VerticalHeight
        {
            get { return 0; }
        }

        public float MinimumWidth
        {
            get { return 0; }
        }

        public float HorizontalWidth
        {
            get { return 0; }
        }

        public float MinimumHeight
        {
            get { return 0; }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public System.Windows.Forms.Control GetSettingsControl(UI.LayoutMode mode)
        {
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public void RenameComparison(string oldName, string newName)
        {
        }

        public void Update(UI.IInvalidator invalidator, LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
        }

        public void Dispose()
        {
        }
    }
}
