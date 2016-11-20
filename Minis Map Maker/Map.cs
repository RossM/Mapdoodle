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
    public class Grid<T>
    {
        int _width;
        int _height;
        T[] _squares;

        public Grid()
        {
        }

        public Grid(int width, int height)
        {
            _width = width;
            _height = height;
            _squares = new T[width * height];
        }

        public T this[int x, int y]
        {
            [DebuggerStepThrough]
            get
            {
                Debug.Assert(x >= 0 && x < _width);
                Debug.Assert(y >= 0 && y < _height);
                return _squares[x + _width * y];
            }
            [DebuggerStepThrough]
            set
            {
                Debug.Assert(x >= 0 && x < _width);
                Debug.Assert(y >= 0 && y < _height);
                _squares[x + _width * y] = value;
            }
        }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
    }


    public class TerrainBrush : Mapdoodle.IMapBrush
    {
        TerrainKind _terrain;

        public TerrainBrush(TerrainKind terrain)
        {
            _terrain = terrain;
        }

        private static void SetTerrainBase(MapState mapState, int squareX, int squareY, TerrainKind terrain)
        {
            mapState.SetSquareKind(squareX, squareY, terrain);
        }

        private static bool HasNullTile(MapState mapState, int tileX, int tileY)
        {
            for (int layer = 0; layer < 4; layer++)
                if (mapState.GetTile(layer, tileX, tileY) == null)
                    return true;
            return false;
        }

        private static bool IncompatibleTerrain(TerrainKind terrain1, TerrainKind terrain2)
        {
            for (int layer = 0; layer < 4; layer++)
            {
                if (terrain1._layerTerrainId[layer] != BorderKind.Null &&
                    terrain2._layerTerrainId[layer] != BorderKind.Null &&
                    terrain1._layerTerrainId[layer] != terrain2._layerTerrainId[layer])
                    return true;
            }
            return false;
        }

        public bool NeedsDraw(MapState state, int squareX, int squareY)
        {
            return (state.GetSquareKind(squareX, squareY) != _terrain);
        }

        public void DrawAt(TileSet tileSet, MapState mapState, Random rng, int squareX, int squareY)
        {
            bool needReSelect;

            needReSelect = false;

            int tileXmin = squareX;
            int tileXmax = squareX + 2;
            int tileYmin = squareY;
            int tileYmax = squareY + 2;

            SetTerrainBase(mapState, squareX, squareY, _terrain);
            tileSet.SelectTiles(mapState, rng, tileXmin, tileYmin, tileXmax, tileYmax);

            for (int x = 0; x <= 1; x++)
                for (int y = 0; y <= 1; y++)
                {
                    if (HasNullTile(mapState, squareX + x, squareY + y))
                    {
                        for (int x2 = 0; x2 <= 1; x2++)
                            for (int y2 = 0; y2 <= 1; y2++)
                            {
                                if (x != x2 || y != y2)
                                {
                                    if (IncompatibleTerrain(mapState.GetSquareKind(squareX + x - x2, squareY + y - y2), _terrain))
                                    {
                                        SetTerrainBase(mapState, squareX + x - x2, squareY + y - y2, tileSet.TerrainByIndex[0]);
                                        if (x - x2 < 0)
                                            tileXmin = squareX - 1;
                                        if (x - x2 > 0)
                                            tileXmax = squareX + 3;
                                        if (y - y2 < 0)
                                            tileYmin = squareY - 1;
                                        if (y - y2 > 0)
                                            tileYmax = squareY + 3;
                                    }
                                }
                            }
                        needReSelect = true;
                    }
                }
            if (needReSelect)
            {
                SetTerrainBase(mapState, squareX, squareY, _terrain);
                tileSet.SelectTiles(mapState, rng, tileXmin, tileYmin, tileXmax, tileYmax);
            }
        }
    }

    public class Layer
    {
        public Grid<Tile> _tiles;

        public Layer(int width, int height)
        {
            _tiles = new Grid<Tile>(width + 1, height + 1);
        }
    }

    public class MapState
    {
        Grid<TerrainKind> _squareKinds;
        Layer[] _layers;

        public int Width
        {
            get
            {
                return _squareKinds.Width;
            }
        }

        public int Height
        {
            get
            {
                return _squareKinds.Height;
            }
        }

        public MapState()
        {
        }

        public MapState(int width, int height)
        {
            _squareKinds = new Grid<TerrainKind>(width, height);
            _layers = new Layer[4];
            for (int i = 0; i < 4; i++)
                _layers[i] = new Layer(width, height);
        }

        public void CopyFrom(MapState state)
        {
            Debug.Assert(state._squareKinds.Width == _squareKinds.Width);
            Debug.Assert(state._squareKinds.Height == _squareKinds.Height);

            for (int x = 0; x < _squareKinds.Width; x++)
                for (int y = 0; y < _squareKinds.Height; y++)
                    _squareKinds[x, y] = state._squareKinds[x, y];

            for (int x = 0; x < _squareKinds.Width + 1; x++)
                for (int y = 0; y < _squareKinds.Height + 1; y++)
                    for (int layer = 0; layer < 4; layer++)
                        _layers[layer]._tiles[x, y] = state._layers[layer]._tiles[x, y];
        }

        public TerrainKind GetSquareKind(int squareX, int squareY)
        {
            if (squareX < 0)
                squareX = 0;
            if (squareX >= _squareKinds.Width)
                squareX = _squareKinds.Width - 1;
            if (squareY < 0)
                squareY = 0;
            if (squareY >= _squareKinds.Height)
                squareY = _squareKinds.Height - 1;

            return _squareKinds[squareX, squareY];
        }

        public void SetSquareKind(int squareX, int squareY, TerrainKind terrain)
        {
            _squareKinds[squareX, squareY] = terrain;
        }

        public Tile GetTile(int layer, int tileX, int tileY)
        {
            return _layers[layer]._tiles[tileX, tileY];
        }

        public void SetTile(int layer, int tileX, int tileY, Tile tile)
        {
            _layers[layer]._tiles[tileX, tileY] = tile;
        }
    }

    public class Map
    {
        int _width;
        int _height;

        MapState _state;
        MapState _stateHover;

        List<MapState> _undoStack = new List<MapState>();
        List<MapState> _redoStack = new List<MapState>();

        TileSet _tileSet;

        public Random _rng = new Random();

        public Map(TileSet tileSet, int width, int height)
        {
            _tileSet = tileSet;
            _width = width;
            _height = height;
            _state = new MapState(_width, _height);
            _stateHover = new MapState(_width, _height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < height; x++)
                    _state.SetSquareKind(x, y, TerrainKind.Floor);

            _tileSet.SelectTiles(_state, _rng, 0, 0, _width + 1, _height + 1);
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
            MapState state = map._state;

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

                Tile tile = tileSet._tileList[value];
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
            TileSet tileSet = _tileSet;
            MapState state = _state;

            Dictionary<TerrainKind, string> terrainIds = new Dictionary<TerrainKind, string>();
            for (int i = 0; i < tileSet.TerrainByIndex.Length; i++)
                terrainIds[tileSet.TerrainByIndex[i]] = i.ToString();

            Dictionary<Tile, string> tileIds = new Dictionary<Tile, string>();
            for (int i = 0; i < tileSet._tileList.Count; i++)
                tileIds[tileSet._tileList[i]] = i.ToString();

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
                    xml.WriteElementString("Square", terrainIds[_state.GetSquareKind(x, y)]);
            xml.WriteEndElement(); /* Terrain */

            xml.WriteStartElement("Tiles");
            for (int layer = 0; layer < 4; layer++)
                for (int y = 0; y < Height + 1; y++)
                    for (int x = 0; x < Width + 1; x++)
                    {
                        Tile tile = _state.GetTile(layer, x, y);
                        if (tile.IsLarge && (tile._xPosition > 0 || tile._yPosition > 0))
                            continue;
                        if (tile.IsLarge)
                            tile = tile._superTile;

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
            MapState undoState = new MapState(_width, _height);
            undoState.CopyFrom(_state);
            _undoStack.Add(undoState);
            if (_undoStack.Count > 100)
                _undoStack.RemoveAt(0);

            _redoStack.Clear();
        }

        public bool CanUndo()
        {
            return _undoStack.Count > 0;
        }

        public bool CanRedo()
        {
            return _redoStack.Count > 0;
        }

        public bool Undo()
        {
            if (_undoStack.Count == 0)
                return false;
            _redoStack.Add(_state);
            _state = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            ClearHoverTerrain();
            return true;
        }

        public bool Redo()
        {
            if (_redoStack.Count == 0)
                return false;
            _undoStack.Add(_state);
            _state = _redoStack[_redoStack.Count - 1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            ClearHoverTerrain();
            return true;
        }

        public static Grid<BorderKind> MakeTerrainSubMap(MapState mapState, int tileX, int tileY, int tileXLimit, int tileYLimit, int layer)
        {
            Grid<BorderKind> terrainMap = new Grid<BorderKind>(tileXLimit - tileX + 1, tileYLimit - tileY + 1);
            for (int y = 0; y < terrainMap.Height; y++)
                for (int x = 0; x < terrainMap.Width; x++)
                    terrainMap[x, y] = mapState.GetSquareKind(tileX + x - 1, tileY + y - 1)._layerTerrainId[layer];
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
                left * _tileSet.TileWidth - _tileSet.TileXOffset,
                top * _tileSet.TileHeight - _tileSet.TileYOffset,
                (right - left + 1) * _tileSet.TileWidth,
                (bottom - top + 1) * _tileSet.TileHeight);
        }

        public void SetTerrain(int squareX, int squareY, IMapBrush brush, out Rectangle updateRectangle)
        {
            updateRectangle = new Rectangle();

            if (squareX < 0 || squareX >= Width || squareY < 0 || squareY >= Width)
                return;

            MapState oldState = new MapState(_stateHover.Width, _state.Height);
            oldState.CopyFrom(_state);

            brush.DrawAt(_tileSet, _state, _rng, squareX, squareY);
            updateRectangle = CalculateUpdateRectangle(oldState, _state);

            ClearHoverTerrain();
        }

        public bool SetHoverTerrain(int squareX, int squareY, IMapBrush brush, out Rectangle updateRectangle)
        {
            updateRectangle = new Rectangle();

            if (squareX < 0 || squareX >= Width || squareY < 0 || squareY >= Width)
                return false;

            MapState oldState = new MapState(_stateHover.Width, _stateHover.Height);
            oldState.CopyFrom(_stateHover);

            if (!brush.NeedsDraw(_state, squareX, squareY))
            {
                ClearHoverTerrain();
                updateRectangle = CalculateUpdateRectangle(oldState, _stateHover);
                return true;
            }

            if (!brush.NeedsDraw(_stateHover, squareX, squareY))
                return false;

            ClearHoverTerrain();
            brush.DrawAt(_tileSet, _stateHover, null /* rng */, squareX, squareY);
            updateRectangle = CalculateUpdateRectangle(oldState, _stateHover);
            return true;
        }

        public void ClearHoverTerrain()
        {
            _stateHover.CopyFrom(_state);
        }

        public void DrawToGraphics(Graphics target, Rectangle clipRectangle)
        {
            Debug.Assert(clipRectangle.Width > 0);
            Debug.Assert(clipRectangle.Height > 0);

            int tileWidth = _tileSet.TileWidth;
            int tileHeight = _tileSet.TileHeight;
            int offsetX = _tileSet.TileXOffset;
            int offsetY = _tileSet.TileYOffset;

            for (int y = _height; y >= 0; y--)
            {
                for (int x = _width; x >= 0; x--)
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
                        Tile tile = _state.GetTile(layer, x, y);
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
            for (int y = _height; y >= 0; y--)
            {
                for (int x = _width; x >= 0; x--)
                {
                    bool needDraw = false;
                    for (int layer = 0; layer < 4; layer++)
                        if (_state.GetTile(layer, x, y) != _stateHover.GetTile(layer, x, y))
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

                    Bitmap hoverBitmap = new Bitmap(_tileSet.TileWidth, _tileSet.TileHeight);
                    Graphics hoverGraphics = Graphics.FromImage(hoverBitmap);
                    for (int layer = 0; layer < 4; layer++)
                    {
                        Tile tile = _stateHover.GetTile(layer, x, y);
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
                    TerrainKind squareKind = _state.GetSquareKind(x + subX - 1, y + subY - 1);
                    Tile defaultTile = _tileSet.GetDefaultTile(squareKind, layer);
                    int sourceX, sourceY, sourceWidth, sourceHeight;
                    if (subX == 0)
                    {
                        sourceX = 0;
                        sourceWidth = _tileSet.TileXOffset;
                    }
                    else
                    {
                        sourceX = _tileSet.TileXOffset;
                        sourceWidth = _tileSet.TileWidth - _tileSet.TileXOffset;
                    }
                    if (subY == 0)
                    {
                        sourceY = 0;
                        sourceHeight = _tileSet.TileYOffset;
                    }
                    else
                    {
                        sourceY = _tileSet.TileYOffset;
                        sourceHeight = _tileSet.TileHeight - _tileSet.TileYOffset;
                    }
                    Rectangle rect = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);
                    if (defaultTile != null)
                        defaultTile.DrawToGraphics(target, destX, destY, rect);
                }
            }
            target.DrawRectangle(new Pen(Color.FromArgb(255, 255, 0, 0)),
                new Rectangle(destX, destY, _tileSet.TileWidth - 1, _tileSet.TileWidth - 1));
        }
#endif

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public TileSet TileSet { get { return _tileSet; } }
    }
}
