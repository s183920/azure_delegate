using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using Azure.Storage.Blobs;

namespace AzureKursusfunction
{
    public static class FileUploadFunction
    {
        [FunctionName("FileUpload")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //var containerName = Environment.GetEnvironmentVariable("ContainerName");
            //var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var containerName = "images";
            var conn = "DefaultEndpointsProtocol=https;AccountName=storageaccforeveryone;AccountKey=CLmrnk0KWKPlIykDCaxT2fTk74++II6q00YW+bDRpUom/+fGE7IA7HOwiHj6ilxJnMA4tdj0oa5C+AStWdJD7g==;EndpointSuffix=core.windows.net";

            try
            {
                //Get File from request and display name and size
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                //Now we have the file as a pointer to a datastream
                //Create an image from the file variable

                

                // --- Hint you could use the System.Drawing.Image
                // --- And convert the file variable to a stream

                var image = Image.FromStream(file.OpenReadStream());

                // --- For resizing on could use new Bitmap(image, size)
                // --- Other example could be the ImageSharp package from nuget.org       

                var resized = new Bitmap(image, new Size(256, 256));

                // --- Save the resized image to a variable of type: MemoryStream (var imageStream = new MemoryStream())
                // --- OBS. set stream position to 0 before uploading image

                using var imageStream = new MemoryStream();
                resized.Save(imageStream, ImageFormat.Jpeg);
                imageStream.Position = 0;


                // We need to save the image to azure,
                // and we've chosen to save it in a blobstorage

                // --- First create a BlobServiceClient (from the Azure.Storage.Blobs library)
                // --- From the BlobServiceClient Create a new Blob container with a proper name (like pics, images, etc.)

                var blobService = new BlobServiceClient(conn);
                // await blobService.CreateBlobContainerAsync(containerName);
                var blobClient = new BlobContainerClient(conn, containerName);

                // When the blob is created we need to uploade the image to it

                // --- Use the BlobContainerClient (From same library as before) to first
                // --- get the blob client using .GetBlobClient
                // --- afterwards Upload the file to the blob

                var blob = blobClient.GetBlobClient(file.FileName);
                await blob.UploadAsync(imageStream);

                //return new OkObjectResult("");
                return new OkObjectResult(file.FileName + " - " + file.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}