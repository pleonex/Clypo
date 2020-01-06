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
    using System.Reflection;
    using Clypo.Layout;
    using Yarhl.FileSystem;
    using Yarhl.FileFormat;
    using Yarhl.IO;
    using Yarhl.Media.Text;

    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Clypo -- Export and import BCLYT files");
            Console.WriteLine(
                "v{0} ~~ by pleonex ~~",
                Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();

            if (args.Length != 4)
            {
                PrintHelp();
            }

            string mode = args[0];
            if (mode == "export")
            {
                Console.WriteLine("Exporting:");
                Console.WriteLine("* Input BCLYT: " + args[1]);
                Console.WriteLine("* Output YML: " + args[2]);
                Console.WriteLine("* Output PO: " + args[3]);
                Console.WriteLine();

                Export(args[1], args[2], args[3]);
            }
            else if (mode == "import")
            {
                Console.WriteLine("Importing:");
                Console.WriteLine("* Input YML: " + args[2]);
                Console.WriteLine("* Input PO: " + args[3]);
                Console.WriteLine("* Input BCLYT to overwrite: " + args[1]);
                Console.WriteLine();

                Import(args[1], args[2], args[3]);
            }
            else
            {
                PrintHelp();
            }

            Console.WriteLine("Done!");
        }

        static void PrintHelp()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("   export input.bclyt output.yml output.po");
            Console.WriteLine("   import input.yml input.po inout.bclyt");
            Console.WriteLine();
            Console.WriteLine("To import specify the original BCLYT but take into account");
            Console.WriteLine("that it will be overwritten with the new content.");

            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
            Environment.Exit(1);
        }

        static void Export(string input, string outputYml, string outputPo)
        {
            using (Node node = NodeFactory.FromFile(input))
            {
                node.TransformWith<Binary2Clyt>();
                Clyt clyt = node.GetFormatAs<Clyt>();

                using (var ymlBin = (BinaryFormat)ConvertFormat.With<Clyt2Yml>(clyt))
                {
                    ymlBin.Stream.WriteTo(outputYml);
                }

                node.TransformWith<Clyt2Po>()
                    .TransformWith<Po2Binary>()
                    .Stream.WriteTo(outputPo);
            }
        }

        static void Import(string inputYml, string inputPo, string inout)
        {
            using (Node node = NodeFactory.FromFile(inout))
            {
                node.TransformWith<Binary2Clyt>();

                using (Node ymlNode = NodeFactory.FromFile(inputYml))
                {
                    var yml = ymlNode.GetFormatAs<BinaryFormat>();
                    node.TransformWith<Yml2Clyt, BinaryFormat>(yml);
                }

                using (Node poNode = NodeFactory.FromFile(inputPo))
                {
                    var po = poNode.TransformWith<Po2Binary>().GetFormatAs<Po>();
                    node.TransformWith<Po2Clyt, Po>(po);
                }

                node.TransformWith<Clyt2Binary>()
                    .Stream
                    .WriteTo(inout);
            }
        }
    }
}
