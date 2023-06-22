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
        INode currentNode = null;

        public MainWindow()
        {
            InitializeComponent();
            checkversionAsync();
            CV_search.Visibility = Visibility.Hidden;
            BTN_back.IsEnabled = false;
            BTN_refresh.IsEnabled = false;
            BTN_home.IsEnabled = false;
            BTN_Search_clear.Visibility = Visibility.Hidden;
            BTN_search.Visibility = Visibility.Hidden;
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
            try
            {
                client.LoginAnonymous();
                LB_status.Content = "Click refresh to begin";
                BTN_refresh.IsEnabled = true;
            }
            catch (ApiException)
            {
                LB_status.Content = "api error";
            }
           
        }
        void loadlink()
        {
            BTN_home.IsEnabled = false;
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
            button.Click += (sender, EventArgs) => { Download_click(sender, EventArgs, item); };
            Grid.SetColumn(text, 0);
            Grid.SetRow(text, 0);
            grid.Children.Add(text);

            Grid.SetColumn(button, 1);
            Grid.SetRow(button, 0);
            grid.Children.Add(button);

            listbox1.Items.Add(grid);
        }



        void Main(bool filtered=false)
        {
            CV_search.Visibility = Visibility.Hidden;
            LB_status.Content = "Filling up the list";
            if (!database.IsNulll())
            {

                listbox1.Items.Clear();
                foreach (INode node in database.getnodes(filtered))
                {
                    if (filtered||node.ParentId == currentNode.Id)
                    {
                        listAdd(node.Name, node);
                    }

                    /* string parents = GetParents(node, nodes);
                     Directory.CreateDirectory(parents);
                     Console.WriteLine($"Downloading {parents}\\{node.Name}");
                     client.DownloadFile(node, System.IO.Path.Combine(parents, node.Name));*/
                }
                if (!filtered)
                {
                    if (currentNode.Type == NodeType.Root)
                    {
                        BTN_back.IsEnabled = false;
                    }
                    else
                    {
                        BTN_back.IsEnabled = true;
                        BTN_home.IsEnabled = true;
                    }
                }
               
                // client.Logout();
                LB_status.Content = "Ready";
                CV_search.Visibility= Visibility.Visible;
                
            }
            else
            {
                CV_search.Visibility = Visibility.Hidden;
                BTN_home.IsEnabled = false;
                LB_status.Content = "Local database is empty";
            }

        }

        void Download_click(object sender, RoutedEventArgs e, INode item)
        {
            currentNode = item;
            if (item.Type == NodeType.Directory)
            {
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

        private void Refresh_click(object sender, RoutedEventArgs e)
        {
            LB_status.Content = "Reloading local storage...";
            loadlink();
        }

        private void Back_click(object sender, RoutedEventArgs e)
        {
            LB_status.Content = "Back one folder";
            currentNode = database.getParentParent(currentNode);
            Main();
        }

        private void Open_downloaded_click(object sender, RoutedEventArgs e)
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

        private void Upload_click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://mega.nz/filerequest/QK_jfHIDbGk") { UseShellExecute = true });
        }

        private void Info_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("If you found a bug please report on github \n https://github.com/KiKiHUN1/Mega-Schematics-Downloader/issues/new/choose");
        }

        private void Github_click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/KiKiHUN1/Mega-Schematics-Downloader") { UseShellExecute = true });
        }

        private void Home_click(object sender, RoutedEventArgs e)
        {
            currentNode=database.getRoot();
            Main();
        }

        private void Search_clear_click(object sender, RoutedEventArgs e)
        {
            LB_status.Content = "Reloading local storage...";
            TB_search.Clear();
            BTN_refresh.IsEnabled = true;
            Main();
        }

        private void BTN_search_Click(object sender, RoutedEventArgs e)
        {
            BTN_refresh.IsEnabled= false;
            BTN_back.IsEnabled=false;
            BTN_home.IsEnabled=false;
            int count=database.SearchFor(TB_search.Text);
            if (count > 0)
            {
                LB_status.Content = count + " items found";
                Main(true);
            }
            else
            {
                LB_status.Content = "no items found";
                listbox1.Items.Clear();
            }
        }

        private void TB_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TB_search.Text!="")
            {
                BTN_Search_clear.Visibility = Visibility.Visible;
                BTN_search.Visibility = Visibility.Visible;
            }
            else
            {
                BTN_Search_clear.Visibility = Visibility.Hidden;
                BTN_search.Visibility = Visibility.Hidden;
            }
        }
    }
}
