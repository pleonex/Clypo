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
namespace Clypo
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using Clypo.Layout;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// CLYT layout format to binary (BCLYT).
    /// </summary>
    /// <remarks>
    /// <p>Based on assembly research and information from:
    /// https://www.3dbrew.org/wiki/CLYT_format</p>
    /// </remarks>
    public class Clyt2Binary : IConverter<Clyt, BinaryFormat>
    {
        const string Id = "CLYT";
        const ushort Endianness = 0xFEFF;
        const ushort HeaderSize = 0x14;
        const uint Version = 0x02020000; // 2.2.0.0

        DataWriter writer;
        int sections;

        public BinaryFormat Convert(Clyt source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var binary = new BinaryFormat();
            writer = new DataWriter(binary.Stream);

            WriteHeader(source);
            WriteSections(source);
            FinishHeader();

            return binary;
        }

        void WriteHeader(Clyt source)
        {
            writer.Write(Id, nullTerminator: false);
            writer.Write(Endianness);
            writer.Write(HeaderSize);
            writer.Write(Version);
            writer.Write(0x00); // placeholder for size
            writer.Write(0x00); // placeholder for number of sections
        }

        void FinishHeader()
        {
            writer.Stream.Position = 0x0C;
            writer.Write((uint)writer.Stream.Length);
            writer.Write(sections);
        }

        void WriteSections(Clyt source)
        {
            WriteSection("lyt1", () => WriteLayout(source.Layout));

            if (source.Textures.Count > 0) {
                WriteSection("txl1", () => WriteTextures(source.Textures));
            }

            if (source.Fonts.Count > 0) {
                WriteSection("fnl1", () => WriteFonts(source.Fonts));
            }

            if (source.Materials.Count > 0) {
                WriteSection("mat1", () => WriteMaterials(source.Materials));
            }

            WritePanelGroup(source.RootPanel);
            WriteGroups(source.RootGroup);
        }

        void WritePanelGroup(Panel panel)
        {
            if (panel is TextSection text) {
                WriteSection("txt1", () => WriteTextInfo(text));
            } else if (panel is Picture picture) {
                WriteSection("pic1", () => WritePictureInfo(picture));
            } else if (panel is Window window) {
                WriteSection("wnd1", () => WriteWindow(window));
            } else {
                WriteSection("pan1", () => WritePanel(panel));
            }

            if (panel.UserData != null) {
                WriteSection("usd1", () => WriteUserData(panel.UserData));
            }

            if (panel.Children.Any()) {
                WriteSection("pas1", () => {});
                foreach (var child in panel.Children) {
                    WritePanelGroup(child);
                }

                WriteSection("pae1", () => {});
            }
        }

        void WriteGroups(Group group)
        {
            WriteSection("grp1", () => WriteGroup(group));

            if (group.Children.Any()) {
                WriteSection("grs1", () => { });
                foreach (var child in group.Children) {
                    WriteGroups(child);
                }

                WriteSection("gre1", () => { });
            }
        }

        void WriteSection(string id, Action writeFnc)
        {
            long initialSize = writer.Stream.Length;
            long initialPos = writer.Stream.Position;

            writer.Write(id, nullTerminator: false);
            writer.Write(0x00); // place holder for size
            writeFnc();
            writer.WritePadding(0x00, 4);

            // Update size
            uint sectionSize = (uint)(writer.Stream.Length - initialSize);
            writer.Stream.Position = initialPos + 0x04;
            writer.Write(sectionSize);

            writer.Stream.Position = initialPos + sectionSize;
            sections++;
        }

        void WriteLayout(LayoutDefinition layout)
        {
            writer.Write((uint)layout.Origin);
            writer.Write(layout.Size.Width);
            writer.Write(layout.Size.Height);
        }

        void WriteTextures(Collection<string> textures)
        {
            writer.Write(textures.Count);

            // Pre-initialize offset table so we can write names at the same time
            long tablePos = writer.Stream.Position;
            writer.WriteTimes(0x00, 4 * textures.Count);

            for (int i = 0; i < textures.Count; i++) {
                writer.Stream.RunInPosition(
                    () => writer.Write((uint)(writer.Stream.Length - tablePos)),
                    tablePos + (i * 4));
                writer.Write(textures[i]);
            }
        }

        void WriteFonts(Collection<string> fonts)
        {
            writer.Write(fonts.Count);

            // Pre-initialize offset table so we can write names at the same time
            long tablePos = writer.Stream.Position;
            writer.WriteTimes(0x00, 4 * fonts.Count);

            for (int i = 0; i < fonts.Count; i++) {
                writer.Stream.RunInPosition(
                    () => writer.Write((uint)(writer.Stream.Length - tablePos)),
                    tablePos + (i * 4));
                writer.Write(fonts[i]);
            }
        }

        void WriteMaterials(Collection<Material> materials)
        {
            long sectionStart = writer.Stream.Position - 8;
            writer.Write(materials.Count);

            // Pre-initialize offset table so we can write names at the same time
            long tablePos = writer.Stream.Position;
            writer.WriteTimes(0x00, 4 * materials.Count);

            for (int idx = 0; idx < materials.Count; idx++) {
                writer.Stream.RunInPosition(
                    () => writer.Write((uint)(writer.Stream.Length - sectionStart)),
                    tablePos + (idx * 4));

                Material mat = materials[idx];
                writer.Write(mat.Name, 0x14);

                for (int j = 0; j < mat.TevConstantColors.Length; j++) {
                    writer.Write(mat.TevConstantColors[j]);
                }

                int flag = 0x00;
                flag |= mat.TexMapEntries.Count;
                flag |= (mat.TexMatrixEntries.Count << 2);
                flag |= (mat.TextureCoordGen.Count << 4);
                flag |= ((mat.UseTextureOnly ? 1 : 0) << 11);
                // TODO: Find a bclyt with the rest of sections

                writer.Write(flag);

                foreach (var entry in mat.TexMapEntries) {
                    writer.Write((ushort)entry.Index);

                    int flag1 = (byte)(entry.WrapS);
                    int flag2 = (byte)(entry.WrapT);
                    flag1 |= (byte)(entry.MinFilter) << 2;
                    flag2 |= (byte)(entry.MagFilter) << 2;

                    writer.Write((byte)flag1);
                    writer.Write((byte)flag2);
                }

                foreach (var entry in mat.TexMatrixEntries) {
                    writer.Write(entry.Translation.X);
                    writer.Write(entry.Translation.Y);
                    writer.Write(entry.Rotation);
                    writer.Write(entry.Scale.X);
                    writer.Write(entry.Scale.Y);
                }

                foreach (var coord in mat.TextureCoordGen) {
                    writer.Write(coord);
                }
            }
        }

        void WriteGroup(Group group)
        {
            writer.Write(group.Name, 0x10);
            writer.Write((uint)group.Panels.Count);
            foreach (var panel in group.Panels) {
                writer.Write(panel, 0x10);
            }
        }

        void WritePanel(Panel panel)
        {
            writer.Write((byte)panel.Flags);
            writer.Write(panel.Origin);
            writer.Write(panel.Alpha);
            writer.Write((byte)panel.MagnificationFlags);

            writer.Write(panel.Name, 0x18);

            writer.Write(panel.Translation.X);
            writer.Write(panel.Translation.Y);
            writer.Write(panel.Translation.Z);

            writer.Write(panel.Rotation.X);
            writer.Write(panel.Rotation.Y);
            writer.Write(panel.Rotation.Z);

            writer.Write(panel.Scale.X);
            writer.Write(panel.Scale.Y);

            writer.Write(panel.Size.Width);
            writer.Write(panel.Size.Height);
        }

        void WriteUserData(UserData data)
        {
            writer.Write(data.Data);
        }

        void WriteTextInfo(TextSection textInfo)
        {
            string text = string.IsNullOrEmpty(textInfo.Text) ?
                string.Empty :
                textInfo.Text + "\0";
            byte[] utf16Text = Encoding.Unicode.GetBytes(text);

            WritePanel(textInfo);

            int additionalSize = textInfo.AdditionalChars * 2; // multiplied by UTF-16 code-point size
            writer.Write((ushort)(utf16Text.Length + additionalSize));
            writer.Write((ushort)utf16Text.Length);
            writer.Write(textInfo.MaterialIndex);
            writer.Write(textInfo.FontIndex);

            writer.Write(textInfo.Unknown54);
            writer.Write(textInfo.Unknown55);
            writer.Write((ushort)0x00); // reserved

            // start text address is always 0x74 because previous fields
            // have a constant size always.
            writer.Write(0x74);

            writer.Write(textInfo.Unknown5C[0]);
            writer.Write(textInfo.Unknown5C[1]);
            writer.Write(textInfo.Unknown64.X);
            writer.Write(textInfo.Unknown64.Y);
            writer.Write(textInfo.Unknown6C);
            writer.Write(textInfo.Unknown70);

            writer.Write(utf16Text);
        }

        void WritePictureInfo(Picture picInfo)
        {
            WritePanel(picInfo);

            writer.Write(picInfo.TopLeftVertexColor);
            writer.Write(picInfo.TopRightVertexColor);
            writer.Write(picInfo.BottomLeftVertexColor);
            writer.Write(picInfo.BottomRightVertexColor);

            writer.Write((ushort)picInfo.MaterialIndex);

            int count = picInfo.TopLeftVertexCoords.Length;
            writer.Write((ushort)count);
            for (int i = 0; i < count; i++) {
                writer.Write(picInfo.TopLeftVertexCoords[i].X);
                writer.Write(picInfo.TopLeftVertexCoords[i].Y);
                writer.Write(picInfo.TopRightVertexCoords[i].X);
                writer.Write(picInfo.TopRightVertexCoords[i].Y);
                writer.Write(picInfo.BottomLeftVertexCoords[i].X);
                writer.Write(picInfo.BottomLeftVertexCoords[i].Y);
                writer.Write(picInfo.BottomRightVertexCoords[i].X);
                writer.Write(picInfo.BottomRightVertexCoords[i].Y);
            }
        }

        void WriteWindow(Window window)
        {
            WritePanel(window);
            writer.Write(window.Unknown);
        }
    }
}
