﻿using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Ned
{
    public class NedExporter
    {
        public static void Export(Graph graph, string filename)
        {
            var sr = new StreamWriter(filename);
            using (var b = new BinaryWriter(sr.BaseStream))
            {
                var magic = "NEDX".ToCharArray();
                var version = 1;

                b.Write(magic);
                b.Write(version);

                b.Write(graph.Count);

                foreach (var node in graph)
                {
                    b.Write(node.Id.ToByteArray());
                    b.Write(node.NodeInfo.Type);
                    b.Write(node.Outputs.Count);

                    foreach (var output in node.Outputs)
                    {
                        b.Write(output.Text);
                        b.Write(output.ConnectedNode != null);
                        if (output.ConnectedNode != null)
                            b.Write(output.ConnectedNode.ParentNode.Id.ToByteArray());
                    }
                }
            }
        }

        public static void ExportJson(Graph graph, string filename)
        {
            var savedGraph = graph.Select(node => node.Export()).ToList();
            File.WriteAllText(filename, JsonConvert.SerializeObject(savedGraph));
        }
    }
}