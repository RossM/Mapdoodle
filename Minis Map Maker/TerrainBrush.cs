using System;

namespace Mapdoodle
{
    public class TerrainBrush : Mapdoodle.IMapBrush
    {
        readonly TerrainKind terrain;

        public TerrainBrush(TerrainKind terrain)
        {
            this.terrain = terrain;
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
                if (terrain1.LayerTerrainId[layer] != BorderKind.Null &&
                    terrain2.LayerTerrainId[layer] != BorderKind.Null &&
                    terrain1.LayerTerrainId[layer] != terrain2.LayerTerrainId[layer])
                    return true;
            }
            return false;
        }

        public bool NeedsDraw(MapState state, int squareX, int squareY)
        {
            return (state.GetSquareKind(squareX, squareY) != terrain);
        }

        public void DrawAt(TileSet tileSet, MapState mapState, Random rng, int squareX, int squareY)
        {
            bool needReSelect;

            needReSelect = false;

            int tileXmin = squareX;
            int tileXmax = squareX + 2;
            int tileYmin = squareY;
            int tileYmax = squareY + 2;

            SetTerrainBase(mapState, squareX, squareY, terrain);
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
                                    if (IncompatibleTerrain(mapState.GetSquareKind(squareX + x - x2, squareY + y - y2), terrain))
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
                SetTerrainBase(mapState, squareX, squareY, terrain);
                tileSet.SelectTiles(mapState, rng, tileXmin, tileYmin, tileXmax, tileYmax);
            }
        }
    }
}