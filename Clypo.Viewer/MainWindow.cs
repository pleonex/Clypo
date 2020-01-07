// Copyright (c) 2019 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Clypo.Viewer
{
    using System;
    using System.Collections.Generic;
    using Clypo.Layout;
    using Eto.Drawing;
    using Eto.Forms;
    using Yarhl.FileSystem;

    class MainWindow : Form
    {
        Drawable drawable;
        Clyt layout;

        public MainWindow()
        {
            CreateControls();
        }

        void CreateControls()
        {
            Title = "Nintendo CTR Layout (BCLYT) Viewer ~~ by pleonex";
            ClientSize = new Eto.Drawing.Size(600, 600);

            StackLayout mainPanel = new StackLayout();
            Content = mainPanel;

            drawable = new Drawable();
            drawable.BackgroundColor = Colors.White;
            drawable.Paint += Draw;

            Scrollable drawScrollable = new Scrollable();
            TableLayout drawPanel = new TableLayout();
            drawPanel.BackgroundColor = Colors.Gray;
            drawPanel.Rows.Add(null);
            drawPanel.Rows.Add(new TableRow(null, new TableCell(drawable), null));
            drawPanel.Rows.Add(null);
            drawScrollable.Content = drawPanel;

            TableLayout sidePanel = new TableLayout();
            sidePanel.ClientSize = new Eto.Drawing.Size(200, 30);

            Button openBtn = new Button();
            openBtn.Text = "Open";
            openBtn.Click += OnOpenClicked;
            sidePanel.Rows.Add(null);
            sidePanel.Rows.Add(new TableRow(openBtn, null));

            mainPanel.Orientation = Orientation.Horizontal;
            mainPanel.Items.Add(new StackLayoutItem(sidePanel, VerticalAlignment.Stretch, false));
            mainPanel.Items.Add(new StackLayoutItem(drawScrollable, VerticalAlignment.Stretch, true));
        }

        void OnOpenClicked(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.Filters.Add(new FileFilter("Binary CTR LaYouT", ".bclyt"));
                dialog.Filters.Add(new FileFilter("All files", "*"));
                dialog.MultiSelect = false;
                dialog.Title = "Open a BCLYT file";
                var result = dialog.ShowDialog(this);
                if (result == DialogResult.Ok)
                {
                    OpenClyt(dialog.FileName);
                }
            }
        }

        void OpenClyt(string file)
        {
            using (var node = NodeFactory.FromFile(file))
            {
                layout = node.TransformWith<Binary2Clyt>().GetFormatAs<Clyt>();
            }

            drawable.Size = new Eto.Drawing.Size((int)layout.Layout.Size.Width, (int)layout.Layout.Size.Height);
            drawable.Invalidate();
        }

        void Draw(object sender, PaintEventArgs e)
        {
            var panels = new Queue<(Clypo.Layout.Panel, Vector3)>();
            panels.Enqueue((layout.RootPanel, new Vector3(0, 0, 0)));

            while (panels.Count > 0)
            {
                (Clypo.Layout.Panel current, Vector3 location) = panels.Dequeue();
                Vector3 pos = location + current.Translation;

                if (current is TextSection t)
                {
                    e.Graphics.DrawText(
                        Fonts.Sans(8),
                        Colors.Black,
                        pos.X,
                        -pos.Y,
                        t.Text);
                }
                else if (current is Picture pic)
                {
                    e.Graphics.FillRectangle(
                        Colors.LightBlue,
                        pos.X,
                        -pos.Y,
                        current.Size.Width,
                        current.Size.Height);
                    e.Graphics.DrawText(
                        Fonts.Sans(8),
                        Colors.White,
                        pos.X,
                        -pos.Y,
                       layout.Textures[layout.Materials[pic.MaterialIndex].TexMapEntries[0].Index]);
                }
                else
                {
                    e.Graphics.DrawRectangle(
                        Colors.Black,
                        pos.X,
                        -pos.Y,
                        current.Size.Width,
                        current.Size.Height);
                }

                foreach (var child in current.Children)
                {
                    panels.Enqueue((child, pos));
                }
            }
        }
    }
}
