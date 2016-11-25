using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Net.Mime;
using System.Windows.Forms;

namespace Mapdoodle
{
    public class MapDocument
    {
        private readonly IMapForm form;
        private string currentFilename;
        private Map currentMap;
        private int dirtyCount;
        private int hoverX = -1;
        private int hoverY = -1;
        private int lastChangeX;
        private int lastChangeY;
        private bool painting;
        private int paintTerrain = 1;

        public MapDocument(MapForm form)
        {
            this.form = form;
        }

        public void NewMap(TileSet tileSet)
        {
            Debug.Assert(tileSet != null);

            currentMap = new Map(tileSet, 24, 24);
            form.ResizeMapContents(tileSet.TileWidth * currentMap.Width, tileSet.TileHeight * currentMap.Height);
            dirtyCount = 0;
            currentFilename = null;

            Debug.Assert(currentMap != null);
        }

        public void FillTerrainList(TileSet tileSet)
        {
            form.ClearTerrainList();
            for (var i = 0; i < tileSet.TerrainByIndex.Length; i++)
            {
                var terrainMap = new Map(tileSet, 3, 3);
                terrainMap.Rng = null;
                Rectangle updateRectangle;
                terrainMap.SetTerrain(1, 1, new TerrainBrush(tileSet.TerrainByIndex[i]), out updateRectangle);

                var terrainBitmap = new Bitmap(tileSet.TileWidth * 3, tileSet.TileHeight * 3);
                Graphics terrainGraphics = Graphics.FromImage(terrainBitmap);

                var clipRectangle =
                    new Rectangle(tileSet.TileWidth, tileSet.TileHeight, tileSet.TileWidth, tileSet.TileHeight);
                terrainMap.DrawToGraphics(terrainGraphics, clipRectangle);

                var iconBitmap = new Bitmap(32, 32);
                Graphics iconGraphics = Graphics.FromImage(iconBitmap);
                iconGraphics.DrawImage(terrainBitmap,
                    new Rectangle(0, 0, 32, 32),
                    clipRectangle,
                    GraphicsUnit.Pixel);

                form.AddTerrainListItem(iconBitmap, tileSet.TerrainByIndex[i].ToString());
            }
        }

        public void ExtractCoordinatesFromClick(MouseEventArgs e, out int x, out int y)
        {
            x = e.X / currentMap.TileSet.TileWidth;
            y = e.Y / currentMap.TileSet.TileHeight;
        }

        public void PaintTerrain(MouseEventArgs e)
        {
            Debug.Assert(currentMap != null);

            int x;
            int y;
            ExtractCoordinatesFromClick(e, out x, out y);
            Rectangle updateRectangle;
            if ((e.Button & MouseButtons.Right) != 0)
                currentMap.SetTerrain(x, y, new TerrainBrush(currentMap.TileSet.TerrainByIndex[0]), out updateRectangle);
            else
                currentMap.SetTerrain(x, y, new TerrainBrush(currentMap.TileSet.TerrainByIndex[paintTerrain]),
                    out updateRectangle);

            MoveDrawBox(x, y);
            lastChangeX = x;
            lastChangeY = y;

            form.SetStatusText("");
            if (dirtyCount < 1000)
                dirtyCount++;
            form.UpdateInterface(updateRectangle);
            UpdateTitlebar();
        }

        public void ExportImage()
        {
            if (currentMap == null)
            {
                MessageBox.Show("You can't export the map because there is no map loaded.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialog = new SaveFileDialog();

            dialog.Filter = "Image Files(*.png)|*.png|All files (*.*)|*.*";
            dialog.Title = "Export";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream outStream = dialog.OpenFile())
                {
                    var bitmap = new Bitmap(currentMap.Width * currentMap.TileSet.TileWidth,
                        currentMap.Height * currentMap.TileSet.TileHeight);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    currentMap.DrawToGraphics(graphics,
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                    bitmap.Save(outStream, ImageFormat.Png);
                    form.SetStatusText(string.Format("Exported to {0}.", currentFilename));
                }
            }
        }

        public void Undo()
        {
            if (currentMap.Undo())
                dirtyCount--;
            form.UpdateInterface();
            UpdateTitlebar();
        }

        public void Redo()
        {
            if (currentMap.Redo())
                dirtyCount++;
            form.UpdateInterface();
            UpdateTitlebar();
        }

        public void SaveMapAs()
        {
            var dialog = new SaveFileDialog();

            dialog.Filter = "Map Files(*.mdm)|*.mdm|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream outStream = dialog.OpenFile())
                {
                    SaveMapToStream(outStream);
                    currentFilename = dialog.FileName;
                    dirtyCount = 0;
                    UpdateTitlebar();
                    form.SetStatusText(string.Format("Saved as {0}.", currentFilename));
                }
            }
        }

        public void SaveMap()
        {
            if (currentFilename == null)
                SaveMapAs();
            else
            {
                using (Stream outStream = new FileStream(currentFilename, FileMode.Create, FileAccess.ReadWrite))
                {
                    SaveMapToStream(outStream);
                    form.SetStatusText(string.Format("Saved as {0}.", currentFilename));
                    dirtyCount = 0;
                    UpdateTitlebar();
                }
            }
        }

        public void SaveMapToStream(Stream outStream)
        {
            Debug.Assert(currentMap != null);

            Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Map.xml", UriKind.Relative));
            using (Package package = Package.Open(outStream, FileMode.Create))
            {
                PackagePart mapDocument = package.CreatePart(partUriMap, MediaTypeNames.Text.Xml,
                    CompressionOption.Normal);
                using (var writer = new StreamWriter(mapDocument.GetStream()))
                {
                    currentMap.SerializeTo(writer);
                }
            }
        }

