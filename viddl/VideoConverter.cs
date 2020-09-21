using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace viddl {
  class VideoConverter {
    Process info_process;
    Process download_process;

    public int Download(string url, DataReceivedEventHandler outputHandler) {
      // [Ref] stackoverflow.com/a/12436300/4824627
      // [Ref] docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process.beginerrorreadline
      download_process = new Process();
      string videos_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
      string path = Path.Combine(videos_folder, "%(id)s-%(title)s.%(ext)s");
      string flags = $"--ignore-errors --ignore-config --no-color --no-playlist --newline --output \"{path}\" --restrict-filenames";
      ProcessStartInfo psi = new ProcessStartInfo {
        UseShellExecute = false,
        // Using youtube-dl: github.com/ytdl-org/youtube-dl
        FileName = @"Resources\youtube-dl.exe",
        Arguments = $"{flags} {url}",
        RedirectStandardOutput = true,
        // [Ref] stackoverflow.com/a/6857053/4824627
        WindowStyle = ProcessWindowStyle.Hidden,
        // [Ref] stackoverflow.com/a/13269085/4824627
        CreateNoWindow = true
      };
      download_process.StartInfo = psi;
      download_process.OutputDataReceived += outputHandler;

      Console.WriteLine($"Executing [{download_process.StartInfo.FileName} {download_process.StartInfo.Arguments}]");
      download_process.Start();
      download_process.BeginOutputReadLine();
      download_process.WaitForExit();
      int exit_code = download_process.ExitCode;
      download_process.Close();
      download_process.Dispose();
      download_process = null;
      return exit_code;
    }

    public int GetInfo(string url, DataReceivedEventHandler outputHandler) {
      info_process = new Process();
      string flags = "--ignore-errors --ignore-config --no-color --no-playlist --dump-json";
      ProcessStartInfo psi = new ProcessStartInfo {
        UseShellExecute = false,
        // Using youtube-dl: github.com/ytdl-org/youtube-dl
        FileName = @"Resources\youtube-dl.exe",
        Arguments = $"{flags} {url}",
        RedirectStandardOutput = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true
      };
      info_process.StartInfo = psi;
      info_process.OutputDataReceived += outputHandler;

      Console.WriteLine($"Executing [{info_process.StartInfo.FileName} {info_process.StartInfo.Arguments}]");
      info_process.Start();
      info_process.BeginOutputReadLine();
      info_process.WaitForExit();
      int exit_code = info_process.ExitCode;
      info_process.Close();
      info_process.Dispose();
      info_process = null;
      return exit_code;
    }

    public void Kill() {
      if(info_process != null && !info_process.HasExited)
        info_process.Kill();
      if(download_process != null && !download_process.HasExited)
        download_process.Kill();
    }
  }
}
