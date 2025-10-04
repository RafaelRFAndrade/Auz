namespace Application.Interfaces
{
    public interface IAwsService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "");
    }
}
