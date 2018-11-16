using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TheVoodooProject.API.Models;

namespace TheVoodooProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private static string text;

        public FileUploadController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<ActionResult> UploadFile([FromForm(Name = "file")] IFormFile file)
        {
            try
            {
                const string folderName = "Upload";
                var webRootPath = hostingEnvironment.ContentRootPath;
                var newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (file.Length <= 0)
                {
                    return Json("Upload Successful.");
                }

                var fullPath = Path.Combine(newPath, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                text = await SpeechToText(fullPath);

                return Json(new { text });
            }
            catch (Exception ex)
            {
                return Json("Failed: " + ex.Message);
            }
        }

        private async Task<string> SpeechToText(string fullPath)
        {
            var config = SpeechConfig.FromSubscription("13beeb913c1341feb3e714b7bb21f85f ", "westeurope");

            var stopRecognition = new TaskCompletionSource<int>();

            using (var audioInput = AudioConfig.FromWavFileInput(fullPath))
            {
                text = null;

                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    recognizer.Recognizing += (s, e) => { };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            text = e.Result.Text;
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
//                            text = "NOMATCH: Speech could not be recognized.";
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
//                        text = "CANCELED: Reason={e.Reason}";

                        if (e.Reason == CancellationReason.Error)
                        {
//                            text = $"CANCELED: ErrorCode={e.ErrorCode}";
//                            text += $"CANCELED: ErrorDetails={e.ErrorDetails}";
//                            text += $"CANCELED: Did you update the subscription info?";
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
//                        text = "\n    Session started event.";
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
//                        text = "\n    Session stopped event.";
//                        text += "\nStop recognition.";
                        stopRecognition.TrySetResult(0);
                    };

                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    Task.WaitAny(stopRecognition.Task);

                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

                    return text;
                }
            }
        }

        public ActionResult TextToSentiment()
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
            {
                Endpoint = "https://westeurope.api.cognitive.microsoft.com"
            };

            var result = client.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", text)
                        })).Result;

            return Json(result);
        }
    }
}
