using FilterDotNet.Interfaces;
using System.Drawing;

// Initial test junk to ensure dependencies work.
FastImageLibrary.FastImage myImage = new FastImageLibrary.FastImage(100, 100);
IEngine engine = FastImageProvider.Injectables.FiEngine;
Bitmap b = myImage.ToBitmap();