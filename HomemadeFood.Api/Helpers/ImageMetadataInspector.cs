using System.Buffers.Binary;
using Microsoft.AspNetCore.Http;

namespace HomemadeFood.Api.Helpers
{
    public readonly record struct ImageDimensions(
        int Width,
        int Height);

    public static class ImageMetadataInspector
    {
        public static async Task<ImageDimensions>
            ReadDimensionsAsync(
                IFormFile image,
                CancellationToken cancellationToken = default)
        {
            if (image == null || image.Length <= 0)
            {
                throw new ArgumentException(
                    "Fotoğraf okunamadı.");
            }

            await using var inputStream =
                image.OpenReadStream();

            using var memoryStream =
                new MemoryStream();

            await inputStream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var bytes =
                memoryStream.ToArray();

            var dimensions =
                TryReadPngDimensions(bytes) ??
                TryReadJpegDimensions(bytes) ??
                TryReadWebPDimensions(bytes);

            if (
                dimensions == null ||
                dimensions.Value.Width <= 0 ||
                dimensions.Value.Height <= 0
            )
            {
                throw new ArgumentException(
                    "Fotoğraf çözünürlüğü okunamadı.");
            }

            return dimensions.Value;
        }

        public static void EnsureMinimumResolution(
            ImageDimensions dimensions,
            string imageLabel,
            int minimumShortSide = 600,
            int minimumLongSide = 900)
        {
            var shortSide =
                Math.Min(
                    dimensions.Width,
                    dimensions.Height);

            var longSide =
                Math.Max(
                    dimensions.Width,
                    dimensions.Height);

            if (
                shortSide < minimumShortSide ||
                longSide < minimumLongSide
            )
            {
                throw new ArgumentException(
                    $"{imageLabel} yeterli çözünürlükte değil. " +
                    $"Kısa kenar en az {minimumShortSide} px, " +
                    $"uzun kenar en az {minimumLongSide} px olmalıdır. " +
                    $"Seçilen görsel: {dimensions.Width}x{dimensions.Height} px.");
            }
        }

        private static ImageDimensions?
            TryReadPngDimensions(
                byte[] bytes)
        {
            if (bytes.Length < 24)
            {
                return null;
            }

            var isPng =
                bytes[0] == 0x89 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x4E &&
                bytes[3] == 0x47 &&
                bytes[4] == 0x0D &&
                bytes[5] == 0x0A &&
                bytes[6] == 0x1A &&
                bytes[7] == 0x0A;

            if (!isPng)
            {
                return null;
            }

            var width =
                BinaryPrimitives
                    .ReadInt32BigEndian(
                        bytes.AsSpan(
                            16,
                            4));

            var height =
                BinaryPrimitives
                    .ReadInt32BigEndian(
                        bytes.AsSpan(
                            20,
                            4));

            return new ImageDimensions(
                width,
                height);
        }

        private static ImageDimensions?
            TryReadJpegDimensions(
                byte[] bytes)
        {
            if (
                bytes.Length < 4 ||
                bytes[0] != 0xFF ||
                bytes[1] != 0xD8
            )
            {
                return null;
            }

            var index = 2;

            while (index < bytes.Length)
            {
                while (
                    index < bytes.Length &&
                    bytes[index] != 0xFF
                )
                {
                    index++;
                }

                while (
                    index < bytes.Length &&
                    bytes[index] == 0xFF
                )
                {
                    index++;
                }

                if (index >= bytes.Length)
                {
                    break;
                }

                var marker =
                    bytes[index++];

                if (
                    marker == 0xD8 ||
                    marker == 0xD9
                )
                {
                    continue;
                }

                /*
                 * Standalone markerlar segment length taşımaz.
                 */
                if (
                    marker == 0x01 ||
                    marker is >= 0xD0 and <= 0xD7
                )
                {
                    continue;
                }

                /*
                 * SOS sonrası piksel stream'i başlar.
                 * SOF bu noktadan önce bulunmuş olmalıdır.
                 */
                if (marker == 0xDA)
                {
                    break;
                }

                if (index + 2 > bytes.Length)
                {
                    break;
                }

                var segmentLength =
                    BinaryPrimitives
                        .ReadUInt16BigEndian(
                            bytes.AsSpan(
                                index,
                                2));

                if (
                    segmentLength < 2 ||
                    index + segmentLength >
                    bytes.Length
                )
                {
                    break;
                }

                if (
                    IsStartOfFrameMarker(marker) &&
                    segmentLength >= 7
                )
                {
                    var height =
                        BinaryPrimitives
                            .ReadUInt16BigEndian(
                                bytes.AsSpan(
                                    index + 3,
                                    2));

                    var width =
                        BinaryPrimitives
                            .ReadUInt16BigEndian(
                                bytes.AsSpan(
                                    index + 5,
                                    2));

                    return new ImageDimensions(
                        width,
                        height);
                }

                index +=
                    segmentLength;
            }

            return null;
        }

