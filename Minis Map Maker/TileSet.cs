using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Xml.Serialization;
using System.IO.Packaging;
using System.IO;
using System.Windows.Forms;

namespace Mapdoodle
{
    public class SpecialTileDefinition
    {
        [XmlElement(ElementName = "AutoPlace")]
        public bool AutoPlace;
        [XmlElement(ElementName = "Left")]
        public int Left;
        [XmlElement(ElementName = "Top")]
        public int Top;
        [XmlElement(ElementName = "Width")]
        public int Width;
        [XmlElement(ElementName = "Height")]
        public int Height;
        public SpecialTileDefinition()
        {
        }
        public SpecialTileDefinition(bool autoPlace, int left, int top, int width, int height)
        {
            AutoPlace = autoPlace;
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }

    public class TileSourceDefinition
    {
        [XmlElement(ElementName = "Filename")]
        public string Filename;
        [XmlElement(ElementName = "Layer")]
        public int Layer;
        [XmlElement(ElementName = "TileWidth")]
        public int TileWidth;
        [XmlElement(ElementName = "TileHeight")]
        public int TileHeight;
        [XmlElement(ElementName = "MapWidth")]
        public int MapWidth;
        [XmlElement(ElementName = "MapHeight")]
        public int MapHeight;
        [XmlElement(ElementName = "ImageWidth")]
        public int ImageWidth;
        [XmlElement(ElementName = "ImageHeight")]
        public int ImageHeight;
        [XmlElement(ElementName = "ImageLeft")]
        public int ImageLeft;
        [XmlElement(ElementName = "ImageTop")]
        public int ImageTop;
        [XmlArray(ElementName = "BorderKinds")]
        public string[] BorderKinds;
        [XmlArray(ElementName = "Squares")]
        public int[] Squares;
        [XmlArray(ElementName = "SpecialTiles")]
        public SpecialTileDefinition[] SpecialTiles;

        public TileSourceDefinition()
        {
        }

        public TileSourceDefinition(string filename, int layer, int tileWidth, int tileHeight, int mapWidth, int mapHeight,
            int imageWidth, int imageHeight, int imageLeft, int imageTop, 
            string[] borderKinds, int[] squares, SpecialTileDefinition[] specialTiles)
        {
            Filename = filename;
            Layer = layer;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            ImageTop = imageTop;
            ImageLeft = imageLeft;
            BorderKinds = borderKinds;
            Squares = squares;
            SpecialTiles = specialTiles;
        }
    }

    public class TileSet
    {
        public int TileWidth { get { return 50; } }
        public int TileHeight { get { return 50; } }
        public int TileXOffset { get { return 25; } }
        public int TileYOffset { get { return 25; } }

        public TerrainKind[] TerrainByIndex = new TerrainKind[] {
            TerrainKind.Floor,
            TerrainKind.Wall,
            TerrainKind.Wall2,
            TerrainKind.Rubble,
            TerrainKind.Water,
        };

        internal List<Tile> TileList = new List<Tile>();

        public static TileSet Load(string filename)
        {
            TileSet tileSet = new TileSet();
            TileSourceDefinition[] tileSources;

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(TileSourceDefinition[]));

            if (Directory.Exists(filename))
            {
                tileSources = tileSet.LoadTilesetFromPackage(filename, serializer);
            }
            else
            {
                string packageDirectory = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                tileSources = tileSet.LoadTilesetFromDirectory(packageDirectory, serializer);
                // HACK Create a new package...
                SaveTileset(packageDirectory + ".mdts", packageDirectory, tileSources, serializer);
            }

            return tileSet;
        }

