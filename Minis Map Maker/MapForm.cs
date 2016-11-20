using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Diagnostics;

namespace Mapdoodle
{
    public partial class MapForm : Form
    {
        MapDocument _document;

        public MapForm()
        {
            InitializeComponent();
            _document = new MapDocument(this);
        }

        #region Accessors for MapDocument

        public void ResizeMapContents(int width, int height)
        {
            mapContents.Width = width;
            mapContents.Height = height;
        }

        public void ClearTerrainList()
        {
            terrainList.Items.Clear();
        }

        public void AddTerrainListItem(Bitmap iconBitmap, string name)
        {
            terrainIcons.Images.Add(iconBitmap);
            terrainList.Items.Add(name, terrainIcons.Images.Count - 1);
        }

        public void SetStatusText(string text)
        {
            statusText.Text = text;
        }

        public void SetTitlebarText(string text)
        {
            Text = text;
        }

        public void UpdateInterface(Rectangle updateRectangle)
        {
            if (updateRectangle.Width != 0 && updateRectangle.Height != 0)
                mapContents.Invalidate(updateRectangle);

            undoToolStripMenuItem.Enabled = buttonUndo.Enabled = _document.CanUndo();
            redoToolStripMenuItem.Enabled = buttonRedo.Enabled = _document.CanRedo();
        }

        public void UpdateInterface()
        {
            mapContents.Invalidate();

            undoToolStripMenuItem.Enabled = buttonUndo.Enabled = _document.CanUndo();
            redoToolStripMenuItem.Enabled = buttonRedo.Enabled = _document.CanRedo();
        }

        #endregion

        private void mapContents_Paint(object sender, PaintEventArgs e)
        {
            _document.Paint(e.Graphics, e.ClipRectangle);
        }

        private void MapForm_Load(object sender, EventArgs e)
        {
            BorderKind.MakeCompatible(BorderKind.Wall, BorderKind.Wall2);
            BorderKind.MakeCompatible(BorderKind.Wall, BorderKind.Water);

            try
            {
                TileSet tileSet = TileSet.Load(@"Data\Simple.mdts");
                Debug.Assert(tileSet != null);

                _document.NewMap(tileSet);

                _document.FillTerrainList(tileSet);
            }
            catch (FileNotFoundException exception)
            {
                MessageBox.Show("There was an error loading the default tileset. Please ensure you have the .NET Framework version 3.5 installed.\n" + exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
        }

        private void mapContents_MouseDown(object sender, MouseEventArgs e)
        {
            _document.BeginDraw(e);
        }

        private void mapContents_MouseMove(object sender, MouseEventArgs e)
        {
            _document.ContinueDrawOrHover(e);
        }

        private void mapContents_MouseLeave(object sender, EventArgs e)
        {
            _document.EndHover(e);
        }

        private void terrainList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (terrainList.SelectedIndices.Count > 0)
                _document.SetPaintTerrain(terrainList.SelectedIndices[0]);
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            _document.ExportImage();
        }

        private void buttonUndo_Click(object sender, EventArgs e)
        {
            _document.Undo();
        }

        private void buttonRedo_Click(object sender, EventArgs e)
        {
            _document.Redo();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.NewMap();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.ExportImage();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.Redo();
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //StreamWriter stream = new StreamWriter("test.xml");
            //_currentMap.SerializeTo(stream);
            //stream.Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.SaveMapAs();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.LoadMap();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _document.SaveMap();
        }

        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_document.ConfirmClose())
                e.Cancel = true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _document.SaveMap();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            _document.LoadMap();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mapContents_MouseUp(object sender, MouseEventArgs e)
        {
            _document.EndDraw(e);
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            _document.NewMap();
        }
    }

    public class MapDocument
    {
        MapForm _form;

        Map _currentMap;
        string _currentFilename;
        int _dirtyCount;
        bool _painting;
        int _paintTerrain = 1;
        int _lastChangeX;
        int _lastChangeY;
        int _hoverX = -1;
        int _hoverY = -1;

        public MapDocument(MapForm form)
        {
            _form = form;
        }

        public void NewMap(TileSet tileSet)
        {
            Debug.Assert(tileSet != null);

            _currentMap = new Map(tileSet, 24, 24);
            _form.ResizeMapContents(tileSet.TileWidth * _currentMap.Width, tileSet.TileHeight * _currentMap.Height);
            _dirtyCount = 0;
            _currentFilename = null;

            Debug.Assert(_currentMap != null);
        }

