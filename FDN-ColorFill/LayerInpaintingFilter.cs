using FilterDotNet.Interfaces;

namespace FDN_ColorFill
{
    public class LayerInpaintingFilter : IFilter, IConfigurableFilter
    {
        /* DI */
        private readonly IPluginConfigurator<IImage> _pluginConfigurator;
        private readonly IEngine _engine;

        /* Internals */
        private bool _ready = false;
        private IImage _colorLayer;

        /* Properties */
        public string Name => "Layer In-Painting";

        public LayerInpaintingFilter(IPluginConfigurator<IImage> pluginConfigurator, IEngine engine)
        {
            this._pluginConfigurator = pluginConfigurator;
            this._engine = engine;
        }

        public IImage Apply(IImage input)
        {
            IImage output = this._engine.CreateImage(input.Width, input.Height);
            throw new NotImplementedException();
        }

        public IFilter Initialize()
        {
            this._colorLayer = this._pluginConfigurator.GetPluginConfiguration();
            this._ready = true;
            return this;
        }
    }
}
