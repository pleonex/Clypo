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
    using System.Linq;
    using Clypo.Layout;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    public class Clyt2Yml : IConverter<Clyt, BinaryFormat>
    {
        public BinaryFormat Convert(Clyt source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ClytYml ymlClyt = new ClytYml { Layout = source.Layout.Size };

            Stack<Panel> stack = new Stack<Panel>();
            stack.Push(source.RootPanel);
            while (stack.Count > 0) {
                Panel panelClyt = stack.Pop();
                var ymlPanel = new PanelYml {
                    Name = panelClyt.Name,
                    Type = panelClyt.GetType().Name,
                    Position = panelClyt.Translation,
                    Scale = panelClyt.Scale,
                    Size = panelClyt.Size,
                };
                ymlClyt.Panels.Add(ymlPanel);

                foreach (var child in panelClyt.Children.Reverse()) {
                    stack.Push(child);
                }
            }

            string yamlText = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build()
                .Serialize(ymlClyt);

            BinaryFormat binary = new BinaryFormat();
            new TextWriter(binary.Stream).Write(yamlText);

            return binary;
        }
    }
}
