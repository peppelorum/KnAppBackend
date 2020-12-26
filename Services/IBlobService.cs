using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Services
{
    public interface IBlobService
    {
        Task<Uri> UploadFileBlobAsync(string blobContainerName, Stream content, string contentType, string fileName);
    }
}