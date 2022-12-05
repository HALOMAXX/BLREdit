using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BLREdit.UI.Windows;

public partial class DownloadInfoWindow : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int progress = 0;

    public int DownloadProgress { get { return progress; } set { progress = value; OnPropertyChanged(); } }

    private string infoText = "Downloading File";
    public string InfoText { get { return infoText; } set { infoText = value; OnPropertyChanged(); } }

    private readonly string URL, Filename;

    public DownloadInfoWindow(string url, string filename)
    {
        InitializeComponent();
        URL = url;
        Filename = filename;
    }

    public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
    {
        DownloadProgress = args.ProgressPercentage;
        InfoText = $"{(args.BytesReceived / 1024) / 1024}/{(args.TotalBytesToReceive / 1024) / 1024} MB";
    }

    public void DownloadFinished(object sender, AsyncCompletedEventArgs args)
    {
        IOResources.WebClient.DownloadProgressChanged -= DownloadProgressChanged;
        IOResources.WebClient.DownloadFileCompleted -= DownloadFinished;
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        IOResources.WebClient.DownloadProgressChanged += DownloadProgressChanged;
        IOResources.WebClient.DownloadFileCompleted += DownloadFinished;
        IOResources.WebClient.DownloadFileTaskAsync(URL, Filename);
    }
}