        public void FillTerrainList(TileSet tileSet)
        {
            _form.ClearTerrainList();
            for (int i = 0; i < tileSet.TerrainByIndex.Length; i++)
            {
                Map terrainMap = new Map(tileSet, 3, 3);
                terrainMap._rng = null;
                Rectangle updateRectangle;
                terrainMap.SetTerrain(1, 1, new TerrainBrush(tileSet.TerrainByIndex[i]), out updateRectangle);

                Bitmap terrainBitmap = new Bitmap(tileSet.TileWidth * 3, tileSet.TileHeight * 3);
                Graphics terrainGraphics = Graphics.FromImage(terrainBitmap);

                Rectangle clipRectangle =
                    new Rectangle(tileSet.TileWidth, tileSet.TileHeight, tileSet.TileWidth, tileSet.TileHeight);
                terrainMap.DrawToGraphics(terrainGraphics, clipRectangle);

                Bitmap iconBitmap = new Bitmap(32, 32);
                Graphics iconGraphics = Graphics.FromImage(iconBitmap);
                iconGraphics.DrawImage(terrainBitmap,
                    new Rectangle(0, 0, 32, 32),
                    clipRectangle,
                    GraphicsUnit.Pixel);

                _form.AddTerrainListItem(iconBitmap, tileSet.TerrainByIndex[i].ToString());
            }
        }

        public void ExtractCoordinatesFromClick(MouseEventArgs e, out int x, out int y)
        {
            x = e.X / _currentMap.TileSet.TileWidth;
            y = e.Y / _currentMap.TileSet.TileHeight;
        }

        public void PaintTerrain(MouseEventArgs e)
        {
            Debug.Assert(_currentMap != null);

            int x;
            int y;
            ExtractCoordinatesFromClick(e, out x, out y);
            Rectangle updateRectangle;
            if ((e.Button & MouseButtons.Right) != 0)
                _currentMap.SetTerrain(x, y, new TerrainBrush(_currentMap.TileSet.TerrainByIndex[0]), out updateRectangle);
            else
                _currentMap.SetTerrain(x, y, new TerrainBrush(_currentMap.TileSet.TerrainByIndex[_paintTerrain]), out updateRectangle);

            MoveDrawBox(x, y);
            _lastChangeX = x;
            _lastChangeY = y;

            _form.SetStatusText("");
            if (_dirtyCount < 1000)
                _dirtyCount++;
            _form.UpdateInterface(updateRectangle);
            UpdateTitlebar();
        }

        public void ExportImage()
        {
            if (_currentMap == null)
            {
                MessageBox.Show("You can't export the map because there is no map loaded.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Image Files(*.png)|*.png|All files (*.*)|*.*";
            dialog.Title = "Export";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream outStream = dialog.OpenFile())
                {
                    if (outStream != null)
                    {
                        Bitmap bitmap = new Bitmap(_currentMap.Width * _currentMap.TileSet.TileWidth,
                            _currentMap.Height * _currentMap.TileSet.TileHeight);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        _currentMap.DrawToGraphics(graphics,
                            new Rectangle(0, 0, bitmap.Width, bitmap.Height));

                        bitmap.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                        _form.SetStatusText(string.Format("Exported to {0}.", _currentFilename));
                    }
                }
            }
        }

        public void Undo()
        {
            if (_currentMap.Undo())
                _dirtyCount--;
            _form.UpdateInterface();
            UpdateTitlebar();
        }

        public void Redo()
        {
            if (_currentMap.Redo())
                _dirtyCount++;
            _form.UpdateInterface();
            UpdateTitlebar();
        }

