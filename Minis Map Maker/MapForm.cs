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
}
