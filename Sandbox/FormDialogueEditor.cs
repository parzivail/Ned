﻿using System;
using System.Windows.Forms;
using Ned;

namespace Sandbox
{
    public partial class FormDialogueEditor : Form
    {
        private readonly MainWindow _nodeEditor;
        private string _fileName;

        private Graph _graph;

        public string FileName
        {
            get => _fileName;

            private set
            {
                _fileName = value;
                Text = string.Format(Resources.AppTitleWorking, value);
            }
        }

        public FormDialogueEditor(MainWindow nodeEditor)
        {
            _nodeEditor = nodeEditor;
            InitializeComponent();
        }

        public void AskExportFile()
        {
            if (efd.ShowDialog() != DialogResult.OK)
                return;

            Lumberjack.Info($"Exporting {efd.FileName}...");
            NedExporter.Export(_graph, efd.FileName);
            Lumberjack.Info($"Exported {efd.FileName}.");
        }

        public void AskExportJsonFile()
        {
            if (ejfd.ShowDialog() != DialogResult.OK)
                return;

            Lumberjack.Info($"Exporting {ejfd.FileName}...");
            NedExporter.ExportJson(_graph, ejfd.FileName);
            Lumberjack.Info($"Exported {ejfd.FileName}.");
        }

        public void AskOpenFile()
        {
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            FileName = ofd.FileName;

            Lumberjack.Info($"Opening {FileName}...");
            _graph = Graph.Load(ofd.FileName);
            Lumberjack.Info($"Opened {FileName}.");
            _nodeEditor.Title = $"{string.Format(Resources.AppTitleWorking, ofd.FileName)}  (beta-{Resources.Version})";
        }

        public void AskSaveFile()
        {
            if (FileName == null)
            {
                AskSaveFileAs();
                return;
            }

            Lumberjack.Info($"Saving {FileName}...");
            _graph.SaveAs(FileName);
            Lumberjack.Info($"Saved {FileName}.");
            _nodeEditor.Title = $"{string.Format(Resources.AppTitleWorking, sfd.FileName)}  (beta-{Resources.Version})";
        }

        public void AskSaveFileAs()
        {
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            FileName = sfd.FileName;

            Lumberjack.Info($"Saving {FileName}...");
            _graph.SaveAs(sfd.FileName);
            Lumberjack.Info($"Saved {FileName}.");
            _nodeEditor.Title = $"{string.Format(Resources.AppTitleWorking, sfd.FileName)}  (beta-{Resources.Version})";
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            AskOpenFile();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            AskSaveFile();
        }

        private void bSaveAs_Click(object sender, EventArgs e)
        {
            AskSaveFileAs();
        }

        private void FormDialogEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            _nodeEditor.Kill();
            e.Cancel = true;
        }

        private void FormDialogEditor_Load(object sender, EventArgs e)
        {
            Text = Resources.AppTitleStatic;

            _graph = new Graph
            {
                new Node(NodeInfo.Start, 50, 50),
                new Node(NodeInfo.End, 300, 100)
            };
        }

        public Graph GetGraph()
        {
            return _graph;
        }
    }
}