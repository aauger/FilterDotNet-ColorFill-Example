using System.Drawing;
using FDN_ColorFill;
using FilterDotNet.Filters;
using FilterDotNet.Interfaces;
using FilterDotNet.LibraryConfigurators;
using FastImageProvider;

// Take two layers, one with edge information (black/white) and one with color information
IImage outlineLayer = new FIDrawingImage(Bitmap.FromFile(args[0]));
IImage colorLayer = new FIDrawingImage(Bitmap.FromFile(args[1]));

// Engine provided by FIP
IEngine engine = Injectables.FiEngine;

// Simplify creating LPFs for Filters
IPluginConfigurator<T> CreateConf<T>(Func<T> func) => new LambdaPluginConfigurator<T>(func);

IFilter CreateMinFilter() {
    return new StatisticalFilter(CreateConf(() => 
        new StatisticalFilterConfiguration 
        { 
            Mode = StatisticalFilterMode.MIN, 
            BlockSize = 8, 
            Thresholding = false, 
            Threshold = 0 
        }), engine).Initialize();
}

IFilter CreateThresholdFilter() {
    return new ThresholdFilter(CreateConf(() => 127), engine).Initialize();
}

IImage thresholdedMinned = CreateThresholdFilter().Apply(outlineLayer)
                       .Then(i => CreateMinFilter().Apply(i));

IFilter CreateLayerInpaintingFilter() {
    return new LayerInpaintingFilter(CreateConf(() => thresholdedMinned), engine).Initialize(); 
}

IImage result = CreateThresholdFilter().Apply(thresholdedMinned);
