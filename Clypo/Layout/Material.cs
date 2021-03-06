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
namespace Clypo.Layout
{
    using System.Collections.ObjectModel;

    public class Material
    {
        public string Name { get; set; }

        public uint[] TevConstantColors { get; } = new uint[7];

        public bool UseTextureOnly { get; set; }

        public Collection<TextureMapEntry> TexMapEntries { get; } = new Collection<TextureMapEntry>();

        public Collection<TextureMatrixEntry> TexMatrixEntries { get; } = new Collection<TextureMatrixEntry>();

        public Collection<float> TextureCoordGen { get; } = new Collection<float>();

        public Collection<TevStage> TevStages { get; } = new Collection<TevStage>();

        public AlphaCompare AlphaCompare { get; set; }

        public BlendMode ColorBlendMode { get; set; }

        public BlendMode AlphaBlendMode { get; set; }
    }
}