        private static bool IsStartOfFrameMarker(
            byte marker)
        {
            return marker is
                0xC0 or 0xC1 or 0xC2 or 0xC3 or
                0xC5 or 0xC6 or 0xC7 or
                0xC9 or 0xCA or 0xCB or
                0xCD or 0xCE or 0xCF;
        }

        private static ImageDimensions?
            TryReadWebPDimensions(
                byte[] bytes)
        {
            if (bytes.Length < 20)
            {
                return null;
            }

            var isWebP =
                bytes[0] == (byte)'R' &&
                bytes[1] == (byte)'I' &&
                bytes[2] == (byte)'F' &&
                bytes[3] == (byte)'F' &&
                bytes[8] == (byte)'W' &&
                bytes[9] == (byte)'E' &&
                bytes[10] == (byte)'B' &&
                bytes[11] == (byte)'P';

            if (!isWebP)
            {
                return null;
            }

            var offset = 12;

            while (offset + 8 <= bytes.Length)
            {
                var chunkType =
                    System.Text.Encoding.ASCII
                        .GetString(
                            bytes,
                            offset,
                            4);

                var chunkSize =
                    BinaryPrimitives
                        .ReadUInt32LittleEndian(
                            bytes.AsSpan(
                                offset + 4,
                                4));

                var dataOffset =
                    offset + 8;

                if (
                    chunkSize >
                    int.MaxValue ||
                    dataOffset >
                    bytes.Length
                )
                {
                    return null;
                }

                var available =
                    bytes.Length - dataOffset;

                if (chunkSize > available)
                {
                    return null;
                }

                if (
                    chunkType == "VP8X" &&
                    chunkSize >= 10
                )
                {
                    var widthMinusOne =
                        ReadUInt24LittleEndian(
                            bytes,
                            dataOffset + 4);

                    var heightMinusOne =
                        ReadUInt24LittleEndian(
                            bytes,
                            dataOffset + 7);

                    return new ImageDimensions(
                        widthMinusOne + 1,
                        heightMinusOne + 1);
                }

                if (
                    chunkType == "VP8L" &&
                    chunkSize >= 5 &&
                    bytes[dataOffset] == 0x2F
                )
                {
                    var packed =
                        (uint)bytes[dataOffset + 1] |
                        ((uint)bytes[dataOffset + 2] << 8) |
                        ((uint)bytes[dataOffset + 3] << 16) |
                        ((uint)bytes[dataOffset + 4] << 24);

                    var width =
                        (int)(packed & 0x3FFF) + 1;

                    var height =
                        (int)((packed >> 14) & 0x3FFF) + 1;

                    return new ImageDimensions(
                        width,
                        height);
                }

                if (
                    chunkType == "VP8 " &&
                    chunkSize >= 10 &&
                    bytes[dataOffset + 3] == 0x9D &&
                    bytes[dataOffset + 4] == 0x01 &&
                    bytes[dataOffset + 5] == 0x2A
                )
                {
                    var rawWidth =
                        BinaryPrimitives
                            .ReadUInt16LittleEndian(
                                bytes.AsSpan(
                                    dataOffset + 6,
                                    2));

                    var rawHeight =
                        BinaryPrimitives
                            .ReadUInt16LittleEndian(
                                bytes.AsSpan(
                                    dataOffset + 8,
                                    2));

                    return new ImageDimensions(
                        rawWidth & 0x3FFF,
                        rawHeight & 0x3FFF);
                }

                var paddedChunkSize =
                    (long)chunkSize +
                    (chunkSize % 2);

                var nextOffset =
                    (long)dataOffset +
                    paddedChunkSize;

                if (
                    nextOffset >
                    bytes.Length ||
                    nextOffset >
                    int.MaxValue
                )
                {
                    break;
                }

                offset =
                    (int)nextOffset;
            }

            return null;
        }

        private static int ReadUInt24LittleEndian(
            byte[] bytes,
            int offset)
        {
            if (offset + 3 > bytes.Length)
            {
                throw new ArgumentException(
                    "WEBP görsel bilgisi okunamadı.");
            }

            return
                bytes[offset] |
                (bytes[offset + 1] << 8) |
                (bytes[offset + 2] << 16);
        }
    }
}