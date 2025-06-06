﻿using System;
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
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private int progress;

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
        if (args is null) { LoggingSystem.Log("failed to update download progess as EventArgs where null"); return; }
        DownloadProgress = args.ProgressPercentage;
        InfoText = $"{(args.BytesReceived / 1024) / 1024}/{(args.TotalBytesToReceive / 1024) / 1024} MB";
    }

    public void DownloadFinished(object sender, AsyncCompletedEventArgs args)
    {
        WebResources.WebClient.DownloadProgressChanged -= DownloadProgressChanged;
        WebResources.WebClient.DownloadFileCompleted -= DownloadFinished;
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        WebResources.WebClient.DownloadProgressChanged += DownloadProgressChanged;
        WebResources.WebClient.DownloadFileCompleted += DownloadFinished;
        WebResources.WebClient.DownloadFileTaskAsync(URL, Filename);
    }
}
