using System.Windows;
using System.Collections.Generic;
using System;
using System.Windows.Controls;

namespace viddl {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    List<Video> videos;

    public MainWindow() {
      InitializeComponent();

      videos = new List<Video>();
      videolist.ItemsSource = videos;
    }

    public void RefreshList() {
      videolist.Items.Refresh();
    }

    private void AddToList(Video vid) {
      videos.Add(vid);
      RefreshList();
    }

    private void AddBtnClick(object sender, RoutedEventArgs e) {
      string url = url_box.Text;
      url_box.Text = "";
      Video vid = new Video { URL = url };
      AddToList(vid);
      vid.FetchInfo();
    }

    private void DownloadBtnClick(object sender, RoutedEventArgs e) {
      foreach(Video vid in videos) {
        if(!vid.Downloading && !vid.Done) {
          vid.QueueDownload();
        }
      }
    }

    private void CtxRemoveClick(object sender, RoutedEventArgs e) {
      foreach(Video i in videolist.SelectedItems) {
        if(i.Downloading) i.Kill();
        videos.Remove(i);
      }
      RefreshList();
    }
  }
}