        public void LoadMap()
        {
            if (!ConfirmClose())
                return;

            var dialog = new OpenFileDialog();

            dialog.Filter = "Map Files(*.mdm)|*.mdm|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream inStream = dialog.OpenFile())
                {
                    {
                        Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Map.xml", UriKind.Relative));
                        using (Package package = Package.Open(inStream))
                        {
                            PackagePart mapDocument = package.GetPart(partUriMap);
                            using (var reader = new StreamReader(mapDocument.GetStream()))
                            {
                                currentMap = Map.DeserializeFrom(reader);
                                currentFilename = dialog.FileName;
                                dirtyCount = 0;
                                TileSet tileSet = currentMap.TileSet;
                                form.ResizeMapContents(tileSet.TileWidth * currentMap.Width,
                                    tileSet.TileHeight * currentMap.Height);
                                form.UpdateInterface();
                                UpdateTitlebar();
                                form.SetStatusText(string.Format("Loaded {0}.", currentFilename));
                            }
                        }
                    }
                }
            }
            form.UpdateInterface();
        }

        public bool ConfirmClose()
        {
            if (dirtyCount != 0)
            {
                DialogResult result = MessageBox.Show("Save changes to current map?", "Confirm",
                    MessageBoxButtons.YesNoCancel);
                switch (result)
                {
                    case DialogResult.Yes:
                        SaveMap();
                        return true;
                    case DialogResult.Cancel:
                        return false;
                    default:
                        return true;
                }
            }
            return true;
        }

        public void UpdateTitlebar()
        {
            if (currentFilename == null)
                form.SetTitlebarText(String.Format("Mapdoodle"));
            else if (dirtyCount != 0)
                form.SetTitlebarText(String.Format("Mapdoodle - [{0}]*", currentFilename));
            else
                form.SetTitlebarText(String.Format("Mapdoodle - [{0}]", currentFilename));
        }

        public void NewMap()
        {
            if (ConfirmClose())
            {
                TileSet tileSet = TileSet.Load(@"Data\Simple.mdts");

                if (tileSet == null)
                    return;

                NewMap(tileSet);
                form.SetStatusText("New map created.");
                form.UpdateInterface();
                UpdateTitlebar();
            }
        }

        public bool CanUndo()
        {
            if (currentMap != null)
                return currentMap.CanUndo();
            return false;
        }

        public bool CanRedo()
        {
            if (currentMap != null)
                return currentMap.CanRedo();
            return false;
        }

        public void Paint(Graphics graphics, Rectangle clipRectangle)
        {
            if (currentMap != null)
            {
                currentMap.DrawToGraphics(graphics, clipRectangle);
                if (hoverX >= 0)
                {
                    var penWidth = 2;
                    graphics.DrawRectangle(new Pen(Color.FromArgb(192, 0, 255, 0), penWidth),
                        HoverRectangle(1));
                }
            }
        }

        private Rectangle HoverRectangle(int extraBorder)
        {
            int tileWidth = currentMap.TileSet.TileWidth;
            int tileHeight = currentMap.TileSet.TileHeight;
            return new Rectangle(hoverX * tileWidth - extraBorder, hoverY * tileHeight - extraBorder,
                tileWidth + 2 * extraBorder, tileHeight + 2 * extraBorder);
        }

        public void BeginDraw(MouseEventArgs e)
        {
            if (currentMap == null)
                return;

            currentMap.SaveUndo();
            painting = true;
            PaintTerrain(e);
        }

        public void ContinueDrawOrHover(MouseEventArgs e)
        {
            var slopPixels = 8;

            if (currentMap == null)
                return;

            if (painting)
            {
                ContinueDraw(e, slopPixels);
            }
            else
            {
                UpdateHover(e);
            }
        }

        private void ContinueDraw(MouseEventArgs e, int slopPixels)
        {
            // If the drag is near the last tile drawn, don't do anything.
            if (e.X >= lastChangeX * currentMap.TileSet.TileWidth - slopPixels &&
                e.X < (lastChangeX + 1) * currentMap.TileSet.TileWidth + slopPixels &&
                e.Y >= lastChangeY * currentMap.TileSet.TileHeight - slopPixels &&
                e.Y < (lastChangeY + 1) * currentMap.TileSet.TileHeight + slopPixels)
                return;
            PaintTerrain(e);
        }

        private void UpdateHover(MouseEventArgs e)
        {
            int x;
            int y;
            ExtractCoordinatesFromClick(e, out x, out y);

            if (x == hoverX && y == hoverY)
                return;

            Rectangle updateRectangle;
            if (currentMap.SetHoverTerrain(x, y, new TerrainBrush(currentMap.TileSet.TerrainByIndex[paintTerrain]),
                out updateRectangle))
            {
                MoveDrawBox(x, y);
                form.UpdateInterface(updateRectangle);
#if DEBUG
                Console.Out.WriteLine("Hover {0}", updateRectangle);
#endif
            }
        }

        private void MoveDrawBox(int x, int y)
        {
            if (hoverX >= 0)
                form.UpdateInterface(HoverRectangle(2));
            hoverX = x;
            hoverY = y;
            if (hoverX >= 0)
                form.UpdateInterface(HoverRectangle(2));
        }

        public void EndDraw(MouseEventArgs e)
        {
            painting = false;
        }

        public void EndHover(EventArgs e)
        {
            hoverX = hoverY = -1;
            currentMap.ClearHoverTerrain();
            form.UpdateInterface();
        }

        public void SetPaintTerrain(int i)
        {
            paintTerrain = i;
        }
    }
}