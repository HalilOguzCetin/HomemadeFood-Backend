using HomemadeFood.Api.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HomemadeFood.Api.Services
{
    public class FoodImageStorageService : IFoodImageStorageService
    {
        private const long MaxFileSizeBytes =
            5 * 1024 * 1024;

        private readonly IWebHostEnvironment _environment;

        public FoodImageStorageService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveAsync(
            IFormFile image,
            CancellationToken cancellationToken = default)
        {
            if (image == null || image.Length <= 0)
            {
                throw new ArgumentException(
                    "Yemek fotoğrafı zorunludur.");
            }

            if (image.Length > MaxFileSizeBytes)
            {
                throw new ArgumentException(
                    "Yemek fotoğrafı en fazla 5 MB olabilir.");
            }

            var extension =
                await DetectAndValidateImageTypeAsync(
                    image,
                    cancellationToken);

            var webRootPath =
                string.IsNullOrWhiteSpace(
                    _environment.WebRootPath)
                    ? Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot")
                    : _environment.WebRootPath;

            var foodImageDirectory =
                Path.Combine(
                    webRootPath,
                    "uploads",
                    "foods");

            Directory.CreateDirectory(
                foodImageDirectory);

            /*
             * Kullanıcının gönderdiği dosya adı storage adı
             * olarak kesinlikle kullanılmaz.
             */
            var safeFileName =
                $"{Guid.NewGuid():N}{extension}";

            var physicalPath =
                Path.Combine(
                    foodImageDirectory,
                    safeFileName);

            await using var outputStream =
                new FileStream(
                    physicalPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);

            await image.CopyToAsync(
                outputStream,
                cancellationToken);

            return $"/uploads/foods/{safeFileName}";
        }

        public Task DeleteAsync(
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                !imageUrl.StartsWith(
                    "/uploads/foods/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var fileName =
                Path.GetFileName(imageUrl);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Task.CompletedTask;
            }

            var webRootPath =
                string.IsNullOrWhiteSpace(
                    _environment.WebRootPath)
                    ? Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot")
                    : _environment.WebRootPath;

            var physicalPath =
                Path.Combine(
                    webRootPath,
                    "uploads",
                    "foods",
                    fileName);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        private static async Task<string>
            DetectAndValidateImageTypeAsync(
                IFormFile image,
                CancellationToken cancellationToken)
        {
            /*
             * Sadece Content-Type veya uzantıya güvenmiyoruz.
             * Dosyanın imza byte'larını da kontrol ediyoruz.
             */
            var header = new byte[12];

            await using var inputStream =
                image.OpenReadStream();

            var totalRead = 0;

            while (totalRead < header.Length)
            {
                var read =
                    await inputStream.ReadAsync(
                        header.AsMemory(
                            totalRead,
                            header.Length - totalRead),
                        cancellationToken);

                if (read == 0)
                {
                    break;
                }

                totalRead += read;
            }

            var isJpeg =
                totalRead >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF;

            var isPng =
                totalRead >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A;

            var isWebP =
                totalRead >= 12 &&
                header[0] == (byte)'R' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F' &&
                header[3] == (byte)'F' &&
                header[8] == (byte)'W' &&
                header[9] == (byte)'E' &&
                header[10] == (byte)'B' &&
                header[11] == (byte)'P';

            if (isJpeg)
            {
                EnsureAllowedContentType(
                    image.ContentType,
                    "image/jpeg");

                return ".jpg";
            }

            if (isPng)
            {
                EnsureAllowedContentType(
                    image.ContentType,
                    "image/png");

                return ".png";
            }

            if (isWebP)
            {
                EnsureAllowedContentType(
                    image.ContentType,
                    "image/webp");

                return ".webp";
            }

            throw new ArgumentException(
                "Yalnızca JPG, PNG veya WEBP yemek fotoğrafları yüklenebilir.");
        }

        private static void EnsureAllowedContentType(
            string? actualContentType,
            string expectedContentType)
        {
            if (!string.Equals(
                    actualContentType,
                    expectedContentType,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Fotoğrafın dosya türü doğrulanamadı.");
            }
        }
    }
}