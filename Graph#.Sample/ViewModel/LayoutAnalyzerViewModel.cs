using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Core.Models;
using Core.Pubsub;
using Core.Pubsub.Events;
using GraphSharp.Controls;
using GraphSharp.Sample.DragDrop;
using GraphSharp.Sample.Model;
using GraphSharp.Serialization;
using Prism.Events;

namespace GraphSharp.Sample.ViewModel
{
    public partial class LayoutAnalyzerViewModel : INotifyPropertyChanged,IDisposable
    {
        public ICommand RelayoutCommand { get; private set; }

        public ICommand OpenGraphsCommand { get; private set; }

        public ICommand SaveGraphsCommand { get; private set; }



        private GraphModel _selectedGraphModel;
        private GraphLayoutCommand _commands;

        private PocVertex _selectedVertex;

        public PocVertex SelectedVertex
        {
            get { return _selectedVertex; }
            set
            {
                _selectedVertex = value;

                NotifyChanged(nameof(SelectedVertex));
            }
        }

        private string positionInfo;

        public string PositionInfo
        {
            get { return positionInfo; }
            set
            {
                positionInfo = value;
                NotifyChanged(nameof(PositionInfo));
            }
        }


        public ObservableCollection<GraphModel> GraphModels { get; private set; }

        public GraphModel SelectedGraphModel
        {
            get { return _selectedGraphModel; }
            set
            {
                if (_selectedGraphModel != value)
                {
                    _selectedGraphModel = value;
                    SelectedGraphChanged();
                    NotifyChanged("SelectedGraphModel");
                }
            }
        }

        private void SelectedGraphChanged()
        {
            if (AnalyzedLayouts != null)
            {
                AnalyzedLayouts.Graph = _selectedGraphModel.Graph;
            }
        }
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public string LogPath;
        private StreamWriter FileWriter;
        public GraphLayoutViewModel AnalyzedLayouts { get; private set; }

        public DragDropManagerUtilities DragDropManagerUtilities { get; set; }
        public LayoutAnalyzerViewModel()
        {
            var time = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss");
            LogPath = Path.Combine(AssemblyDirectory, $"App_{time}.log");
            //FileWriter = File.AppendText(LogPath);
            InitSubscribers();

            DragDropManagerUtilities = new DragDropManagerUtilities();
            AnalyzedLayouts = new GraphLayoutViewModel
            {
                LayoutAlgorithmType = "EfficientSugiyama"
            };
            GraphModels = new ObservableCollection<GraphModel>();

            RelayoutCommand = new RelayCommand(p => Relayout(), p => AnalyzedLayouts != null, "Relayout");
            OpenGraphsCommand = new RelayCommand(p => OpenGraphs(), title: "Open Graphs");
            SaveGraphsCommand = new RelayCommand(p => SaveGraphs(), p => GraphModels.Count > 0, "Save Graphs");

            CreateSampleGraphs();

            PubSub.Aggregator.GetEvent<CreateNode>().Subscribe(OnCreateNode, ThreadOption.UIThread);
            PubSub.Aggregator.GetEvent<MousePositionChanged>().Subscribe(OnMousePositionChanged, ThreadOption.UIThread);

        }

        private void MousePosition(string text, Point p)
        {
            OnMousePositionChanged(new MousePositionChangedBody(text, p));
            
        }
        private void OnMousePositionChanged(MousePositionChangedBody b)
        {
            PositionInfo = $"{b.Text}\r\nX: {b.Point.X}\r\nY: {b.Point.Y}";

            using (StreamWriter sw = File.AppendText(LogPath))
            {
                var time = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'_'mm'_'ss");
                sw.WriteLine(time);
                sw.WriteLine($"{PositionInfo}");
                //sw.WriteLine($"x: {b.Point.X}");
                //sw.WriteLine($"y: {b.Point.Y}");
                sw.WriteLine();
            }
        }
        
        private void OnCreateNode(CreateNodeBody body)
        {
            
            if (!(body.Content is PocGraphLayout pocGraphLayout)) return;
            MousePosition(pocGraphLayout.GetType().Name,Mouse.GetPosition(pocGraphLayout));
            if (pocGraphLayout.Graph == null) return;
            
            var graph = pocGraphLayout.Graph;
            //MousePosition(graph.GetType().Name,Mouse.GetPosition(graph))

            var to = new PocVertex("Ny", 12);
            to.Point = body.Point;

            MousePosition("OnCreateNode", to.Point);


            graph.AddVertex(to);
            //var vertexControl = pocGraphLayout.GetVertexControl(to);

            //GraphCanvas.SetX(vertexControl, body.Point.X);
            //GraphCanvas.SetY(vertexControl, body.Point.Y);
            //vertexControl.SetValue()

        }

        public void InitSubscribers()
        {
            PubSub.Aggregator.GetEvent<GraphObjectClicked>().Subscribe(GraphObjectClickedEvent);

        }



        private void GraphObjectClickedEvent(object obj)
        {
            if (obj is PocVertex pocVertex)
            {
                SelectedVertex = pocVertex;
            }

        }

        partial void CreateSampleGraphs();

        private static void Relayout()
        {
            LayoutManager.Instance.Relayout();
        }

        private void OpenGraphs()
        {
            var ofd = new OpenFileDialog
            {
                FileName = "FA.gml",
                CheckPathExists = true
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                GraphInfo = ofd.FileName.Load<PocGraphInfo>();
            }
        }

        private void SaveGraphs()
        {
            var fd = new SaveFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                Commands = GraphLayoutCommand.Save;
                PocGraphInfo graphInfo = GraphInfo;
                graphInfo.Save(fd.FileName);
            }
        }

        private PocGraphInfo _graphInfo;
        public PocGraphInfo GraphInfo
        {
            get { return _graphInfo; }
            set
            {
                _graphInfo = value;
                NotifyChanged(nameof(GraphInfo));
            }
        }


        public GraphLayoutCommand Commands
        {
            get { return _commands; }
            set
            {
                if (_commands != value)
                {
                    _commands = value;
                    NotifyChanged(nameof(Commands));
                }

            }
        }

        private void NotifyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            //FileWriter.Dispose();
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}