        private TileSourceDefinition[] LoadTilesetFromPackage(string filename, System.Xml.Serialization.XmlSerializer serializer)
        {
            TileSourceDefinition[] tileSources;
            Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Tileset.xml", UriKind.Relative));
            using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read))
            {
                PackagePart tilesetDocument = package.GetPart(partUriMap);
                using (Stream stream = tilesetDocument.GetStream())
                {
                    tileSources = (TileSourceDefinition[])serializer.Deserialize(stream);
                }

                foreach (TileSourceDefinition source in tileSources)
                {
                    Uri partUriImage = PackUriHelper.CreatePartUri(new Uri(source.Filename, UriKind.Relative));
                    PackagePart imageDocument = package.GetPart(partUriImage);
                    using (Stream imageStream = imageDocument.GetStream())
                    {
                        AddTileSource(source, imageStream);
                    }
                }
            }
            return tileSources;
        }

        private TileSourceDefinition[] LoadTilesetFromDirectory(string filename, System.Xml.Serialization.XmlSerializer serializer)
        {
            TileSourceDefinition[] tileSources;
            using (Stream stream = new FileStream(filename + @"\Tileset.xml", FileMode.Open, FileAccess.Read))
            {
                tileSources = (TileSourceDefinition[])serializer.Deserialize(stream);
            }

            foreach (TileSourceDefinition source in tileSources)
            {
                using (Stream imageStream = new FileStream(filename + @"\" + source.Filename, FileMode.Open, FileAccess.Read))
                {
                    AddTileSource(source, imageStream);
                }
            }

            return tileSources;
        }

        private static void SaveTileset(string filename, string directoryName, TileSourceDefinition[] tileSources, System.Xml.Serialization.XmlSerializer serializer)
        {
            Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Tileset.xml", UriKind.Relative));
            using (Package package = Package.Open(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                PackagePart tilesetDocument = package.CreatePart(partUriMap, System.Net.Mime.MediaTypeNames.Text.Xml,
                    CompressionOption.Maximum);

                using (Stream stream = tilesetDocument.GetStream())
                {
                    serializer.Serialize(stream, tileSources);
                }

                foreach (TileSourceDefinition source in tileSources)
                {
                    Uri partUriImage = PackUriHelper.CreatePartUri(new Uri(source.Filename, UriKind.Relative));
                    PackagePart imageDocument = package.CreatePart(partUriImage, "image/png", CompressionOption.NotCompressed);
                    using (Stream imageInStream = new FileStream(directoryName + @"\" + source.Filename, FileMode.Open, FileAccess.Read))
                    {
                        using (Stream imageOutStream = imageDocument.GetStream())
                        {
                            int bytesRead;
                            byte[] buffer = new byte[4096];
                            do
                            {
                                bytesRead = imageInStream.Read(buffer, 0, 4096);
                                if (bytesRead > 0)
                                    imageOutStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead > 0);
                        }
                    }
                }
            }
        }

        private BorderKind FindBorderKind(string name)
        {
            switch (name)
            {
                case "null":
                    return BorderKind.Null;
                case "floor":
                    return BorderKind.Floor;
                case "wall":
                    return BorderKind.Wall;
                case "wall2":
                    return BorderKind.Wall2;
                case "rubble":
                    return BorderKind.Rubble;
                case "water":
                    return BorderKind.Water;
                default:
                    Debug.Fail("Border kind not found");
                    return null;
            }
        }

        private void AddTileSource(TileSourceDefinition source, Stream stream)
        {
            Bitmap image = new Bitmap(stream);

            Grid<bool> useSquares = new Grid<bool>(source.MapWidth, source.MapHeight);
            for (int x = 0; x < source.MapWidth; x++)
                for (int y = 0; y < source.MapHeight; y++)
                    useSquares[x, y] = true;

            BorderKind[] borderKinds = new BorderKind[source.BorderKinds.Count()];
            for (int i = 0; i < source.BorderKinds.Count(); i++)
                borderKinds[i] = FindBorderKind(source.BorderKinds[i]);            

            Grid<BorderKind> squares = new Grid<BorderKind>(source.MapWidth, source.MapHeight);
            for (int x = 0; x < source.MapWidth; x++)
                for (int y = 0; y < source.MapHeight; y++)
                    squares[x, y] = borderKinds[source.Squares[x + y * source.MapWidth]];

            int baseX = TileXOffset - source.ImageLeft;
            int baseY = TileYOffset - source.ImageTop;

            // Add special tiles
            foreach (SpecialTileDefinition special in source.SpecialTiles)
            {
                int left = special.Left;
                int top = special.Top;
                int width = special.Width;
                int height = special.Height;
                Tile tile = CreateTileFromTileSource(image, squares, baseX, baseY, left, top, width, height);  

                // Add it?
                if (special.AutoPlace)
                    TileList.Add(tile);

                // Squares inside the special tile are not normal squares
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        useSquares[left + x, top + y] = false;
            }

            // Add normal tiles, except those excluded as part of special tiles.
            for (int x = 0; x < source.MapWidth - 1; x++)
                for (int y = 0; y < source.MapHeight - 1; y++) 
                {
                    if (useSquares[x, y]) 
                    {
                        Tile tile = CreateTileFromTileSource(image, squares, baseX, baseY, x, y, 1, 1);
                        TileList.Add(tile);
                    }
                }
        }

        private Tile CreateTileFromTileSource(Bitmap image, Grid<BorderKind> squares, int baseX, int baseY, int left, int top, int width, int height)
        {
            // Extract the correct subgrid as an array
            BorderKind[] tileBorders = new BorderKind[(width + 1) * (height + 1)];
            for (int x = 0; x <= width; x++)
                for (int y = 0; y <= height; y++)
                    tileBorders[x + y * (width + 1)] = squares[left + x, top + y];

            // Create a tile
            Tile tile = new Tile(image, baseX + TileWidth * left, baseY + TileHeight * top,
                TileWidth * width, TileHeight * height, width, height, tileBorders);
            return tile;
        }

        public Tile GetTile(Random rng, int tileX, int tileY, Grid<BorderKind> terrainId, bool large, Tile preferredTile)
        {
            List<Tile>[] candidates = new List<Tile>[5];
            for (int i = 0; i <= 4; i++)
                candidates[i] = new List<Tile>();
            foreach (Tile t in TileList)
            {
                if (t.IsLarge != large)
                    continue;

                int matches;
                if (CanPlaceTile(t, terrainId, tileX, tileY, out matches))
                    candidates[matches * 4 / ((t.Height + 1) * (t.Width + 1))].Add(t);
            }
            for (int i = 4; i >= 0; i--)
            {
                if (candidates[i].Count > 0)
                {
                    if (rng == null)
                    {
                        foreach (Tile t in candidates[i])
                            if (t == preferredTile)
                                return t;
                        return candidates[i][0];
                    }
                    return candidates[i][rng.Next(candidates[i].Count)];
                }
            }
            return null;
        }

        private static bool CanPlaceTile(Tile t, Grid<BorderKind> terrainId, int tileX, int tileY, out int matches)
        {
            matches = 0;
            for (int x = 0; x <= t.Width; x++)
                for (int y = 0; y <= t.Height; y++)
                {
                    if (tileX + x >= terrainId.Width ||
                        tileY + y >= terrainId.Height)
                        return false;

                    BorderKind terrain1 = t.TerrainId[x, y];
                    BorderKind terrain2 = terrainId[tileX + x, tileY + y];
                    if (!terrain1.IsCompatible(terrain2))
                        return false;
                    if (terrain1 == terrain2)
                        matches++;
                    else if (t.IsLarge)
                        return false;
                }
            return true;
        }

        public Tile GetDefaultTile(TerrainKind terrain, int layer)
        {
            BorderKind terrainId = terrain.LayerTerrainId[layer];
            Grid<BorderKind> g = new Grid<BorderKind>(2, 2);
            g[0, 0] = g[0, 1] = g[1, 0] = g[1, 1] = terrainId;

            return GetTile(null, 0, 0, g, false, null);
        }

        private void SelectTilesBase(MapState mapState, Random rng, int tileX, int tileY, int tileXLimit, int tileYLimit)
        {
#if DEBUG
            Console.Out.WriteLine("SelectTilesBase ({0},{1})-({2},{3})", tileX, tileY, tileXLimit, tileYLimit);
#endif
            for (int layer = 0; layer < 4; layer++)
            {
                Grid<BorderKind> terrainMap = Map.MakeTerrainSubMap(mapState, tileX, tileY, tileXLimit, tileYLimit, layer);
                for (int y = tileYLimit - 1; y >= tileY; y--)
                    for (int x = tileXLimit - 1; x >= tileX; x--)
                    {
                        Tile t = GetTile(rng, x - tileX, y - tileY, terrainMap, false, mapState.GetTile(layer, x, y));
                        mapState.SetTile(layer, x, y, t);
                    }
            }
        }

        private void ClearLargeTiles(MapState mapState, Random rng, int tileX, int tileY, int tileXLimit, int tileYLimit)
        {
#if DEBUG
            Console.Out.WriteLine("ClearLargeTiles ({0},{1})-({2},{3})", tileX, tileY, tileXLimit, tileYLimit);
#endif
            for (int layer = 0; layer < 4; layer++)
            {
                for (int y = tileYLimit - 1; y >= tileY; y--)
                    for (int x = tileXLimit - 1; x >= tileX; x--)
                    {
                        Tile t = mapState.GetTile(layer, x, y);
                        if (t != null && t.IsLarge)
                        {
                            int baseX = x - t.XPosition;
                            int baseY = y - t.YPosition;
                            SelectTilesBase(mapState, rng, baseX, baseY, baseX + t.Width, baseY + t.Height);
                        }
                    }
            }
        }

        private void SelectLargeTiles(MapState mapState, Random rng, int tileX, int tileY, int tileXLimit, int tileYLimit)
        {
            if (rng == null)
                return;

            int width = mapState.Width;
            int height = mapState.Height;

            if (tileX < 0)
                tileX = 0;
            if (tileY < 0)
                tileY = 0;
            if (tileXLimit >= width)
                tileXLimit = width - 1;
            if (tileYLimit >= height)
                tileYLimit = height - 1;

            for (int layer = 0; layer < 4; layer++)
            {
                Grid<BorderKind> terrainId = new Grid<BorderKind>(width + 2, height + 2);
                for (int y = 0; y < terrainId.Height; y++)
                    for (int x = 0; x < terrainId.Width; x++)
                        terrainId[x, y] = mapState.GetSquareKind(x - 1, y - 1).LayerTerrainId[layer];
                for (int x = 0; x < tileXLimit; x++)
                    for (int y = 0; y < tileYLimit; y++)
                    {
                        if (rng.NextDouble() < 0.5)
                            continue;

                        Tile oldTile = mapState.GetTile(layer, x, y);
                        if (oldTile == null || oldTile.IsLarge)
                            continue;

                        Tile t = GetTile(rng, x, y, terrainId, true, null);
                        if (t != null)
                        {
                            if (x + t.Width - 1 < tileX)
                                continue;
                            if (y + t.Height - 1 < tileY)
                                continue;

                            if (CanPlaceLargeTile(mapState, t, layer, x, y))
                            {
                                PlaceLargeTile(mapState, t, layer, x, y);
                            }
                        }
                    }
            }
        }

        private static bool CanPlaceLargeTile(MapState mapState, Tile t, int layer, int x, int y)
        {
            for (int subX = 0; subX < t.Width; subX++)
                for (int subY = 0; subY < t.Height; subY++)
                    if (mapState.GetTile(layer, x + subX, y + subY).IsLarge)
                        return false;
            return true;
        }

        private static void PlaceLargeTile(MapState mapState, Tile tile, int layer, int x, int y)
        {
            for (int subX = 0; subX < tile.Width; subX++)
                for (int subY = 0; subY < tile.Height; subY++)
                {
                    mapState.SetTile(layer, x + subX, y + subY, tile.GetSubtile(subX, subY));
                }
        }

        public void SelectTiles(MapState mapState, Random rng, int tileX, int tileY, int tileXLimit, int tileYLimit)
        {
#if DEBUG
            Console.Out.WriteLine("SelectTiles ({0},{1})-({2},{3})", tileX, tileY, tileXLimit, tileYLimit);
#endif
            ClearLargeTiles(mapState, rng, tileX, tileY, tileXLimit, tileYLimit);
            SelectTilesBase(mapState, rng, tileX, tileY, tileXLimit, tileYLimit);
            SelectLargeTiles(mapState, rng, tileX, tileY, tileXLimit, tileYLimit);
        }

    }


    public class Tile
    {
        readonly Bitmap bitmap;
        public Grid<BorderKind> TerrainId;
        readonly int baseX;
        readonly int baseY;
        readonly int imageWidth;
        readonly int imageHeight;
        int width, height;
        public int XPosition, YPosition;
        public Tile SuperTile = null;
        readonly Bitmap cacheBitmap;
        public Tile(Bitmap bitmap, int baseX, int baseY, int imageWidth, int imageHeight, int width, int height, params BorderKind[] terrainId)
        {
            this.bitmap = bitmap;
            this.baseX = baseX;
            this.baseY = baseY;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.width = width;
            this.height = height;
            if (terrainId != null)
            {
                TerrainId = new Grid<BorderKind>(this.width + 1, this.height + 1);
                for (int y = 0; y <= this.height; y++)
                    for (int x = 0; x <= this.width; x++)
                        TerrainId[x, y] = terrainId[x + y * (this.width + 1)];
            }

            cacheBitmap = new Bitmap(imageWidth, imageHeight);
            Graphics g = Graphics.FromImage(cacheBitmap);
            g.DrawImage(this.bitmap,
                new Rectangle(0, 0, this.imageWidth, this.imageHeight),
                new Rectangle(this.baseX, this.baseY, this.imageWidth, this.imageHeight),
                GraphicsUnit.Pixel);
        }
        public Tile GetSubtile(int xPosition, int yPosition)
        {
            if (Width == 1 && Height == 1)
                return this;

            Tile subtile = new Tile(Bitmap, 
                baseX + (imageWidth / width) * xPosition, baseY + (imageHeight / height) * yPosition,
                (imageWidth / width), (imageHeight / height), 1, 1, null);
            subtile.XPosition = xPosition;
            subtile.YPosition = yPosition;
            subtile.width = width;
            subtile.height = height;
            subtile.SuperTile = this;
            return subtile;
        }
        public Bitmap Bitmap { get { return bitmap; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public bool IsLarge { get { return (width > 1 || height > 1); } }

        public void DrawToGraphics(Graphics target, int destX, int destY)
        {
            target.DrawImage(cacheBitmap,
                new Rectangle(destX, destY, imageWidth, imageHeight),
                new Rectangle(0, 0, imageWidth, imageHeight),
                GraphicsUnit.Pixel);

#if DEBUGTILES
            if (IsLarge)
                target.DrawRectangle(new Pen(Color.FromArgb(128, 255, 0, 0)),
                    new Rectangle(destX, destY, _imageWidth, _imageHeight));
#endif
        }

        public void DrawToGraphics(Graphics target, int destX, int destY, Rectangle rect)
        {
            target.DrawImage(cacheBitmap,
                new Rectangle(destX + rect.X, destY + rect.Y, rect.Width, rect.Height),
                new Rectangle(0 + rect.X, 0 + rect.Y, rect.Width, rect.Height),
                GraphicsUnit.Pixel);
        }

    }

    public class BorderKind
    {
        readonly string name;
        readonly List<BorderKind> compatible;

        public bool IsCompatible(BorderKind kind)
        {
            if (kind == this)
                return true;
            foreach (BorderKind test in compatible)
            {
                if (test == kind)
                    return true;
            }
            return false;
        }

        public static void MakeCompatible(BorderKind kind1, BorderKind kind2)
        {
            kind1.compatible.Add(kind2);
            kind2.compatible.Add(kind1);
        }

        BorderKind(string name)
        {
            this.name = name;
            compatible = new List<BorderKind>();
        }

        public static BorderKind Null = new BorderKind("null");
        public static BorderKind Floor = new BorderKind("floor");
        public static BorderKind Wall = new BorderKind("wall");
        public static BorderKind Wall2 = new BorderKind("wall2");
        public static BorderKind Rubble = new BorderKind("rubble");
        public static BorderKind Water = new BorderKind("water");

        public override string ToString()
        {
            return name;
        }
    }

    public class TerrainKind
    {
        public string Name;
        public BorderKind[] LayerTerrainId;

        public TerrainKind(string name, params BorderKind[] layerTerrainId)
        {
            Name = name;
            LayerTerrainId = layerTerrainId;
        }

        public override string ToString()
        {
            return Name;
        }

        public static TerrainKind Floor = new TerrainKind("Floor", BorderKind.Floor, BorderKind.Null, BorderKind.Null, BorderKind.Null);
        public static TerrainKind Wall = new TerrainKind("Rough Wall", BorderKind.Floor, BorderKind.Null, BorderKind.Wall, BorderKind.Null);
        public static TerrainKind Wall2 = new TerrainKind("Straight Wall", BorderKind.Floor, BorderKind.Null, BorderKind.Wall2, BorderKind.Null);
        public static TerrainKind Rubble = new TerrainKind("Rubble", BorderKind.Floor, BorderKind.Rubble, BorderKind.Null, BorderKind.Null);
        public static TerrainKind Water = new TerrainKind("Water", BorderKind.Floor, BorderKind.Water, BorderKind.Null, BorderKind.Null);
    }

}