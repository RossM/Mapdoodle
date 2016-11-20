namespace Mapdoodle
{
    partial class MapForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStrip toolStrip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapForm));
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripContainer toolStripContainer1;
            System.Windows.Forms.SplitContainer splitContainer1;
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Floor");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Rough Wall");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Straight Wall");
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Rubble");
            System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("Water");
            this.buttonNew = new System.Windows.Forms.ToolStripButton();
            this.buttonOpen = new System.Windows.Forms.ToolStripButton();
            this.buttonSave = new System.Windows.Forms.ToolStripButton();
            this.buttonExport = new System.Windows.Forms.ToolStripButton();
            this.buttonUndo = new System.Windows.Forms.ToolStripButton();
            this.buttonRedo = new System.Windows.Forms.ToolStripButton();
            this.terrainList = new System.Windows.Forms.ListView();
            this.terrainIcons = new System.Windows.Forms.ImageList(this.components);
            this.mapContents = new System.Windows.Forms.PictureBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            toolStrip1.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapContents)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = System.Drawing.SystemColors.MenuBar;
            toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNew,
            this.buttonOpen,
            this.buttonSave,
            this.buttonExport,
            toolStripSeparator1,
            this.buttonUndo,
            this.buttonRedo});
            toolStrip1.Location = new System.Drawing.Point(3, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(156, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // buttonNew
            // 
            this.buttonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonNew.Image")));
            this.buttonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(23, 22);
            this.buttonNew.Text = "New";
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonOpen.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpen.Image")));
            this.buttonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(23, 22);
            this.buttonOpen.Text = "Open";
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonSave.Image")));
            this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(23, 22);
            this.buttonSave.Text = "Save";
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.AccessibleName = "";
            this.buttonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonExport.Image = ((System.Drawing.Image)(resources.GetObject("buttonExport.Image")));
            this.buttonExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(23, 22);
            this.buttonExport.Text = "Export";
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonUndo
            // 
            this.buttonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonUndo.Enabled = false;
            this.buttonUndo.Image = ((System.Drawing.Image)(resources.GetObject("buttonUndo.Image")));
            this.buttonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(23, 22);
            this.buttonUndo.Text = "Undo";
            this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
            // 
            // buttonRedo
            // 
            this.buttonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRedo.Enabled = false;
            this.buttonRedo.Image = ((System.Drawing.Image)(resources.GetObject("buttonRedo.Image")));
            this.buttonRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRedo.Name = "buttonRedo";
            this.buttonRedo.Size = new System.Drawing.Size(23, 22);
            this.buttonRedo.Text = "Redo";
            this.buttonRedo.Click += new System.EventHandler(this.buttonRedo_Click);
            // 
            // toolStripContainer1
            // 
            toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(splitContainer1);
            toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(511, 284);
            toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer1.LeftToolStripPanelVisible = false;
            toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.RightToolStripPanelVisible = false;
            toolStripContainer1.Size = new System.Drawing.Size(511, 309);
            toolStripContainer1.TabIndex = 3;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            toolStripContainer1.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(this.terrainList);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.Controls.Add(this.mapContents);
            splitContainer1.Size = new System.Drawing.Size(511, 284);
            splitContainer1.SplitterDistance = 119;
            splitContainer1.TabIndex = 1;
            // 
            // terrainList
            // 
            this.terrainList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.terrainList.HideSelection = false;
            this.terrainList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10});
            this.terrainList.Location = new System.Drawing.Point(0, 0);
            this.terrainList.MultiSelect = false;
            this.terrainList.Name = "terrainList";
            this.terrainList.Size = new System.Drawing.Size(119, 284);
            this.terrainList.SmallImageList = this.terrainIcons;
            this.terrainList.TabIndex = 0;
            this.terrainList.UseCompatibleStateImageBehavior = false;
            this.terrainList.View = System.Windows.Forms.View.List;
            this.terrainList.SelectedIndexChanged += new System.EventHandler(this.terrainList_SelectedIndexChanged);
            // 
            // terrainIcons
            // 
            this.terrainIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this.terrainIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.terrainIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // mapContents
            // 
            this.mapContents.BackColor = System.Drawing.SystemColors.Window;
            this.mapContents.Location = new System.Drawing.Point(0, 0);
            this.mapContents.Name = "mapContents";
            this.mapContents.Size = new System.Drawing.Size(284, 217);
            this.mapContents.TabIndex = 0;
            this.mapContents.TabStop = false;
            this.mapContents.MouseLeave += new System.EventHandler(this.mapContents_MouseLeave);
            this.mapContents.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapContents_MouseMove);
            this.mapContents.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mapContents_MouseDown);
            this.mapContents.Paint += new System.Windows.Forms.PaintEventHandler(this.mapContents_Paint);
            this.mapContents.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mapContents_MouseUp);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
            this.statusStrip.Location = new System.Drawing.Point(0, 333);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(511, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(511, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As..";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exportToolStripMenuItem.Text = "&Export...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(152, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripMenuItem2,
            this.debugToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("undoToolStripMenuItem.Image")));
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.undoToolStripMenuItem.Text = "Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("redoToolStripMenuItem.Image")));
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.redoToolStripMenuItem.Text = "Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.Enabled = false;
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.debugToolStripMenuItem.Text = "&Debug";
            this.debugToolStripMenuItem.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(0, 17);
            // 
            // MapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 355);
            this.Controls.Add(toolStripContainer1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MapForm";
            this.Text = "Mapdoodle";
            this.Load += new System.EventHandler(this.MapForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapForm_FormClosing);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mapContents)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.PictureBox mapContents;
        private System.Windows.Forms.ListView terrainList;
        private System.Windows.Forms.ImageList terrainIcons;
        private System.Windows.Forms.ToolStripButton buttonNew;
        private System.Windows.Forms.ToolStripButton buttonOpen;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripButton buttonExport;
        private System.Windows.Forms.ToolStripButton buttonUndo;
        private System.Windows.Forms.ToolStripButton buttonRedo;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
    }
}

