using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System;
using System.Collections.Generic;
using System.IO;
using TheVoodooProject.API.Models;

namespace TheVoodooProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public FileUploadController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost, DisableRequestSizeLimit]
        public ActionResult UploadFile()
        {
            try
            {
                var file = Request.Form.Files[0];
                const string folderName = "Upload";
                var webRootPath = hostingEnvironment.ContentRootPath;
                var newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                if (file.Length <= 0) return Json("Upload Successful.");

                var fullPath = Path.Combine(newPath, file.FileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Json("Upload Successful.");
            }
            catch (Exception ex)
            {
                return Json("Upload Failed: " + ex.Message);
            }
        }

        [HttpGet]
        public void GetResult()
        {
            ITextAnalyticsClient client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials())
            {
                Endpoint = "https://westeurope.api.cognitive.microsoft.com"
            }; //Replace 'westus' with the correct region for your Text Analytics subscription

            SentimentBatchResult result3 = client.SentimentAsync(
                    new MultiLanguageBatchInput(
                        new List<MultiLanguageInput>()
                        {
                          new MultiLanguageInput("en", "0", "I had the best day of my life."),
                          new MultiLanguageInput("en", "1", "This was a waste of my time. The speaker put me to sleep."),
                          new MultiLanguageInput("es", "2", "No tengo dinero ni nada que dar..."),
                          new MultiLanguageInput("it", "3", "L'hotel veneziano era meraviglioso. È un bellissimo pezzo di architettura."),
                        })).Result;

            foreach (var document in result3.Documents)
            {
                Console.WriteLine("Document ID: {0} , Sentiment Score: {1:0.00}", document.Id, document.Score);
            }
        }
    }
}
