using System.IO;
#if ANDROID
using Android.Graphics;
#elif IOS || MACCATALYST
using CoreGraphics;
using Foundation;
using UIKit;
#elif WINDOWS
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

namespace ProductSnap.Maui.Services;

public interface IBarcodeImageDecoder
{
    Task<string?> DecodeAsync(Stream imageStream, CancellationToken cancellationToken = default);
}

public sealed class BarcodeImageDecoder : IBarcodeImageDecoder
{
    public async Task<string?> DecodeAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        var pixels = await ReadPixelsAsync(imageStream, cancellationToken);
        return EanBarcodeDecoder.Decode(pixels);
    }

    private static async Task<GrayImage> ReadPixelsAsync(Stream imageStream, CancellationToken cancellationToken)
    {
#if ANDROID
        using var bitmap = BitmapFactory.DecodeStream(imageStream)
            ?? throw new InvalidDataException("Le fichier sélectionné n'est pas une image lisible.");
        using var scaled = Scale(bitmap);
        var width = scaled.Width;
        var height = scaled.Height;
        var colors = new int[checked(width * height)];
        scaled.GetPixels(colors, 0, width, 0, 0, width, height);
        var pixels = new byte[colors.Length];
        for (var i = 0; i < colors.Length; i++)
        {
            var color = new Android.Graphics.Color(colors[i]);
            pixels[i] = (byte)((color.R * 299 + color.G * 587 + color.B * 114) / 1000);
        }
        return new GrayImage(width, height, pixels);
#elif IOS || MACCATALYST
        using var data = NSData.FromStream(imageStream)
            ?? throw new InvalidDataException("Impossible de lire le fichier image.");
        using var image = UIImage.LoadFromData(data)
            ?? throw new InvalidDataException("Le fichier sélectionné n'est pas une image lisible.");
        var cgImage = image.CGImage
            ?? throw new InvalidDataException("L'image sélectionnée ne contient aucun pixel.");
        var scale = Math.Min(1d, 1600d / Math.Max(cgImage.Width, cgImage.Height));
        var width = Math.Max(1, (int)(cgImage.Width * scale));
        var height = Math.Max(1, (int)(cgImage.Height * scale));
        var rgba = new byte[checked(width * height * 4)];
        using var colorSpace = CGColorSpace.CreateDeviceRGB();
        using var context = new CGBitmapContext(
            rgba, width, height, 8, width * 4, colorSpace, CGImageAlphaInfo.PremultipliedLast);
        context.DrawImage(new CGRect(0, 0, width, height), cgImage);
        var pixels = new byte[checked(width * height)];
        for (var i = 0; i < pixels.Length; i++)
        {
            var offset = i * 4;
            pixels[i] = (byte)((rgba[offset] * 299 + rgba[offset + 1] * 587 + rgba[offset + 2] * 114) / 1000);
        }
        return new GrayImage(width, height, pixels);
#elif WINDOWS
        using var input = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(input))
        {
            using var memory = new MemoryStream();
            await imageStream.CopyToAsync(memory, cancellationToken);
            writer.WriteBytes(memory.ToArray());
            await writer.StoreAsync().AsTask(cancellationToken);
            writer.DetachStream();
        }
        input.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(input);
        var scale = Math.Min(1d, 1600d / Math.Max(decoder.PixelWidth, decoder.PixelHeight));
        var transform = new BitmapTransform
        {
            ScaledWidth = (uint)Math.Max(1, decoder.PixelWidth * scale),
            ScaledHeight = (uint)Math.Max(1, decoder.PixelHeight * scale)
        };
        using var bitmap = await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform,
            ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var buffer = new Windows.Storage.Streams.Buffer(checked((uint)(width * height * 4)));
        bitmap.CopyToBuffer(buffer);
        using var reader = DataReader.FromBuffer(buffer);
        var rgba = new byte[checked(width * height * 4)];
        reader.ReadBytes(rgba);
        var pixels = new byte[checked(width * height)];
        for (var i = 0; i < pixels.Length; i++)
        {
            var offset = i * 4;
            pixels[i] = (byte)((rgba[offset + 2] * 299 + rgba[offset + 1] * 587 + rgba[offset] * 114) / 1000);
        }
        return new GrayImage(width, height, pixels);
#else
        await Task.CompletedTask;
        throw new PlatformNotSupportedException("Le décodage d'image n'est pas pris en charge sur cette plateforme.");
#endif
    }

#if ANDROID
    private static Bitmap Scale(Bitmap source)
    {
        var scale = Math.Min(1d, 1600d / Math.Max(source.Width, source.Height));
        if (scale >= 1)
        {
            var config = source.GetConfig()
                ?? throw new InvalidDataException("Format d'image non pris en charge.");
            return source.Copy(config, false)
                ?? throw new InvalidDataException("Impossible de préparer l'image sélectionnée.");
        }

        return Bitmap.CreateScaledBitmap(
                   source,
                   Math.Max(1, (int)(source.Width * scale)),
                   Math.Max(1, (int)(source.Height * scale)),
                   true)
               ?? throw new InvalidDataException("Impossible de redimensionner l'image sélectionnée.");
    }
#endif
}

internal sealed class GrayImage(int width, int height, byte[] pixels)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public byte[] Pixels { get; } = pixels;

    public byte this[int x, int y] => Pixels[y * Width + x];
}

internal static class EanBarcodeDecoder
{
    private static readonly string[] LeftOdd =
    [
        "0001101", "0011001", "0010011", "0111101", "0100011",
        "0110001", "0101111", "0111011", "0110111", "0001011"
    ];

    private static readonly string[] LeftEven =
    [
        "0100111", "0110011", "0011011", "0100001", "0011101",
        "0111001", "0000101", "0010001", "0001001", "0010111"
    ];

