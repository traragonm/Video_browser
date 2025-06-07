using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BrowserVideoEditor.Processes
{


    public class VideoProcessWrapper
    {
        public event Action<string,string, string, int> InfoReceived;
        public event Action<string,string> OutputReceived;
        public event Action<string, string> ErrorReceived;
        public event Action<int, string,string> ProgressUpdated;
        public event Action<string, string> DownloadCompleted;
        public event Action<string,string> SplitCompleted;

        private readonly string ytdlpPath = "yt-dlp";
        private readonly string ffmpegPath = "ffmpeg";

        public VideoProcessWrapper()
        {
        }

        public async Task DownloadAndSplitAsync(string url, string saveFolder,string id, int segmentDuration, int segmentGap)
        {

            saveFolder = Path.Combine(saveFolder, id);

            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            string videoInfo = await GetVideoInfoAsync(url, id);
            var (title, ext, duration) = ParseVideoInfo(videoInfo);
            InfoReceived?.Invoke(title, id, ext, duration);

            string safeTitle = MakeSafeFileName(title);
            string outputPath = Path.Combine(saveFolder, $"{safeTitle}.{ext}");

            DownloadVideoAsync(url, outputPath, id);
            DownloadCompleted?.Invoke(outputPath, id);

            SplitVideoAsync(outputPath, saveFolder, safeTitle, duration, segmentDuration, segmentGap, id);
            SplitCompleted?.Invoke(saveFolder, id);
        }

        private async Task<string> GetVideoInfoAsync(string url, string id)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ytdlpPath,
                Arguments = $"--print title --print ext --print duration {url}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            string output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("Failed to retrieve video info.");

            return output.Trim();
        }

        private (string title, string ext, int duration) ParseVideoInfo(string info)
        {
            var lines = info.Split('\n');
            if (lines.Length < 3)
                throw new Exception("Invalid video info format");

            string title = lines[0];
            string ext = lines[1];
            int duration = int.TryParse(lines[2], out int d) ? d : throw new Exception("Invalid duration");

            return (title, ext, duration);
        }

        private void DownloadVideoAsync(string url, string outputPath, string id)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ytdlpPath,
                Arguments = $"-o \"{outputPath}\" {url}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = psi };
            process.OutputDataReceived += (s, e) => { 
                if (e.Data != null) {
                    OutputReceived?.Invoke(e.Data, id);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    //ErrorReceived?.Invoke(e.Data, id);
                    TryParseProgress(e.Data,id);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private void SplitVideoAsync(string inputPath, string saveFolderPath, string outputBaseName, int totalDuration, int videoDuration, int videoGap, string id)
        {
            int start = 0;
            int index = 1;

            while (start < totalDuration)
            {

                string outputFile = Path.Combine(saveFolderPath, $"{outputBaseName}_{index:D3}.mp4");
                string args = $"-ss {start} -i \"{inputPath}\" -t {videoDuration} -c copy \"{outputFile}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = psi };
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"ffmpeg failed at segment {index} starting at {start} seconds.");

                start += (videoDuration + videoGap);
                index++;
            }
        }

        private void TryParseProgress(string data, string id)
        {
            var match = Regex.Match(data, @"\\[download\\]\\s+(\\d+\\.\\d+)% of.+ETA (.+)");
            if (match.Success)
            {
                int percent = (int)double.Parse(match.Groups[1].Value);
                string eta = match.Groups[2].Value;
                ProgressUpdated?.Invoke(percent, eta,id);
            }
        }

        private string MakeSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }

}
