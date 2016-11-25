using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Mapdoodle
{
    public class Layer
    {
        public Grid<Tile> Tiles;

        public Layer(int width, int height)
        {
            Tiles = new Grid<Tile>(width + 1, height + 1);
        }
    }

    public class MapState
    {
        readonly Grid<TerrainKind> squareKinds;
        readonly Layer[] layers;

        public int Width
        {
            get
            {
                return squareKinds.Width;
            }
        }

        public int Height
        {
            get
            {
                return squareKinds.Height;
            }
        }

        public MapState()
        {
        }

        public MapState(int width, int height)
        {
            squareKinds = new Grid<TerrainKind>(width, height);
            layers = new Layer[4];
            for (int i = 0; i < 4; i++)
                layers[i] = new Layer(width, height);
        }

        public void CopyFrom(MapState state)
        {
            Debug.Assert(state.squareKinds.Width == squareKinds.Width);
            Debug.Assert(state.squareKinds.Height == squareKinds.Height);

            for (int x = 0; x < squareKinds.Width; x++)
                for (int y = 0; y < squareKinds.Height; y++)
                    squareKinds[x, y] = state.squareKinds[x, y];

            for (int x = 0; x < squareKinds.Width + 1; x++)
                for (int y = 0; y < squareKinds.Height + 1; y++)
                    for (int layer = 0; layer < 4; layer++)
                        layers[layer].Tiles[x, y] = state.layers[layer].Tiles[x, y];
        }

        public TerrainKind GetSquareKind(int squareX, int squareY)
        {
            if (squareX < 0)
                squareX = 0;
            if (squareX >= squareKinds.Width)
                squareX = squareKinds.Width - 1;
            if (squareY < 0)
                squareY = 0;
            if (squareY >= squareKinds.Height)
                squareY = squareKinds.Height - 1;

            return squareKinds[squareX, squareY];
        }

        public void SetSquareKind(int squareX, int squareY, TerrainKind terrain)
        {
            squareKinds[squareX, squareY] = terrain;
        }

        public Tile GetTile(int layer, int tileX, int tileY)
        {
            return layers[layer].Tiles[tileX, tileY];
        }

        public void SetTile(int layer, int tileX, int tileY, Tile tile)
        {
            layers[layer].Tiles[tileX, tileY] = tile;
        }
    }

    public class Map
    {
        readonly int width;
        readonly int height;

        MapState state;
        readonly MapState stateHover;

        readonly List<MapState> undoStack = new List<MapState>();
        readonly List<MapState> redoStack = new List<MapState>();

        readonly TileSet tileSet;

        public Random Rng = new Random();

        public Map(TileSet tileSet, int width, int height)
        {
            this.tileSet = tileSet;
            this.width = width;
            this.height = height;
            state = new MapState(this.width, this.height);
            stateHover = new MapState(this.width, this.height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < height; x++)
                    state.SetSquareKind(x, y, TerrainKind.Floor);

            this.tileSet.SelectTiles(state, Rng, 0, 0, this.width + 1, this.height + 1);
            ClearHoverTerrain();
        }

        public static Map DeserializeFrom(StreamReader stream)
        {
            XmlReader xml = XmlReader.Create(stream);

            xml.ReadStartElement("Map");

            String tilesetName = xml.ReadElementString("Tileset");
            if (tilesetName != "Linedraw")
                throw new NotImplementedException();

            TileSet tileSet = TileSet.Load(@"Data\Simple.mdts");

            int width = int.Parse(xml.ReadElementString("Width"));
            int height = int.Parse(xml.ReadElementString("Height"));

            Map map = new Map(tileSet, width, height);
            MapState state = map.state;

            xml.ReadStartElement("Terrain");
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int tileId = int.Parse(xml.ReadElementString("Square"));
                    state.SetSquareKind(x, y, tileSet.TerrainByIndex[tileId]);
                }
            xml.ReadEndElement(); /* Terrain */

            xml.ReadStartElement("Tiles");
            while (xml.IsStartElement("Tile"))
            {
                xml.ReadStartElement("Tile");

                int layer = int.Parse(xml.ReadElementString("Layer"));
                int x = int.Parse(xml.ReadElementString("X"));
                int y = int.Parse(xml.ReadElementString("Y"));
                int value = int.Parse(xml.ReadElementString("Value"));

                Tile tile = tileSet.TileList[value];
                if (tile.IsLarge)
                {
                    for (int subX = 0; subX < tile.Width; subX++)
                        for (int subY = 0; subY < tile.Height; subY++)
                            state.SetTile(layer, x + subX, y + subY, tile.GetSubtile(subX, subY));
                }
                else
                    state.SetTile(layer, x, y, tile);

                xml.ReadEndElement(); /* Tile */
            }

            xml.ReadEndElement(); /* Tiles */

            xml.ReadEndElement(); /* Map */

            // TODO: Verify that map is correct
            map.ClearHoverTerrain();
            return map;
        }

        public void SerializeTo(StreamWriter stream)
        {
            TileSet tileSet = this.tileSet;
            MapState state = this.state;

            Dictionary<TerrainKind, string> terrainIds = new Dictionary<TerrainKind, string>();
            for (int i = 0; i < tileSet.TerrainByIndex.Length; i++)
                terrainIds[tileSet.TerrainByIndex[i]] = i.ToString();

            Dictionary<Tile, string> tileIds = new Dictionary<Tile, string>();
            for (int i = 0; i < tileSet.TileList.Count; i++)
                tileIds[tileSet.TileList[i]] = i.ToString();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xml = XmlWriter.Create(stream, settings);

            xml.WriteStartElement("Map");

            xml.WriteElementString("Tileset", "Linedraw");
            xml.WriteElementString("Width", Width.ToString());
            xml.WriteElementString("Height", Height.ToString());

            xml.WriteStartElement("Terrain");
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    xml.WriteElementString("Square", terrainIds[this.state.GetSquareKind(x, y)]);
            xml.WriteEndElement(); /* Terrain */

            xml.WriteStartElement("Tiles");
            for (int layer = 0; layer < 4; layer++)
                for (int y = 0; y < Height + 1; y++)
                    for (int x = 0; x < Width + 1; x++)
                    {
                        Tile tile = this.state.GetTile(layer, x, y);
                        if (tile.IsLarge && (tile.XPosition > 0 || tile.YPosition > 0))
                            continue;
                        if (tile.IsLarge)
                            tile = tile.SuperTile;

                        xml.WriteStartElement("Tile");
                        xml.WriteElementString("Layer", layer.ToString());
                        xml.WriteElementString("X", x.ToString());
                        xml.WriteElementString("Y", y.ToString());
                        xml.WriteElementString("Value", tileIds[tile]);
                        xml.WriteEndElement(); /* Tile */
                    }
            xml.WriteEndElement(); /* Tiles */

            xml.WriteEndElement(); /* Map */

            xml.Flush();
        }

        public void SaveUndo()
        {
            MapState undoState = new MapState(width, height);
            undoState.CopyFrom(state);
            undoStack.Add(undoState);
            if (undoStack.Count > 100)
                undoStack.RemoveAt(0);

            redoStack.Clear();
        }

        public bool CanUndo()
        {
            return undoStack.Count > 0;
        }

        public bool CanRedo()
        {
            return redoStack.Count > 0;
        }

        public bool Undo()
        {
            if (undoStack.Count == 0)
                return false;
            redoStack.Add(state);
            state = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            ClearHoverTerrain();
            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0)
                return false;
            undoStack.Add(state);
            state = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            ClearHoverTerrain();
            return true;
        }

        public static Grid<BorderKind> MakeTerrainSubMap(MapState mapState, int tileX, int tileY, int tileXLimit, int tileYLimit, int layer)
        {
            Grid<BorderKind> terrainMap = new Grid<BorderKind>(tileXLimit - tileX + 1, tileYLimit - tileY + 1);
            for (int y = 0; y < terrainMap.Height; y++)
                for (int x = 0; x < terrainMap.Width; x++)
                    terrainMap[x, y] = mapState.GetSquareKind(tileX + x - 1, tileY + y - 1).LayerTerrainId[layer];
            return terrainMap;
        }

        private Rectangle CalculateUpdateRectangle(MapState oldState, MapState newState)
        {
            int top = oldState.Height + 1;
            int left = oldState.Width + 1;
            int right = -1;
            int bottom = -1;

            for (int x = 0; x < oldState.Width + 1; x++)
                for (int y = 0; y < oldState.Height + 1; y++)
                    for (int layer = 0; layer < 4; layer++)
                    {
                        if (oldState.GetTile(layer, x, y) != newState.GetTile(layer, x, y))
                        {
                            if (top > y)
                                top = y;
                            if (left > x)
                                left = x;
                            if (right < x)
                                right = x;
                            if (bottom < y)
                                bottom = y;
                        }
                    }

            if (right < left)
                return new Rectangle();

            return new Rectangle(
                left * tileSet.TileWidth - tileSet.TileXOffset,
                top * tileSet.TileHeight - tileSet.TileYOffset,
                (right - left + 1) * tileSet.TileWidth,
                (bottom - top + 1) * tileSet.TileHeight);
        }

        public void SetTerrain(int squareX, int squareY, IMapBrush brush, out Rectangle updateRectangle)
        {
            updateRectangle = new Rectangle();

            if (squareX < 0 || squareX >= Width || squareY < 0 || squareY >= Width)
                return;

            MapState oldState = new MapState(stateHover.Width, state.Height);
            oldState.CopyFrom(state);

            brush.DrawAt(tileSet, state, Rng, squareX, squareY);
            updateRectangle = CalculateUpdateRectangle(oldState, state);

            ClearHoverTerrain();
        }

        public bool SetHoverTerrain(int squareX, int squareY, IMapBrush brush, out Rectangle updateRectangle)
        {
            updateRectangle = new Rectangle();

            if (squareX < 0 || squareX >= Width || squareY < 0 || squareY >= Width)
                return false;

            MapState oldState = new MapState(stateHover.Width, stateHover.Height);
            oldState.CopyFrom(stateHover);

            if (!brush.NeedsDraw(state, squareX, squareY))
            {
                ClearHoverTerrain();
                updateRectangle = CalculateUpdateRectangle(oldState, stateHover);
                return true;
            }

            if (!brush.NeedsDraw(stateHover, squareX, squareY))
                return false;

            ClearHoverTerrain();
            brush.DrawAt(tileSet, stateHover, null /* rng */, squareX, squareY);
            updateRectangle = CalculateUpdateRectangle(oldState, stateHover);
            return true;
        }

        public void ClearHoverTerrain()
        {
            stateHover.CopyFrom(state);
        }

        public void DrawToGraphics(Graphics target, Rectangle clipRectangle)
        {
            Debug.Assert(clipRectangle.Width > 0);
            Debug.Assert(clipRectangle.Height > 0);

            int tileWidth = tileSet.TileWidth;
            int tileHeight = tileSet.TileHeight;
            int offsetX = tileSet.TileXOffset;
            int offsetY = tileSet.TileYOffset;

            for (int y = height; y >= 0; y--)
            {
                for (int x = width; x >= 0; x--)
                {
                    int destX = tileWidth * x - offsetX;
                    int destY = tileHeight * y - offsetY;

                    if (destX > clipRectangle.Right)
                        continue;
                    if (destX + tileWidth < clipRectangle.Left)
                        continue;
                    if (destY > clipRectangle.Bottom)
                        continue;
                    if (destY + tileHeight < clipRectangle.Top)
                        continue;

                    for (int layer = 0; layer < 4; layer++)
                    {
                        Tile tile = state.GetTile(layer, x, y);
                        if (tile != null)
                            tile.DrawToGraphics(target, destX, destY);
                        else
                        {
#if false
                            PaintSynthetic(target, layer, x, y, destX, destY);
#endif
                        }
                    }
                }
            }

            // Paint hover
            for (int y = height; y >= 0; y--)
            {
                for (int x = width; x >= 0; x--)
                {
                    bool needDraw = false;
                    for (int layer = 0; layer < 4; layer++)
                        if (state.GetTile(layer, x, y) != stateHover.GetTile(layer, x, y))
                            needDraw = true;

                    if (!needDraw)
                        continue;

                    int destX = tileWidth * x - offsetX;
                    int destY = tileHeight * y - offsetY;

                    if (destX >= clipRectangle.Right)
                        continue;
                    if (destX + tileWidth <= clipRectangle.Left)
                        continue;
                    if (destY >= clipRectangle.Bottom)
                        continue;
                    if (destY + tileHeight <= clipRectangle.Top)
                        continue;

                    Bitmap hoverBitmap = new Bitmap(tileSet.TileWidth, tileSet.TileHeight);
                    Graphics hoverGraphics = Graphics.FromImage(hoverBitmap);
                    for (int layer = 0; layer < 4; layer++)
                    {
                        Tile tile = stateHover.GetTile(layer, x, y);
                        if (tile != null)
                            tile.DrawToGraphics(hoverGraphics, 0, 0);
                        else
                        {
#if false
                            PaintSynthetic(hoverGraphics, layer, x, y, 0, 0);
#endif
                        }
                    }
                    for (int px = 0; px < hoverBitmap.Width; px++)
                        for (int py = 0; py < hoverBitmap.Height; py++)
                        {
                            Color c = hoverBitmap.GetPixel(px, py);
                            hoverBitmap.SetPixel(px, py, Color.FromArgb((int)(c.A * 0.67), c.R, c.G, c.B));
                        }
                    target.DrawImage(hoverBitmap, destX, destY);
                }
            }
        }

