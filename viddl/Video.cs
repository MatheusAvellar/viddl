using System;
using System.Diagnostics;
using System.Globalization;
using System.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace viddl {
  class Video {
    public string Title { get; set; } = "Fetching information";
    public string Resolution { get; set; } = "(?)x(?)";
    public string Uploader { get; set; } = "by (loading)";
    public float Opacity { get; set; } = 1;
    public double Completion { get; set; } = 0;
    public bool Indeterminate { get; set; } = true;
    public string Duration { get; set; } = "?";
    public Brush Color { get; set; } = Brushes.Gray;
    public string URL { get; set; }
    public bool Downloading { get; set; }
    public bool Done { get; set; }

    VideoConverter converter = new VideoConverter();

    private MainWindow mainWindow;

    public Video() {
      // [Ref] stackoverflow.com/a/2219218/4824627
      mainWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;
    }

    public async void FetchInfo() {
      await Task.Run(() => {
        converter.GetInfo(URL, InfoOutputHandler);
      });
    }

    public async void QueueDownload() {
      Downloading = true;
      Color = Brushes.DeepSkyBlue;
      mainWindow.RefreshList();
      await Task.Run(() => {
        converter.Download(URL, DownloadOutputHandler);
      });
    }

    public void Kill() {
      converter.Kill();
    }

    private void InfoOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
      if(!string.IsNullOrEmpty(outLine.Data) && outLine.Data.StartsWith("{")) {
        Console.WriteLine(outLine.Data);
        JsonValue json = JsonValue.Parse(outLine.Data);

        Application.Current.Dispatcher.Invoke(new Action(() => {
          string title = RemoveQuotes(JsonGetKey(json, "title"));

          if(JsonHasKey(json, "extraction") && json["extractor"] == "twitter") {
            title = title.Split(new string[] { " - " }, StringSplitOptions.None)[1];
          }

          Title = title;
          double duration = 0;
          if(JsonHasKey(json, "duration"))
            duration = json["duration"];
          Duration = FormatDuration(duration);

          int fps = 0;
          if(JsonHasKey(json, "fps"))
            fps = json["fps"];

          if(JsonHasKey(json, "width") && JsonHasKey(json, "height"))
            Resolution = FormatResolution(json["width"], json["height"], fps);
          else
            Resolution = "—";
          Uploader = $"by {RemoveQuotes(json["uploader"])}";

          if(!Downloading)
            Color = Brushes.Goldenrod;
          mainWindow.RefreshList();
        }), DispatcherPriority.ContextIdle);
      }
    }

    private bool JsonHasKey(JsonValue json, string key) {
      return json.ContainsKey(key) && json[key] != null;
    }

    private string JsonGetKey(JsonValue json, string key) {
      if(JsonHasKey(json, key)) return json[key];
      return "—";
    }

    private void DownloadOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
      string line = outLine.Data;
      if(!string.IsNullOrEmpty(line)) {
        if(line.StartsWith("[download]")) {
          Console.Write(URL + " - ");
          Console.WriteLine(line);

          Regex rx = new Regex(@"\[download\]\s*(?<pct>[0-9]+(?:\.[0-9])?)%", RegexOptions.Compiled);
          foreach(Match match in rx.Matches(line)) {
            GroupCollection groups = match.Groups;

            double percentage = double.Parse(groups["pct"].Value, CultureInfo.InvariantCulture);
            if(percentage >= 0) {
              Indeterminate = false;
              Color = Brushes.Green;
              Completion = Math.Max(percentage,0.1);
            }
            if(percentage >= 100) {
              Done = true;
              Color = Brushes.LightGreen;
              Opacity = 0.65F;
            }
            // [Ref] stackoverflow.com/a/24271455/4824627
            Application.Current.Dispatcher.Invoke(new Action(() => {
              mainWindow.RefreshList();
            }), DispatcherPriority.ContextIdle);
          }
        }
      }
    }

    private string RemoveQuotes(string str) {
      return str.Substring(0, str.Length);
    }

    private string FormatDuration(double seconds) {
      if(seconds == 0) return "—";
      int minutes = (int)(seconds / 60);
      seconds = (int)(seconds - (minutes * 60));
      if(minutes > 0) {
        // todo: stringbuilder
        if(seconds > 0)
          return $"{minutes}min {seconds}s";
        return $"{minutes}min";
      }
      return $"{seconds}s";
    }

    private string FormatResolution(int w, int h, int fps) {
      if(fps > 0) {
        return $"{w}x{h} @ {fps}fps";
      }
      return $"{w}x{h}";
    }
  }
}
