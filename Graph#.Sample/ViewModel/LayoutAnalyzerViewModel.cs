using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Core.Pubsub;
using Core.Pubsub.Events;
using GraphSharp.Controls;
using GraphSharp.Sample.DragDrop;
using GraphSharp.Sample.Model;
using GraphSharp.Serialization;
using Prism.Events;

namespace GraphSharp.Sample.ViewModel
{
    public partial class LayoutAnalyzerViewModel : INotifyPropertyChanged
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

        public GraphLayoutViewModel AnalyzedLayouts { get; private set; }

        public DragDropManagerUtilities DragDropManagerUtilities { get; set; }
        public LayoutAnalyzerViewModel()
        {
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

        private void OnMousePositionChanged(string info)
        {
            PositionInfo = info;
        }

        private void OnCreateNode(CreateNodeBody body)
        {
            if (!(body.Content is PocGraphLayout pocGraphLayout)) return;
            if (pocGraphLayout.Graph == null) return;
            var graph = pocGraphLayout.Graph;
            var to = new PocVertex("Ny", 12);
            to.Point = Mouse.GetPosition(pocGraphLayout);

            
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}