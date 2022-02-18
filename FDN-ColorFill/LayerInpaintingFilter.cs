using FilterDotNet.Exceptions;
using FilterDotNet.Extensions;
using FilterDotNet.Filters;
using FilterDotNet.Interfaces;

namespace FDN_ColorFill
{
    public class LayerInpaintingFilter : IFilter, IConfigurableFilter
    {
        /* DI */
        private readonly IPluginConfigurator<(IImage, IImage, int)> _pluginConfigurator;
        private readonly IEngine _engine;

        /* Internals */
        private bool _ready = false;
        private IImage _colorLayer;
        private IImage _skipLayer;
        private int _distance = 0;

        /* Properties */
        public string Name => "Layer In-Painting";

        public LayerInpaintingFilter(IPluginConfigurator<(IImage, IImage, int)> pluginConfigurator, IEngine engine)
        {
            this._pluginConfigurator = pluginConfigurator;
            this._engine = engine;
        }

        public IImage Apply(IImage input)
        {
            IColor BLACK = this._engine.CreateColor(this._engine.MinValue, this._engine.MinValue, this._engine.MinValue, this._engine.MaxValue);
            IColor WHITE = this._engine.CreateColor(this._engine.MaxValue, this._engine.MaxValue, this._engine.MaxValue, this._engine.MaxValue);

            if (!this._ready)
                throw new NotReadyException();

            IImage output = this._engine.CreateImage(input.Width, input.Height);

            Parallel.For(0, input.Width, (int x) =>
            {
                Parallel.For(0, input.Height, (int y) =>
                {
                    if (input.GetPixel(x, y).Equivalent(BLACK))
                    {
                        IEnumerable<Node> relNodes = CollectRelativeNodes(this._colorLayer, x, y, this._distance, this._distance);
                        IEnumerable<Node> filtNodes = relNodes.Where(n => !this._skipLayer.GetPixel(n.X, n.Y).Equivalent(BLACK));
                        IColor modeRelatives = Mode(filtNodes.Any() ? filtNodes : relNodes);
                        output.SetPixel(x, y, modeRelatives);
                    }
                }); 
            });

            return output;
        }

        public IFilter Initialize()
        {
            (this._colorLayer, this._skipLayer, this._distance) = this._pluginConfigurator.GetPluginConfiguration();
            this._ready = true;
            return this;
        }

        private static IEnumerable<Node> CollectRelativeNodes(IImage image, int x, int y, int dx, int dy) =>
            from ox in Enumerable.Range(-dx / 2, dx)
            from oy in Enumerable.Range(-dy / 2, dy)
            where !image.OutOfBounds(ox + x, oy + y)
            select new Node { X = x + ox, Y = y + oy, Color = image.GetPixel(ox + x, oy + y) };

        private IColor Mode(IEnumerable<Node> relNodes)
        {
            Dictionary<(int, int, int), int> cDict = new Dictionary<(int, int, int), int>();
            foreach (var relNode in relNodes)
            {
                IColor c = relNode.Color!;
                if (!cDict.ContainsKey((c.R, c.G, c.B)))
                    cDict.Add((c.R, c.G, c.B), 1);
                else
                    cDict[(c.R, c.G, c.B)]++;
            }
            (int, int, int) maxKey = cDict.MaxBy(tk => tk.Value).Key;
            return this._engine.CreateColor(maxKey.Item1, maxKey.Item2, maxKey.Item3, this._engine.MaxValue);
        }
    }
}
