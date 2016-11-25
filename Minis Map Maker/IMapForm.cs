using System.Drawing;

namespace Mapdoodle
{
    public interface IMapForm
    {
        void ResizeMapContents(int width, int height);
        void ClearTerrainList();
        void AddTerrainListItem(Bitmap iconBitmap, string name);
        void SetStatusText(string text);
        void SetTitlebarText(string text);
        void UpdateInterface(Rectangle updateRectangle);
        void UpdateInterface();
    }
}