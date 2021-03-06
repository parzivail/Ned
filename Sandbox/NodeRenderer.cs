﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NanoVGDotNet;
using Ned;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    internal class NodeRenderer
    {
        private readonly Dictionary<NodeInfo, NVGcolor> _colorMap = new Dictionary<NodeInfo, NVGcolor>();
        private readonly MainWindow _window;
        public float gridPitch = 10;
        public Vector2 gridOffset = Vector2.Zero;

        public NodeRenderer(MainWindow window)
        {
            _window = window;

            _colorMap.Add(NodeInfo.None, Color.Black.ToNvgColor());

            _colorMap.Add(NodeInfo.Start, Color.LimeGreen.ToNvgColor());
            _colorMap.Add(NodeInfo.End, Color.IndianRed.ToNvgColor());

            _colorMap.Add(NodeInfo.NpcDialogue, Color.MediumPurple.ToNvgColor());
            _colorMap.Add(NodeInfo.PlayerDialogue, Color.LightSkyBlue.ToNvgColor());

            _colorMap.Add(NodeInfo.WaitForFlag, Color.Orange.ToNvgColor());
            _colorMap.Add(NodeInfo.SetFlag, Color.MediumSeaGreen.ToNvgColor());
            _colorMap.Add(NodeInfo.ClearFlag, Color.MediumVioletRed.ToNvgColor());

            _colorMap.Add(NodeInfo.HasQuest, Color.DarkOrange.ToNvgColor());
            _colorMap.Add(NodeInfo.StartQuest, Color.SteelBlue.ToNvgColor());
            _colorMap.Add(NodeInfo.CompleteQuest, Color.DarkOrchid.ToNvgColor());

            _colorMap.Add(NodeInfo.TriggerEvent, Color.DarkKhaki.ToNvgColor());
        }

        public int GetNodeWidth(Node node)
        {
            var width = 120;
            const int textPadding = 40;

            width = (int)Math.Max(NvgHelper.MeasureString(node.Name).Width + textPadding, width);

            if (node.Input != null)
                width = (int)Math.Max(NvgHelper.MeasureString(node.Input.Text).Width + textPadding, width);

            foreach (var connection in node.Outputs)
                width = (int)Math.Max(NvgHelper.MeasureString(connection.Text).Width + textPadding, width);

            width = (int)(Math.Ceiling(width / gridPitch) * gridPitch);

            return width;
        }

        private void RenderConnection(Connection connection, Connection end)
        {
            var b = end.GetBounds();
            RenderConnection(connection, new Vector2(b.X, b.Y));
        }

        public void RenderConnection(Connection connection, Vector2 end)
        {
            var bound = connection.GetBounds();
            var pos = new Vector2(bound.X, bound.Y);
            var v = new Vector2((pos - end).Length / 4, 0);
            if (pos == end)
                return;
            NanoVG.nvgSave(MainWindow.Nvg);
            NanoVG.nvgLineCap(MainWindow.Nvg, (int)NvgLineCap.Round);
            NanoVG.nvgStrokeColor(MainWindow.Nvg, NanoVG.nvgRGBA(128, 128, 128, 255));
            var ctrl1 = connection.Side == NodeSide.Input ? pos - v : pos + v;
            var ctrl2 = connection.Side == NodeSide.Input ? end + v : end - v;
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgMoveTo(MainWindow.Nvg, pos.X, pos.Y);
            NanoVG.nvgBezierTo(MainWindow.Nvg, ctrl1.X, ctrl1.Y, ctrl2.X, ctrl2.Y, end.X, end.Y);
            NanoVG.nvgStroke(MainWindow.Nvg);
            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        public void RenderConnections(Node node)
        {
            if (!ScreenContains(node))
                return;
            NanoVG.nvgSave(MainWindow.Nvg);
            NanoVG.nvgStrokeColor(MainWindow.Nvg, NanoVG.nvgRGBA(128, 128, 128, 255));

            foreach (var connection in node.Outputs)
                if (connection.ConnectedNode != null)
                    RenderConnection(connection, connection.ConnectedNode);

            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        private void RenderConnector(Connection connection)
        {
            NanoVG.nvgSave(MainWindow.Nvg);

            var pickedForDeletion =
                _window.Selection.HoveringConnection == connection && _window.Keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();
            var r = bound.Radius;
            var twor = 2 * r;
            var halfr = r / 2;
            const int cxnBorderWidth = 2;

            NanoVG.nvgFillColor(MainWindow.Nvg, Color.White.ToNvgColor());

            NanoVG.nvgSave(MainWindow.Nvg);
            if (connection != TextBoxHandler.EditingConnection)
            {
                switch (connection.Side)
                {
                    case NodeSide.Input:
                        NanoVG.nvgTranslate(MainWindow.Nvg, bound.X + twor, bound.Y - r);
                        NvgHelper.RenderString(connection.Text);
                        break;
                    case NodeSide.Output:
                        var s = connection.Text;
                        NanoVG.nvgTranslate(MainWindow.Nvg, bound.X - twor - NvgHelper.MeasureString(s).Width, bound.Y - r);
                        NvgHelper.RenderString(s);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                TextBoxHandler.TextBox.RenderBackground();
                TextBoxHandler.TextBox.RenderForeground();
            }

            NanoVG.nvgRestore(MainWindow.Nvg);

            NanoVG.nvgFillColor(MainWindow.Nvg, Color.DarkSlateGray.ToNvgColor());

            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgCircle(MainWindow.Nvg, bound.X, bound.Y, r + cxnBorderWidth);
            NanoVG.nvgFill(MainWindow.Nvg);

            NanoVG.nvgFillColor(MainWindow.Nvg, connection.Side == NodeSide.Input ? Color.DeepSkyBlue.ToNvgColor() : Color.LimeGreen.ToNvgColor());
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgCircle(MainWindow.Nvg, bound.X, bound.Y, r);
            NanoVG.nvgFill(MainWindow.Nvg);

            NanoVG.nvgBeginPath(MainWindow.Nvg);
            if (_window.Selection.HoveringConnection != null && _window.Selection.DraggingConnection == connection &&
                _window.Selection.HoveringConnection.Side != _window.Selection.DraggingConnection.Side)
                NanoVG.nvgFillColor(MainWindow.Nvg, Color.SlateGray.ToNvgColor());
            else if (connection.ConnectedNode != null)
                NanoVG.nvgFillColor(MainWindow.Nvg, Color.DarkSlateGray.ToNvgColor());

            NanoVG.nvgCircle(MainWindow.Nvg, bound.X, bound.Y, halfr);
            NanoVG.nvgFill(MainWindow.Nvg);

            if (pickedForDeletion)
            {
                NanoVG.nvgFillColor(MainWindow.Nvg, Color.Red.ToNvgColor());

                NanoVG.nvgBeginPath(MainWindow.Nvg);
                NanoVG.nvgCircle(MainWindow.Nvg, bound.X, bound.Y, halfr);
                NanoVG.nvgFill(MainWindow.Nvg);
            }

            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        public void RenderNode(Node node)
        {
            if (!ScreenContains(node))
                return;
            const int borderRadius = 6;
            const int panelInset = 2;
            const float halfPanelInset = panelInset / 2f;

            var headerHeight = (int)(_window.FontLineHeight * 1.2f);

            NanoVG.nvgSave(MainWindow.Nvg);

            if (_window.Selection.SelectedNodes.Contains(node))
            {
                NanoVG.nvgFillColor(MainWindow.Nvg, Color.Black.ToNvgColor());
                NanoVG.nvgBeginPath(MainWindow.Nvg);
                NanoVG.nvgRoundedRect(MainWindow.Nvg, node.X - panelInset - 2, node.Y - 2, node.Width + 2 * (2 + panelInset), node.Height + 4, borderRadius + 2);
                NanoVG.nvgFill(MainWindow.Nvg);
            }

            NanoVG.nvgFillColor(MainWindow.Nvg, _colorMap.ContainsKey(node.NodeInfo) ? _colorMap[node.NodeInfo] : NanoVG.nvgRGBA(0, 0, 0, 255));

            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgRoundedRect(MainWindow.Nvg, node.X - panelInset, node.Y, node.Width + 2 * panelInset, node.Height, borderRadius);
            NanoVG.nvgFill(MainWindow.Nvg);

            NanoVG.nvgFillColor(MainWindow.Nvg, Color.DarkSlateGray.ToNvgColor());
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgRoundedRect(MainWindow.Nvg, node.X, node.Y + headerHeight + panelInset,
                node.Width, node.Height - headerHeight - 2 * panelInset,
                borderRadius - halfPanelInset);
            NanoVG.nvgFill(MainWindow.Nvg);

            NanoVG.nvgFillColor(MainWindow.Nvg, Color.White.ToNvgColor());

            NanoVG.nvgSave(MainWindow.Nvg);
            var headerOffset = (headerHeight + panelInset) / 2f - NvgHelper.MeasureString(node.Name).Height / 2;
            NanoVG.nvgTranslate(MainWindow.Nvg, (int)(node.X + 2 * panelInset), (int)(node.Y + headerOffset));
            NvgHelper.RenderString(node.Name);
            NanoVG.nvgRestore(MainWindow.Nvg);

            if (node.Input != null)
                RenderConnector(node.Input);

            foreach (var nodeOutput in node.Outputs)
                RenderConnector(nodeOutput);
            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        private bool ScreenContains(Node node)
        {
            var nodeRect = node.GetBounds();
            var nodeRectOthers = node.Outputs.Select(connection => connection?.ConnectedNode?.ParentNode?.GetBounds())
                .Where(rectangle => rectangle != null);

            var screenTopLeft = _window.ScreenToCanvasSpace(Vector2.Zero);
            var screenBotRight = _window.ScreenToCanvasSpace(new Vector2(_window.Width, _window.Height));
            var screen = new Rectangle(screenTopLeft.X, screenTopLeft.Y,
                screenBotRight.X - screenTopLeft.X, screenBotRight.Y - screenTopLeft.Y);

            return nodeRect.Intersects(screen) || nodeRectOthers.Any(node1 => node1.Intersects(screen));
        }
    }
}