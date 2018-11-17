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
using System.Linq;
using System.Threading.Tasks;
using TheVoodooProject.API.Models;

namespace TheVoodooProject.API.Controllers
{
	public class FileUploadController : Controller
	{
		private readonly IHostingEnvironment hostingEnvironment;
		private static string text;
		private static string fullPath;

		public FileUploadController(IHostingEnvironment hostingEnvironment)
		{
			this.hostingEnvironment = hostingEnvironment;
		}

		[HttpPost("api/FileUpload"), DisableRequestSizeLimit]
		public ActionResult UploadFile([FromForm(Name = "file")] IFormFile file)
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

				fullPath = Path.Combine(newPath, file.FileName);
				using (var stream = new FileStream(fullPath, FileMode.Create))
				{
					file.CopyTo(stream);
				}

				return Json("Upload Successful.");
			}
			catch (Exception ex)
			{
				return Json("Failed: " + ex.Message);
			}
		}

		[HttpGet("api/FileUpload/SpeechToText")]
		public async Task<ActionResult> SpeechToText()
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
						}
					};

					recognizer.Canceled += (s, e) =>
					{

						if (e.Reason == CancellationReason.Error)
						{
						}

						stopRecognition.TrySetResult(0);
					};

					recognizer.SessionStarted += (s, e) =>
					{
					};

					recognizer.SessionStopped += (s, e) =>
					{
						stopRecognition.TrySetResult(0);
					};

					await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

					Task.WaitAny(stopRecognition.Task);

					await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

					return Json(text);
				}
			}
		}

		[HttpGet("api/FileUpload/TextToSentiment")]
		public ActionResult TextToSentimentScore()
		{
			ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
			{
				Endpoint = "https://westeurope.api.cognitive.microsoft.com"
			};

			var result = client.SentimentAsync(new MultiLanguageBatchInput(new List<MultiLanguageInput>
				{
					new MultiLanguageInput("en", "0", text)
				})).Result;

			return Json(result.Documents.First().Score);
		}
	}
}
