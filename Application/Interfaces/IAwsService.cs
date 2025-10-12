namespace Application.Interfaces
{
    public interface IAwsService
    {
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "");
        Task<byte[]> GetFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsyncWithNoPath(string filePath);
    }
}