        public void SaveMapAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Map Files(*.mdm)|*.mdm|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream outStream = dialog.OpenFile())
                {
                    if (outStream != null)
                    {
                        SaveMapToStream(outStream);
                        _currentFilename = dialog.FileName;
                        _dirtyCount = 0;
                        UpdateTitlebar();
                        _form.SetStatusText(string.Format("Saved as {0}.", _currentFilename));
                    }
                    else
                    {
                        MessageBox.Show("Save failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void SaveMap()
        {
            if (_currentFilename == null)
                SaveMapAs();
            else
            {
                using (Stream outStream = new FileStream(_currentFilename, FileMode.Create, FileAccess.ReadWrite))
                {
                    if (outStream != null)
                    {
                        SaveMapToStream(outStream);
                        _form.SetStatusText(string.Format("Saved as {0}.", _currentFilename));
                        _dirtyCount = 0;
                        UpdateTitlebar();
                    }
                    else
                    {
                        MessageBox.Show("Save failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void SaveMapToStream(Stream outStream)
        {
            Debug.Assert(_currentMap != null);

            Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Map.xml", UriKind.Relative));
            using (Package package = Package.Open(outStream, FileMode.Create))
            {
                PackagePart mapDocument = package.CreatePart(partUriMap, System.Net.Mime.MediaTypeNames.Text.Xml,
                    CompressionOption.Normal);
                using (StreamWriter writer = new StreamWriter(mapDocument.GetStream()))
                {
                    _currentMap.SerializeTo(writer);
                }
            }
        }

        public void LoadMap()
        {
            if (!ConfirmClose())
                return;

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Map Files(*.mdm)|*.mdm|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream inStream = dialog.OpenFile())
                {
                    if (inStream != null)
                    {
                        Uri partUriMap = PackUriHelper.CreatePartUri(new Uri("Map.xml", UriKind.Relative));
                        using (Package package = Package.Open(inStream))
                        {
                            PackagePart mapDocument = package.GetPart(partUriMap);
                            using (StreamReader reader = new StreamReader(mapDocument.GetStream()))
                            {
                                _currentMap = Map.DeserializeFrom(reader);
                                _currentFilename = dialog.FileName;
                                _dirtyCount = 0;
                                TileSet tileSet = _currentMap.TileSet;
                                _form.ResizeMapContents(tileSet.TileWidth * _currentMap.Width, tileSet.TileHeight * _currentMap.Height);
                                _form.UpdateInterface();
                                UpdateTitlebar();
                                _form.SetStatusText(string.Format("Loaded {0}.", _currentFilename));
                            }
                        }
                    }
                }
            }
            _form.UpdateInterface();
        }

        public bool ConfirmClose()
        {
            if (_dirtyCount != 0)
            {
                DialogResult result = MessageBox.Show("Save changes to current map?", "Confirm", MessageBoxButtons.YesNoCancel);
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
            if (_currentFilename == null)
                _form.SetTitlebarText(String.Format("Mapdoodle"));
            else if (_dirtyCount != 0)
                _form.SetTitlebarText(String.Format("Mapdoodle - [{0}]*", _currentFilename));
            else
                _form.SetTitlebarText(String.Format("Mapdoodle - [{0}]", _currentFilename));
        }

        public void NewMap()
        {
            if (ConfirmClose())
            {
                TileSet tileSet = TileSet.Load(@"Data\Simple.mdts");

                if (tileSet == null)
                    return;

                NewMap(tileSet);
                _form.SetStatusText("New map created.");
                _form.UpdateInterface();
                UpdateTitlebar();
            }
        }

        public bool CanUndo()
        {
            if (_currentMap != null)
                return _currentMap.CanUndo();
            return false;
        }

        public bool CanRedo()
        {
            if (_currentMap != null)
                return _currentMap.CanRedo();
            return false;
        }

        public void Paint(Graphics graphics, Rectangle clipRectangle)
        {
            if (_currentMap != null)
            {
                _currentMap.DrawToGraphics(graphics, clipRectangle);
                if (_hoverX >= 0)
                {
                    int penWidth = 2;
                    graphics.DrawRectangle(new Pen(Color.FromArgb(192, 0, 255, 0), penWidth),
                        HoverRectangle(1));
                }
            }
        }

        private Rectangle HoverRectangle(int extraBorder)
        {
            int tileWidth = _currentMap.TileSet.TileWidth;
            int tileHeight = _currentMap.TileSet.TileHeight;
            return new Rectangle(_hoverX * tileWidth - extraBorder, _hoverY * tileHeight - extraBorder,
                                        tileWidth + 2 * extraBorder, tileHeight + 2 * extraBorder);
        }

        public void BeginDraw(MouseEventArgs e)
        {
            if (_currentMap == null)
                return;

            _currentMap.SaveUndo();
            _painting = true;
            PaintTerrain(e);
        }

        public void ContinueDrawOrHover(MouseEventArgs e)
        {
            int slopPixels = 8;

            if (_currentMap == null)
                return;

            if (_painting)
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
            if (e.X >= _lastChangeX * _currentMap.TileSet.TileWidth - slopPixels &&
                e.X < (_lastChangeX + 1) * _currentMap.TileSet.TileWidth + slopPixels &&
                e.Y >= _lastChangeY * _currentMap.TileSet.TileHeight - slopPixels &&
                e.Y < (_lastChangeY + 1) * _currentMap.TileSet.TileHeight + slopPixels)
                return;
            PaintTerrain(e);
        }

        private void UpdateHover(MouseEventArgs e)
        {
            int x;
            int y;
            ExtractCoordinatesFromClick(e, out x, out y);

            if (x == _hoverX && y == _hoverY)
                return;

            Rectangle updateRectangle;
            if (_currentMap.SetHoverTerrain(x, y, new TerrainBrush(_currentMap.TileSet.TerrainByIndex[_paintTerrain]), out updateRectangle))
            {
                MoveDrawBox(x, y);
                _form.UpdateInterface(updateRectangle);
#if DEBUG
                Console.Out.WriteLine("Hover {0}", updateRectangle);
#endif
            }
        }

        private void MoveDrawBox(int x, int y)
        {
            if (_hoverX >= 0)
                _form.UpdateInterface(HoverRectangle(2));
            _hoverX = x;
            _hoverY = y;
            if (_hoverX >= 0)
                _form.UpdateInterface(HoverRectangle(2));
        }

        public void EndDraw(MouseEventArgs e)
        {
            _painting = false;
        }

        public void EndHover(EventArgs e)
        {
            _hoverX = _hoverY = -1;
            _currentMap.ClearHoverTerrain();
            _form.UpdateInterface();
        }

        public void SetPaintTerrain(int i)
        {
            _paintTerrain = i;
        }
    }

}
