using System.Drawing;
using FDN_ColorFill;
using FilterDotNet.Filters;
using FilterDotNet.Interfaces;
using FilterDotNet.LibraryConfigurators;
using FastImageProvider;
using FastImageLibrary;

// Take two layers, one with edge information (black/white) and one with color information
IImage outlineLayer = new FIDrawingImage(Bitmap.FromFile(args[0]));
IImage colorLayer = new FIDrawingImage(Bitmap.FromFile(args[1]));
FastImage blockMapImage = new FastImage(Bitmap.FromFile(args[2]));
bool[,] blockMap = new bool[blockMapImage.Width, blockMapImage.Height];

for (int x = 0; x < blockMapImage.Width; x++)
{
    for (int y = 0; y < blockMapImage.Height; y++)
    {
        FastImageColor fic = blockMapImage.GetPixel(x, y);
        if (fic.GetR() == 0 && fic.GetG() == 0 && fic.GetB() == 0)
            blockMap[x, y] = true;
        else
            blockMap[x, y] = false;
    }
}

// Engine provided by FIP
IEngine engine = Injectables.FiEngine;

// Simplify creating LPFs for Filters
IPluginConfigurator<T> CreateConf<T>(Func<T> func) => new LambdaPluginConfigurator<T>(func);

IFilter CreateMinFilter() {
    return new StatisticalFilter(CreateConf(() => 
        new StatisticalFilterConfiguration 
        { 
            Mode = StatisticalFilterMode.MIN, 
            BlockMask = blockMap, 
            Thresholding = false, 
            Threshold = 0 
        }), engine).Initialize();
}

IFilter CreateBlurFilter() {
    return new StatisticalFilter(CreateConf(() =>
        new StatisticalFilterConfiguration 
        { 
            Mode = StatisticalFilterMode.AVERAGE,
            BlockMask= blockMap,
            Thresholding=false,
            Threshold=0
        }), engine).Initialize();
}

IFilter CreateThresholdFilter() {
    return new ThresholdFilter(CreateConf(() => 200), engine).Initialize();
}

IImage thresholded = CreateBlurFilter().Apply(outlineLayer).Then(i => CreateThresholdFilter().Apply(i));

IFilter CreateLayerInpaintingFilter()
{
    return new LayerInpaintingFilter(CreateConf(() =>
    (
        colorLayer,
        thresholded,
        8
    )), engine).Initialize();
}
IImage result = thresholded
    .Then(CreateMinFilter().Apply)
    .Then(CreateLayerInpaintingFilter().Apply);

((FIDrawingImage)result).UnwrapFastImage().ToBitmap().Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
