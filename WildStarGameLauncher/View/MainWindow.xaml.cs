using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using WildStarGameLauncher.Model;
using WildStarGameLauncher.Static;
using WildStarGameLauncher.Static.Natives;
using WildStarGameLauncher.ViewModel;

namespace WildStarGameLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        public MainWindow()
        {
            InitializeComponent();
            btnLaunchWildstar.IsEnabled = false;
            
            SetupListbox();

            SetButtonState();
        }

        private void SetupListbox()
        {
            listBox.SelectionMode = SelectionMode.Single;

            foreach (ServerEntry serverEntry in mainWindowViewModel.ServerEntries)
            {
                ListBoxItem entry = new ListBoxItem
                {
                    Content = $"{serverEntry.Name} ({serverEntry.Hostname})"
                };
                listBox.Items.Add(entry);
            }
            listBox.SelectedIndex = 0;
        }

        private void MenuItem_SetWildStarPath_Click(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.HandleFileSelect();

            SetButtonState();
        }

        private void Button_LaunchWildstar_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<GameLanguage, string> clientLanguages = new Dictionary<GameLanguage, string>
            {
                { GameLanguage.English, "en" },
                { GameLanguage.French, "fr" },
                { GameLanguage.German, "de" },
                { GameLanguage.Chinese, "cn" },
            };

            string hostname = listBox.SelectedItem.ToString().Split('(')[1].Replace(")", "");

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            CreateProcess(
                mainWindowViewModel.FilePath,
                $"/auth {hostname} /authNc {hostname} /lang {clientLanguages[mainWindowViewModel.ClientLanguage]} /patcher {hostname} /SettingsKey WildStar /realmDataCenterId 9",
                IntPtr.Zero, IntPtr.Zero, false, 0, IntPtr.Zero, null, ref si, out pi);
        }

        private void BtnAddServer_Click(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.HandleCreateServerEntry();
            listBox.Items.Clear();
            SetupListbox();
        }

        private void BtnRemoveServer_Click(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.DeleteServerEntry(listBox.SelectedIndex);
            listBox.Items.Clear();
            SetupListbox();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainWindowViewModel.Save();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel.Save();
            Application.Current.Shutdown();
        }

        private void SetButtonState()
        {
            if (mainWindowViewModel.FilePath.Length > 0)
            {
                btnSetWildstarLocation.Visibility = Visibility.Hidden;
                btnLaunchWildstar.IsEnabled = true;
            }
            else
            {
                btnSetWildstarLocation.Visibility = Visibility.Visible;
                btnLaunchWildstar.IsEnabled = false;
            }
        }
    }
}