    private static readonly string[] Right =
    [
        "1110010", "1100110", "1101100", "1000010", "1011100",
        "1001110", "1010000", "1000100", "1001000", "1110100"
    ];

    private static readonly string[] FirstDigitParity =
    [
        "OOOOOO", "OOE OEE".Replace(" ", "", StringComparison.Ordinal),
        "OOEEOE", "OOEEEO", "OEOOEE", "OEEOOE", "OEEEOO",
        "OEOEOE", "OEOEEO", "OEEOEO"
    ];

    public static string? Decode(GrayImage image)
    {
        if (Math.Max(image.Width, image.Height) < 95)
            return null;

        var horizontal = ScanOrientation(image.Width, image.Height, (x, y) => image[x, y]);
        if (horizontal is not null)
            return horizontal;

        return ScanOrientation(image.Height, image.Width, (x, y) => image[y, x]);
    }

    public static bool IsValid(string code)
    {
        if (code.Length != 13 || code.Any(character => !char.IsAsciiDigit(character)))
            return false;

        var sum = 0;
        for (var index = 0; index < 12; index++)
            sum += (code[index] - '0') * (index % 2 == 0 ? 1 : 3);
        return (10 - sum % 10) % 10 == code[12] - '0';
    }

    private static string? ScanOrientation(int width, int height, Func<int, int, byte> pixel)
    {
        var rowStep = Math.Max(1, height / 80);
        for (var y = height / 10; y < height * 9 / 10; y += rowStep)
        {
            var (minimum, maximum) = GetRange(width, y, pixel);
            if (maximum - minimum < 45)
                continue;
            var threshold = (minimum + maximum) / 2;
            var x = 1;
            while (x < width - 3)
            {
                if (pixel(x - 1, y) >= threshold && pixel(x, y) < threshold)
                {
                    var firstEnd = RunEnd(x, y, true, threshold, width, pixel);
                    var secondEnd = RunEnd(firstEnd, y, false, threshold, width, pixel);
                    var thirdEnd = RunEnd(secondEnd, y, true, threshold, width, pixel);
                    var module = (thirdEnd - x) / 3d;
                    if (module >= 0.75 && firstEnd > x && secondEnd > firstEnd && thirdEnd > secondEnd &&
                        Math.Abs((firstEnd - x) - module) < module * 0.65 &&
                        Math.Abs((secondEnd - firstEnd) - module) < module * 0.65 &&
                        Math.Abs((thirdEnd - secondEnd) - module) < module * 0.65)
                    {
                        var code = DecodeAt(x, y, module, threshold, width, pixel);
                        if (code is not null)
                            return code;
                    }
                    x = Math.Max(x + 1, thirdEnd - 1);
                }
                else
                {
                    x++;
                }
            }
        }

        return null;
    }

    private static (byte minimum, byte maximum) GetRange(int width, int y, Func<int, int, byte> pixel)
    {
        byte minimum = byte.MaxValue;
        byte maximum = byte.MinValue;
        var step = Math.Max(1, width / 1000);
        for (var x = 0; x < width; x += step)
        {
            var value = pixel(x, y);
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);
        }
        return (minimum, maximum);
    }

    private static int RunEnd(int start, int y, bool black, int threshold, int width, Func<int, int, byte> pixel)
    {
        while (start < width && (pixel(start, y) < threshold) == black)
            start++;
        return start;
    }

    private static string? DecodeAt(
        int start, int y, double module, int threshold, int width, Func<int, int, byte> pixel)
    {
        if (start + 95 * module >= width)
            return null;
        Span<char> bits = stackalloc char[95];
        for (var bit = 0; bit < bits.Length; bit++)
        {
            var x = Math.Min(width - 1, (int)(start + (bit + 0.5) * module));
            bits[bit] = pixel(x, y) < threshold ? '1' : '0';
        }

        if (!Matches(bits, 0, "101") || !Matches(bits, 45, "01010") || !Matches(bits, 92, "101"))
            return null;

        Span<char> digits = stackalloc char[13];
        Span<char> parity = stackalloc char[6];
        for (var digitIndex = 0; digitIndex < 6; digitIndex++)
        {
            var offset = 3 + digitIndex * 7;
            var pattern = new string(bits.Slice(offset, 7));
            var digit = Array.IndexOf(LeftOdd, pattern);
            if (digit >= 0)
            {
                digits[digitIndex + 1] = (char)('0' + digit);
                parity[digitIndex] = 'O';
                continue;
            }

            digit = Array.IndexOf(LeftEven, pattern);
            if (digit < 0)
                return null;
            digits[digitIndex + 1] = (char)('0' + digit);
            parity[digitIndex] = 'E';
        }

        var firstDigit = Array.IndexOf(FirstDigitParity, new string(parity));
        if (firstDigit < 0)
            return null;
        digits[0] = (char)('0' + firstDigit);

        for (var digitIndex = 0; digitIndex < 6; digitIndex++)
        {
            var offset = 50 + digitIndex * 7;
            var pattern = new string(bits.Slice(offset, 7));
            var digit = Array.IndexOf(Right, pattern);
            if (digit < 0)
                return null;
            digits[digitIndex + 7] = (char)('0' + digit);
        }

        var sum = 0;
        for (var index = 0; index < 12; index++)
            sum += (digits[index] - '0') * (index % 2 == 0 ? 1 : 3);
        var checkDigit = (10 - sum % 10) % 10;
        return checkDigit == digits[12] - '0' ? new string(digits) : null;
    }

    private static bool Matches(ReadOnlySpan<char> bits, int offset, string expected)
        => bits.Slice(offset, expected.Length).SequenceEqual(expected);
}
