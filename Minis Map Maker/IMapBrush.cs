using System;
namespace Mapdoodle
{
    public interface IMapBrush
    {
        void DrawAt(TileSet tileSet, MapState mapState, Random rng, int squareX, int squareY);
        bool NeedsDraw(MapState state, int squareX, int squareY);
    }
}
