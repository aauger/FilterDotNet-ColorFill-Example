using FilterDotNet.Interfaces;
using FilterDotNet.Filters;
using System.Drawing;
using FilterDotNet.LibraryConfigurators;
using FDN_ColorFill;

IEngine engine = FastImageProvider.Injectables.FiEngine;

IImage image = engine.CreateImage(100, 100);

IFilter thresholdFilter = new ThresholdFilter(new LambdaPluginConfigurator<int>(() => 127), engine).Initialize();
IFilter minFilter = new StatisticalFilter(new LambdaPluginConfigurator<StatisticalFilterConfiguration>(
    () => new() { Mode = StatisticalFilterMode.MIN, BlockSize = 8, Thresholding = false, Threshold = 0 }), engine).Initialize();
IImage fPipeline = (new[] { minFilter, thresholdFilter }).Aggregate(image, (i, f) => f.Apply(i));
IFilter layerInpaintingFilter = new LayerInpaintingFilter(new LambdaPluginConfigurator<IImage>(
    () => fPipeline), engine).Initialize();