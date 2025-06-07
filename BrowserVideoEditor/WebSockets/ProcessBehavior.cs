using System;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using WebSocketSharp;
using BrowserVideoEditor.Processes;
using Newtonsoft.Json;
using BrowserVideoEditor.Models;
using BrowserVideoEditor.Commons;

namespace BrowserVideoEditor.WebSockets
{


    public class ProcessBehavior : WebSocketBehavior
    {
        private VideoProcessWrapper _downloader;

        protected override void OnMessage(MessageEventArgs e)
        {
            var request = JsonConvert.DeserializeObject<ProcessRequestModel>(e.Data);

            var urls = CommonFunction.SafeSplitString(request.urls, null);

            if (urls == null)
            {
                Send(JsonConvert.SerializeObject(new ResponseModel()
                {
                    code = (int)Status.INVALIDED,
                    message = "url bị để trống , vui lòng điền link video!"
                }));
            }
            if (request.splitDuration == 0)
            {
                Send(JsonConvert.SerializeObject(new ResponseModel()
                {
                    code = (int)Status.INVALIDED,
                    message = "Độ dài video phải lớn hơn 0 (giây)"
                }));
            }


            _downloader = new VideoProcessWrapper();

            _downloader.InfoReceived += onInfoReceived;

            _downloader.OutputReceived += onOutputReceived;
            _downloader.ErrorReceived += onErrorReceived;
            _downloader.ProgressUpdated += onProgressUpdated;
            //Send($"progress:{p}|{eta}");
            _downloader.DownloadCompleted += onDownloadCompleted;
            //Send($"done:{path}");
            _downloader.SplitCompleted += onSplitCompleted;
            //Send($"splitdone:{folder}");

            Task.Run(async () =>
            {


                try
                {
                    foreach (var url in urls)
                    {
                        await _downloader.DownloadAndSplitAsync(url, request.saveFolderPath, DateTime.Now.Ticks.ToString(), request.splitDuration, request.splitSpace);

                    }


                }
                catch (Exception ex)
                {
                    Send($"error:{ex.Message}");
                }
            });

        }

        private void onSplitCompleted(string saveFolder, string id)
        {
            Send(JsonConvert.SerializeObject(new ResponseModel()
            {
                code = (int)Status.PROCESSED,
                data = new
                {
                    id = id,
                    path = saveFolder
                }
            }));
        }

        private void onDownloadCompleted(string msg, string id)
        {
            Send(JsonConvert.SerializeObject(new ResponseModel()
            {
                code = (int)Status.DOWNLOADED,
                message = msg,
                data = new
                {
                    id = id,
                }
            }));
        }

        private void onProgressUpdated(int percent, string eta, string id)
        {
            Send(JsonConvert.SerializeObject(new ResponseModel()
            {
                code = (int)Status.DOWNLOADING,
                data = new
                {
                    id = id,
                    percent = percent,
                    eta = eta
                }
            }));;
        }

        private void onErrorReceived(string msg, string id)
        {
            Send(JsonConvert.SerializeObject(new ResponseModel()
            {
                code = (int)Status.ERROR,
                message = msg,
                data = new
                {
                    id = id,
                    
                }
            }));
        }

        private void onOutputReceived(string msg, string id)
        {
            //Send(JsonConvert.SerializeObject(new ResponseModel()
            //{
            //    code = (int)Status.PROCESSING,
            //    message = msg,
            //    data = new
            //    {
            //        id = id,
            //    }
            //}));
        }

        private void onInfoReceived(string title, string id, string extension, int duration)
        {
            Send(JsonConvert.SerializeObject(new ResponseModel()
            {
                code = (int)Status.INQUEUE,
                data = new
                {
                    id = id,
                    title = title,
                    extension = extension,
                    duration = duration

                }
            }));
        }
    }

}
