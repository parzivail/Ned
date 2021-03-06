﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ned
{
    public class Node
    {
        public static Func<Node, int> WidthCalculator = node => 200;

        public readonly Guid Id = Guid.NewGuid();

        private SavedNode _cachedLoadingNode;

        public Connection Input { get; set; }
        public List<Connection> Outputs { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Layer { get; set; }
        public string Name { get; }
        public NodeInfo NodeInfo { get; }

        public float Width { get; private set; } = 90;
        public float Height => (Math.Max(Outputs.Count, 1) + 1) * 20 + 20;

        internal Node(SavedNode node)
        {
            Outputs = new List<Connection>();

            Id = node.Id;
            X = node.X;
            Y = node.Y;
            Name = node.Name;
            NodeInfo = NodeInfo.GetByType(node.Type);

            _cachedLoadingNode = node;

            RecalculateWidth();
        }

        public Node(NodeInfo nodeInfo, float x, float y)
        {
            X = x;
            Y = y;
            Name = nodeInfo.Name;
            NodeInfo = nodeInfo;

            Outputs = new List<Connection>();

            NodeInfo.AddConnections.Invoke(this);

            RecalculateWidth();
        }

        public Node(Node other)
        {
            X = other.X;
            Y = other.Y;
            Layer = other.Layer;
            Name = other.Name;
            NodeInfo = other.NodeInfo;
            Input = other.Input == null ? null : new Connection(this, other.Input);
            Outputs = other.Outputs.Select(connection => new Connection(this, connection)).ToList();
            RecalculateWidth();
        }

        internal void AddOutput(string text)
        {
            if (!NodeInfo.CanEditConnectors) return;
            Outputs.Add(new Connection(this, NodeSide.Output, Outputs.Count, text));
            RecalculateWidth();
        }

        public void BuildConnections()
        {
            for (var i = 0; i < Outputs.Count; i++)
                Outputs[i].ConnectionIndex = i;
        }

        public override bool Equals(object obj)
        {
            return obj is Node node &&
                   Id.Equals(node.Id);
        }

        internal void FinishLoading(Graph graph)
        {
            if (_cachedLoadingNode.Input != null)
                Input = new Connection(graph, _cachedLoadingNode.Input);

            Outputs.AddRange(_cachedLoadingNode.Outputs.Select(connection => new Connection(graph, connection)));

            _cachedLoadingNode = null;
            RecalculateWidth();
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<Guid>.Default.GetHashCode(Id);
        }

        public void MakeConnections(Graph graph)
        {
            Input?.FinishLoading(graph);

            foreach (var connection in Outputs)
                connection.FinishLoading(graph);
        }

        public static bool operator ==(Node node1, Node node2)
        {
            return EqualityComparer<Node>.Default.Equals(node1, node2);
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }

        public bool Pick(float x, float y)
        {
            return GetBounds().Pick(x, y);
        }

        public void RecalculateWidth()
        {
            Width = WidthCalculator.Invoke(this);
        }

        public void RemoveOutput(Connection connection)
        {
            if (!NodeInfo.CanEditConnectors) return;
            Outputs.Remove(connection);
            RecalculateWidth();
            BuildConnections();
        }

        internal SavedNode Save()
        {
            return new SavedNode
            {
                Type = NodeInfo.Type,
                Id = Id,
                Input = Input?.Save(),
                Outputs = Outputs.Select(connection => connection.Save()).ToList(),
                X = X,
                Y = Y,
                Layer = Layer,
                Name = Name
            };
        }

        internal ExportedNode Export()
        {
            return new ExportedNode
            {
                Type = NodeInfo.Type,
                Id = Id,
                Outputs = Outputs.Select(connection => connection.Export()).ToList(),
            };
        }
    }
}