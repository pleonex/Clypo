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
    using System.IO;
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

            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                PrintHelp();
            }

            string mode = args[0];
            if (mode == "export" && args.Length == 4)
            {
                Console.WriteLine("Exporting:");
                Console.WriteLine("* Input BCLYT: " + args[1]);
                Console.WriteLine("* Output YML: " + args[2]);
                Console.WriteLine("* Output PO: " + args[3]);
                Console.WriteLine();

                Export(args[1], args[2], args[3]);
            }
            else if (mode == "export-dir" && args.Length == 3)
            {
                Console.WriteLine("Exporting directory:");
                Console.WriteLine("* Input: " + args[1]);
                Console.WriteLine("* Output: " + args[2]);

                ExportFolder(args[1], args[2]);
            }
            else if (mode == "import" && args.Length == 5)
            {
                Console.WriteLine("Importing:");
                Console.WriteLine("* Input YML: " + args[1]);
                Console.WriteLine("* Input PO: " + args[2]);
                Console.WriteLine("* Input BCLYT: " + args[3]);
                Console.WriteLine("* Output BCLYT: " + args[4]);
                Console.WriteLine();

                Import(args[1], args[2], args[3], args[4]);
            }
            else if (mode == "import-dir" && args.Length == 4)
            {
                Console.WriteLine("Importing directory:");
                Console.WriteLine("* Original: " + args[1]);
                Console.WriteLine("* Input: " + args[2]);
                Console.WriteLine("* Output: " + args[3]);

                ImportFolder(args[1], args[2], args[3]);
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
            Console.WriteLine("   export-dir input output");
            Console.WriteLine("   import input.yml input.po original.bclyt output.bclyt");
            Console.WriteLine("   import-dir original input output");
            Console.WriteLine();

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

        static void ExportFolder(string baseInput, string baseOutput)
        {
            baseInput = Path.GetFullPath(baseInput).TrimEnd(Path.DirectorySeparatorChar);
            baseOutput = Path.GetFullPath(baseOutput).TrimEnd(Path.DirectorySeparatorChar);

            foreach (string bclyt in Directory.GetFiles(baseInput, "*.bclyt", SearchOption.AllDirectories))
            {
                string path = Path.GetDirectoryName(bclyt);
                string name = Path.GetFileNameWithoutExtension(bclyt);
                string relative = path.Remove(0, baseInput.Length).TrimStart(Path.DirectorySeparatorChar);
                string output = Path.Combine(baseOutput, relative, name);

                Console.WriteLine($"* {relative} {name}");
                Export(
                    bclyt,
                    output + ".yml",
                    output + ".po");
            }
        }

        static void Import(string inputYml, string inputPo, string inputClyt, string output)
        {
            using (Node node = NodeFactory.FromFile(inputClyt))
            {
                node.TransformWith<Binary2Clyt>();

                using (Node ymlNode = NodeFactory.FromFile(inputYml))
                {
                    var yml = ymlNode.GetFormatAs<BinaryFormat>();
                    node.TransformWith<Yml2Clyt, BinaryFormat>(yml);
                }

                using (Node poNode = NodeFactory.FromFile(inputPo))
                {
                    var po = poNode.TransformWith<Binary2Po>().GetFormatAs<Po>();
                    node.TransformWith<Po2Clyt, Po>(po);
                }

                node.TransformWith<Clyt2Binary>()
                    .Stream
                    .WriteTo(output);
            }
        }

        static void ImportFolder(string baseOriginal, string baseInput, string baseOutput)
        {
            baseOriginal = Path.GetFullPath(baseOriginal).TrimEnd(Path.DirectorySeparatorChar);
            baseInput = Path.GetFullPath(baseInput).TrimEnd(Path.DirectorySeparatorChar);
            baseOutput = Path.GetFullPath(baseOutput).TrimEnd(Path.DirectorySeparatorChar);

            foreach (string bclyt in Directory.GetFiles(baseOriginal, "*.bclyt", SearchOption.AllDirectories))
            {
                string path = Path.GetDirectoryName(bclyt);
                string name = Path.GetFileNameWithoutExtension(bclyt);
                string relative = path.Remove(0, baseOriginal.Length).TrimStart(Path.DirectorySeparatorChar);
                string output = Path.Combine(baseOutput, relative, name + ".bclyt");
                string input = Path.Combine(baseInput, relative, name);

                Console.WriteLine($"* {relative} {name}");
                Import(
                    input + ".yml",
                    input + ".po",
                    bclyt,
                    output);
            }
        }
    }
}
