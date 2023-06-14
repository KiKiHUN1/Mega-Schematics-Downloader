using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace schema
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double version= 1.0;
        MegaApiClient client = new MegaApiClient();
        datas database = null;
        string parentID = "";
        INode currentNode = null;
        public MainWindow()
        {
            InitializeComponent();
            checkversionAsync();
           
        }
        async Task checkversionAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string server_version = await client.GetStringAsync("https://kgaming.ddns.net/schematics_update/version.txt");
                server_version=server_version.Substring(0, server_version.Length - 1);
                    double server_version2 = double.Parse(server_version, CultureInfo.InvariantCulture);
                if (server_version2 > version)
                {
                    MessageBox.Show("New version available. Press ok to download it");
                    Process.Start(new ProcessStartInfo("https://kgaming.ddns.net/schematics_update/KiKiHUN_software.exe") { UseShellExecute = true });
                    Application.Current.Shutdown();
                }
            }
           
            client.LoginAnonymous();
            BTN_back.IsEnabled = false;
            LB_status.Content = "Click refresh to begin";
        }
        void loadlink()
        {
            Uri folderLink = new Uri("https://mega.nz/folder/EWFAzKIT#uUGjAxvc8TlpnQVLUHl5wg");
            LB_status.Content = "Fetching data...";
            IEnumerable<INode> nodes = null;
            try
            {
                nodes = client.GetNodesFromLink(folderLink);
                LB_status.Content = "Database loaded";
            }
            catch (ApiException)
            {
                LB_status.Content = "Api error";

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unknown error: \n" + ex.Message);
                LB_status.Content = "Unknown error";
            }

            
            database = null;
            GC.Collect();
            database = new datas(nodes);
            parentID = database.getRoot().Id;
            currentNode = database.getRoot();
            Main();
        }

        void listAdd(string name, INode item = null)
        {
            LB_status.Content = "Found: " + name;
            listbox1.Width = mainwindow.Width - 30;
            listbox1.SelectionMode = SelectionMode.Single;
            Grid grid = new Grid();
            ColumnDefinition colDef1 = new ColumnDefinition();
            ColumnDefinition colDef2 = new ColumnDefinition();

            grid.ColumnDefinitions.Add(colDef1);
            grid.ColumnDefinitions.Add(colDef2);

            RowDefinition rowDef1 = new RowDefinition();
            grid.RowDefinitions.Add(rowDef1);

            TextBlock text = new TextBlock();
            text.Text = name;
            text.FontSize = 12;
            text.Width = listbox1.Width - listbox1.Width / 4;
            Button button = new Button();


            System.Drawing.Color myColor;
            if (item.Type == NodeType.Directory)
            {
                button.Content = "Enter";
                myColor = System.Drawing.ColorTranslator.FromHtml("#FF4088FB");
            }
            else
            {
                button.Content = "Download";
                myColor = System.Drawing.ColorTranslator.FromHtml("#FF4FC72F");
            }
            SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(myColor.A, myColor.R, myColor.G, myColor.B));
            button.Background = brush;
            button.FontSize = 12;
            button.Width = listbox1.Width / 4.0 - 30.0;
            button.Click += (sender, EventArgs) => { myButton_Click(sender, EventArgs, item); };
            Grid.SetColumn(text, 0);
            Grid.SetRow(text, 0);
            grid.Children.Add(text);

            Grid.SetColumn(button, 1);
            Grid.SetRow(button, 0);
            grid.Children.Add(button);

            listbox1.Items.Add(grid);
        }



        void Main()
        {
            LB_status.Content = "Filling up the list";
            if (!database.IsNulll())
            {

                listbox1.Items.Clear();

                foreach (INode node in database.getnodes())
                {
                    if (node.ParentId == parentID)
                    {
                        listAdd(node.Name, node);
                    }

                    /* string parents = GetParents(node, nodes);
                     Directory.CreateDirectory(parents);
                     Console.WriteLine($"Downloading {parents}\\{node.Name}");
                     client.DownloadFile(node, System.IO.Path.Combine(parents, node.Name));*/
                }
                if (currentNode.Type == NodeType.Root)
                {
                    BTN_back.IsEnabled = false;
                }
                else
                {
                    BTN_back.IsEnabled = true;
                }
                // client.Logout();
                LB_status.Content = "Ready";
            }
            else
            {
                LB_status.Content = "Local database is empty";
            }

        }

        void myButton_Click(object sender, RoutedEventArgs e, INode item)
        {
            currentNode = item;
            if (item.Type == NodeType.Directory)
            {
                parentID = item.Id;
                Main();
            }
            else
            {
                string parents = database.GetParents(item);
                Directory.CreateDirectory(parents);
                LB_status.Content = "Downloading: " + item.Name;

                if (!File.Exists(System.IO.Path.Combine(parents, item.Name)))
                {
                    try
                    {
                        client.DownloadFile(item, System.IO.Path.Combine(parents, item.Name));
                        LB_status.Content = "Downloaded";
                    }
                    catch (ApiException)
                    {
                        LB_status.Content = "Api error";
                    }
                    catch (DownloadException)
                    {
                        LB_status.Content = "Download Error";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unknown error: " + ex.ToString());
                    }
                }
                else
                {
                    LB_status.Content = "Already downloaded";
                }
                string path = Directory.GetCurrentDirectory();
                path += "\\" + parents;
                Process.Start("explorer.exe", @path);


            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LB_status.Content = "Reloading local storage...";
            loadlink();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LB_status.Content = "Back one folder";
            parentID = database.getParentParent(currentNode).Id;
            currentNode = database.getParentParent(currentNode);
            Main();
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            path += "\\schema+boarview";
            if (Directory.Exists(path))
            {
                LB_status.Content = "Opening file explorer";
                Process.Start("explorer.exe", @path);
                LB_status.Content = "Ready";
            }
            else
            {
                LB_status.Content = "No local folder found";
            }

        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://mega.nz/filerequest/QK_jfHIDbGk") { UseShellExecute = true });
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("If you found a bug please report on discord \n ID: kikihun");
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           MessageBox.Show("A good coffee is always good");
        }
    }
}
