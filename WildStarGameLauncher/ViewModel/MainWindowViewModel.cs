using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WildStarGameLauncher.Model;
using WildStarGameLauncher.Static;

namespace WildStarGameLauncher.ViewModel
{
    class MainWindowViewModel
    {
        public string FilePath { get; private set; } = "";
        public GameLanguage ClientLanguage { get; private set; } = GameLanguage.None;
        public List<ServerEntry> ServerEntries { get; private set; } = new List<ServerEntry>();

        private string serverEntriesFilePath = AppDomain.CurrentDomain.BaseDirectory + "ServerList.json";

        public MainWindowViewModel()
        {
            if (Properties.Settings.Default.FilePath.Length > 0)
                FilePath = Properties.Settings.Default.FilePath;

            if (FilePath != null && FilePath.Length > 0)
            {
                if (File.Exists(FilePath))
                    SetLanguage(FilePath);
                else
                    FilePath = "";
            }

            LoadServers();
        }

        public void HandleFileSelect()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "WildStar Applications|WildStar*.exe";
                
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SetLanguage(dialog.FileName);

                    if(ClientLanguage != GameLanguage.None)
                    {
                        FilePath = dialog.FileName;
                    }
                    else
                    {
                        FilePath = "";
                        MessageBox.Show("This is not a valid client. Ensure you've selected the WildStar client from the Client or Client64 directories.");
                    }
                }
            }
        }

        public void SetLanguage(string path)
        {
            string[] directory = path.Split(Path.DirectorySeparatorChar);
            string patchPath = string.Join(Path.DirectorySeparatorChar.ToString(), directory, 0, directory.Length - 2) + Path.DirectorySeparatorChar + "Patch";
            try
            {
                ClientLanguage = GameLanguage.None;
                string[] patchFiles = Directory.GetFiles(patchPath, "ClientData*.archive");

                foreach (string file in patchFiles)
                {
                    if (ClientLanguage != GameLanguage.None)
                        break;

                    if (file.IndexOf("ClientDataEN") > 0)
                        ClientLanguage = GameLanguage.English;

                    if (file.IndexOf("ClientDataFR") > 0)
                        ClientLanguage = GameLanguage.French;

                    if (file.IndexOf("ClientDataDE") > 0)
                        ClientLanguage = GameLanguage.German;

                    if (file.IndexOf("ClientDataCN") > 0)
                        ClientLanguage = GameLanguage.Chinese;
                }
            }
            catch (Exception e)
            {

            }
        }

        public void CreateServerEntry(string name, string hostname)
        {
            ServerEntry serverEntry = new ServerEntry(name, hostname);
            ServerEntries.Add(serverEntry);
        }

        public bool DeleteServerEntry(int index)
        {
            ServerEntry serverEntry = ServerEntries.GetRange(index, 1).FirstOrDefault();

            if (serverEntry == null)
                return false;

            DeleteServerEntry(serverEntry);
            return true;
        }

        private void DeleteServerEntry(ServerEntry serverEntry)
        {
            ServerEntries.Remove(serverEntry);
        }

        public void HandleCreateServerEntry()
        {
            ServerEntry promptValue = ShowCreateServerDialog();

            if (promptValue != null)
                ServerEntries.Add(promptValue);
        }

        private ServerEntry ShowCreateServerDialog()
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Create Server",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label serverNameLabel = new Label() { Left = 20, Top = 10, Height = 20, Text = "Server Name" };
            TextBox serverNameBox = new TextBox() { Left = 20, Top = 30, Width = 240 };
            Label hostnameLabel = new Label() { Left = 20, Top = 60, Height = 20, Text = "Hostname" };
            TextBox hostnameBox = new TextBox() { Left = 20, Top = 80, Width = 240 };
            Button confirmation = new Button() { Text = "Create", Left = 180, Width = 80, Top = 120, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(serverNameLabel);
            prompt.Controls.Add(serverNameBox);
            prompt.Controls.Add(hostnameLabel);
            prompt.Controls.Add(hostnameBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? new ServerEntry(serverNameBox.Text, hostnameBox.Text) : null;
        }

        public void Save()
        {
            //open file stream
            using (StreamWriter file = File.CreateText(serverEntriesFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, ServerEntries);
            }

            Properties.Settings.Default["FilePath"] = FilePath;
            Properties.Settings.Default.Save();
        }

        private void LoadServers()
        {
            if (File.Exists(serverEntriesFilePath))
            {
                using (StreamReader r = new StreamReader(serverEntriesFilePath))
                {
                    string json = r.ReadToEnd();
                    List<ServerEntry> o = JsonConvert.DeserializeObject<List<ServerEntry>>(json);

                    if (o.Count > 0)
                        ServerEntries = o;
                }
            }
        }
    }
}
