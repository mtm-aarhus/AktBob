using PdfSharp.Fonts;

namespace AktBob.DocumentGenerator;
internal class CustomFontResolver : IFontResolver
{
    public static readonly CustomFontResolver Instance = new CustomFontResolver();

    // Map font family names to the specific font files
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("OpenSans-SemiBold", StringComparison.OrdinalIgnoreCase))
        {
            return new FontResolverInfo("OpenSans-SemiBold");
        }

        if (familyName.Equals("OpenSans", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold && isItalic)
                return new FontResolverInfo("OpenSans-BoldItalic");
            if (isBold)
                return new FontResolverInfo("OpenSans-Bold");
            if (isItalic)
                return new FontResolverInfo("OpenSans-Italic");

            return new FontResolverInfo("OpenSans-Regular");
        }

        return null; // Return null if the font is not recognized
    }

    // Provide the font data for the specified font
    public byte[] GetFont(string faceName)
    {
        switch (faceName)
        {
            case "OpenSans-Regular":
                return LoadFontData("OpenSans-Regular.ttf");
            case "OpenSans-SemiBold":
                return LoadFontData("OpenSans-SemiBold.ttf");
            case "OpenSans-Bold":
                return LoadFontData("OpenSans-Bold.ttf");
            case "OpenSans-Italic":
                return LoadFontData("OpenSans-Italic.ttf");
            case "OpenSans-BoldItalic":
                return LoadFontData("OpenSans-BoldItalic.ttf");
        }

        throw new ArgumentException($"Unknown font face: {faceName}");
    }

    private byte[] LoadFontData(string fontFileName)
    {
        // Load the font data from the file (adjust the path to your needs)
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", fontFileName);
        return File.ReadAllBytes(fontPath);
    }
}