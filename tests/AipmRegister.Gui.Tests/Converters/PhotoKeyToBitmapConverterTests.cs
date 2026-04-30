using AipmRegister.Gui.Converters;
using Avalonia.Media.Imaging;

namespace AipmRegister.Gui.Tests.Converters;

/// Pins the AvaloniaResource contract: every PhotoKey listed in
/// <see cref="AipmRegister.Core.Models.ProductCatalog"/> resolves to a
/// real embedded PNG. Catches accidental rename/deletion before the
/// user navigates to Step 3 in the GUI.
[Collection(HeadlessCollection.Name)]
public sealed class PhotoKeyToBitmapConverterTests
{
    private readonly HeadlessFixture _headless;
    public PhotoKeyToBitmapConverterTests(HeadlessFixture headless) => _headless = headless;

    public static IEnumerable<object[]> AllCatalogPhotoKeys()
        => AipmRegister.Core.Models.ProductCatalog.All
            .Select(p => p.PhotoKey)
            .Distinct()
            .Select(k => new object[] { k });

    [Theory]
    [MemberData(nameof(AllCatalogPhotoKeys))]
    public Task Convert_ReturnsBitmap_ForEveryCatalogPhotoKey(string photoKey)
        => _headless.Run(() =>
        {
            var result = PhotoKeyToBitmapConverter.Instance.Convert(
                photoKey, typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.NotNull(result);
            var bmp = Assert.IsType<Bitmap>(result);
            Assert.True(bmp.PixelSize.Width  > 0);
            Assert.True(bmp.PixelSize.Height > 0);
        });

    [Fact]
    public Task Convert_ReturnsNull_ForEmptyKey()
        => _headless.Run(() =>
        {
            var result = PhotoKeyToBitmapConverter.Instance.Convert(
                string.Empty, typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Null(result);
        });

    [Fact]
    public Task Convert_ReturnsNull_ForNullValue()
        => _headless.Run(() =>
        {
            var result = PhotoKeyToBitmapConverter.Instance.Convert(
                null, typeof(Bitmap), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.Null(result);
        });
}