#if true
        private void PaintSynthetic(Graphics target, int layer, int x, int y, int destX, int destY)
        {
            // No tile. Synthesize one
            for (int subX = 0; subX <= 1; subX++)
            {
                for (int subY = 0; subY <= 1; subY++)
                {
                    TerrainKind squareKind = state.GetSquareKind(x + subX - 1, y + subY - 1);
                    Tile defaultTile = tileSet.GetDefaultTile(squareKind, layer);
                    int sourceX, sourceY, sourceWidth, sourceHeight;
                    if (subX == 0)
                    {
                        sourceX = 0;
                        sourceWidth = tileSet.TileXOffset;
                    }
                    else
                    {
                        sourceX = tileSet.TileXOffset;
                        sourceWidth = tileSet.TileWidth - tileSet.TileXOffset;
                    }
                    if (subY == 0)
                    {
                        sourceY = 0;
                        sourceHeight = tileSet.TileYOffset;
                    }
                    else
                    {
                        sourceY = tileSet.TileYOffset;
                        sourceHeight = tileSet.TileHeight - tileSet.TileYOffset;
                    }
                    Rectangle rect = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);
                    if (defaultTile != null)
                        defaultTile.DrawToGraphics(target, destX, destY, rect);
                }
            }
            target.DrawRectangle(new Pen(Color.FromArgb(255, 255, 0, 0)),
                new Rectangle(destX, destY, tileSet.TileWidth - 1, tileSet.TileWidth - 1));
        }
#endif

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public TileSet TileSet { get { return tileSet; } }
    }
}
