using System.Net.Http.Headers;

namespace MvcApp.Services;

public class CommonService
{
    public static ByteArrayContent CreateFileContent(IFormFile file)
    {
        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = file.FileName
        };
        return fileContent;
    }
}