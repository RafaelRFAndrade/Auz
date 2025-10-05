using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class AwsService : IAwsService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsService(IConfiguration configuration)
        {
            var region = configuration["AWS:Region"];
            _bucketName = configuration["AWS:Bucket"];

            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];

            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
            {
                _s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
            }
            else
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                _s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));
            }
        }

        /// <summary>
        /// Faz upload de um arquivo genérico no S3 em qualquer pasta do bucket.
        /// </summary>
        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "")
        {
            var key = string.IsNullOrWhiteSpace(folder)
                ? fileName
                : $"{folder.TrimEnd('/')}/{fileName}";

            using var stream = new MemoryStream(fileBytes);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                CannedACL = S3CannedACL.Private
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
        }

        /// <summary>
        /// Baixa um arquivo do S3 a partir do caminho completo (URL ou key do bucket).
        /// </summary>
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            // Se o usuário passar a URL completa, extrai apenas o "key" (caminho relativo dentro do bucket)
            var key = filePath;
            if (filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(filePath);
                key = uri.AbsolutePath.TrimStart('/'); // remove o "/"
            }

            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
