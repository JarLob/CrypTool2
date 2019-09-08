using Cryptool.PluginBase.Editor;
using QuadTreeLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using WorkspaceManager.Model;
using WorkspaceManager.View.VisualComponents.StackFrameDijkstra;
using WorkspaceManager.View.Visuals;
using WorkspaceManagerModel.Model.Interfaces;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.View.VisualComponents.CryptoLineView
{
    /// <summary>
    /// Interaction logic for CryptoLineView.xaml
    /// </summary>
    public partial class CryptoLineView : UserControl, INotifyPropertyChanged, IUpdateableView
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler IsSelectedChanged;

        #region Fields

        private UIElement[] newItems, oldItems;
        private IEnumerable<ComponentVisual> selected; 
        #endregion
        #region Properties
		public EditorVisual Editor { private set; get; }

        private ConnectionModel model;
        public ConnectionModel Model
        {
            get { return model; }
            set
            {
                model = value;

                if (model.UpdateableView == null)
                    model.UpdateableView = this;
            }
        }
        #endregion
        #region Dependency Properties
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(ConnectorVisual),
           typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public ConnectorVisual Target
        {
            get { return (ConnectorVisual)base.GetValue(TargetProperty); }
            set
            {
                base.SetValue(TargetProperty, value);
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ConnectorVisual),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public ConnectorVisual Source
        {
            get { return (ConnectorVisual)base.GetValue(SourceProperty); }
            set
            {
                base.SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty PathGeoProperty = DependencyProperty.Register("PathGeo", typeof(PathGeometry),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public PathGeometry PathGeo
        {
            get { return (PathGeometry)base.GetValue(PathGeoProperty); }
            set
            {
                base.SetValue(PathGeoProperty, value);
            }
        }

        public static readonly DependencyProperty LineProperty = DependencyProperty.Register("Line", typeof(InternalCryptoLineView),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(null));

        public InternalCryptoLineView Line
        {
            get { return (InternalCryptoLineView)base.GetValue(LineProperty); }
            set
            {
                base.SetValue(LineProperty, value);
            }
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool),
            typeof(CryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsSelectedChanged)));


        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CryptoLineView l = (CryptoLineView)d;
            if(l.IsSelectedChanged != null)
                l.IsSelectedChanged.Invoke(l, null);
        }
        public bool IsSelected
        {
            get { return (bool)base.GetValue(IsSelectedProperty); }
            set
            {
                base.SetValue(IsSelectedProperty, value);
            }
        } 
        #endregion
        #region Constructors
        public CryptoLineView(ObservableCollection<UIElement> visuals)
        {
            InitializeComponent();
            Canvas.SetZIndex(this, -1);
            Line = new InternalCryptoLineView(visuals);
        }

        public CryptoLineView(ConnectionModel model, ConnectorVisual source, ConnectorVisual target)
        {
            // TODO: Complete member initialization
            //Add connection to statistics:
            ComponentConnectionStatistics.IncrementConnectionUsage(source.Model.PluginModel.PluginType,
                                                                   source.Model.GetName(),
                                                                   target.Model.PluginModel.PluginType,
                                                                   target.Model.GetName());

            Editor = (EditorVisual)model.WorkspaceModel.MyEditor.Presentation;
            InitializeComponent();
            Canvas.SetZIndex(this, -1);
            this.Model = model;
            this.Source = source;
            this.Target = target;

            Line = new InternalCryptoLineView(model, source, target, Editor.VisualCollection, Editor.VisualsHelper);
            Line.SetBinding(InternalCryptoLineView.StartPointProperty, WorkspaceManager.View.Base.Util.CreateConnectorBinding(source, this));
            Line.SetBinding(InternalCryptoLineView.EndPointProperty, WorkspaceManager.View.Base.Util.CreateConnectorBinding(target, this));

            Editor.ItemsSelected += new EventHandler<SelectedItemsEventArgs>(itemsSelected);
            Line.ComputationDone += new EventHandler<ComputationDoneEventArgs>(LineComputationDone);
            Source.Update += new EventHandler(Update);
            Target.Update += new EventHandler(Update);

            if (model.PointList != null)
                assembleGeo();
        }

        void Update(object sender, EventArgs e)
        {
            Line.InvalidateVisual();
        }

        #endregion
        #region Private

        private void assembleGeo()
        {
            if (Model == null)
                return;

            if (Model.PointList == null)
                return;

            if(Model.PointList.Count == 0 || Line.isSubstituteLine)
                return;

            PathGeometry geo = new PathGeometry();
            PathFigure myPathFigure = new PathFigure();
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigure.StartPoint = Line.PointList[0].From;

            foreach (var fromto in Line.PointList)
            {
                LineSegment myLineSegment = new LineSegment();
                myLineSegment.Point = fromto.To;
                myPathSegmentCollection.Add(myLineSegment);
            }

            myPathFigure.Segments = myPathSegmentCollection;
            myPathFigureCollection.Add(myPathFigure);

            geo.Figures = myPathFigureCollection;
            PathGeo = geo;
        } 
        #endregion
        #region Public
        public void update()
        {
            Line.update();
        }
        #endregion
        #region EventHandler
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void ContextMenuClick(object sender, RoutedEventArgs e)
        {
            model.UpdateableView = this;
            if (this.model != null && !((WorkspaceManagerClass)this.model.WorkspaceModel.MyEditor).isExecuting())
            {
                this.model.WorkspaceModel.ModifyModel(new DeleteConnectionModelOperation(this.model));
            }
        }

        private void RearrangeLineContextMenuClick(object sender, RoutedEventArgs e)
        {
            Line.Rearrange();
        }

        void itemsSelected(object sender, SelectedItemsEventArgs e)
        {
            oldItems = newItems;
            newItems = e.Items;

            if (newItems != null)
            {
                selected = (IEnumerable<ComponentVisual>)newItems.OfType<ComponentVisual>();
                foreach (var x in selected)
                {
                    x.PositionDeltaChanged += PositionDeltaChangedHandler;
                }
            }
            if (oldItems != null)
            {
                foreach (var x in oldItems.OfType<ComponentVisual>())
                {
                    x.PositionDeltaChanged -= PositionDeltaChangedHandler;
                }
            }

        }

        void PositionDeltaChangedHandler(object sender, PositionDeltaChangedArgs e)
        {
            if (selected == null || Line.IsDragged == true)
                return;

            if (((WorkspaceManagerClass)this.model.WorkspaceModel.MyEditor).isExecuting())
                return;

            bool b = selected.Any(x => x == Target.WindowParent || x == Source.WindowParent);
            Line.IsDragged = b;

            if (!b)
                return;

            foreach (var x in selected)
            {
                x.IsDraggingChanged += new EventHandler<IsDraggingChangedArgs>(WindowParent_IsDraggingChanged);
            }
        }

        void WindowParent_IsDraggingChanged(object sender, IsDraggingChangedArgs e)
        {
            Line.IsDragged = false;
            foreach (var x in selected)
            {
                x.IsDraggingChanged -= new EventHandler<IsDraggingChangedArgs>(WindowParent_IsDraggingChanged);
            }
        }


        void LineComputationDone(object sender, EventArgs e)
        {
            //if (Line.HasComputed)
            //{
                assembleGeo();
            //}
        }
        #endregion
    }

    public class ComputationDoneEventArgs : EventArgs
    {
        public bool IsPathComputationDone {get;set;}
    }

    public sealed class InternalCryptoLineView : Shape, IUpdateableView
    {
        #region Events
        public event EventHandler<ComputationDoneEventArgs> ComputationDone;
        #endregion
        #region Fields
		private IntersectPoint intersectPoint;
        private static double offset = 6;

        private Brush ActiveColorBrush;
        private Brush NonActiveColorBrush;
        internal bool isSubstituteLine = false;
        internal bool loaded = false;
	    #endregion
        #region Properties
        private ConnectorVisual endPointSource;
        public ConnectorVisual EndPointSource
        {
            get { return endPointSource; }
            set
            {
                value.Update += new EventHandler(ConnectorSourceUpdate);
                endPointSource = value;
            }
        }

        private ConnectorVisual startPointSource;
        public ConnectorVisual StartPointSource
        {
            get { return startPointSource; }
            set
            {
                value.Update += new EventHandler(ConnectorSourceUpdate);
                startPointSource = value;
            }
        }

        private ObservableCollection<FromTo> pointList = new ObservableCollection<FromTo>();
        public ObservableCollection<FromTo> PointList
        {
            get { return pointList; }
        }

        private ConnectionModel model = null;
        public ConnectionModel Model
        {
            get { return model; }
            set
            {
                model = value;

                if (model.UpdateableView == null)
                    model.UpdateableView = this;

                if (model.PointList != null)
                {
                    PointList.Clear();
                    for (int i = 0; i <= model.PointList.Count() - 2; i++)
                    {
                        var from = model.PointList[i];
                        var to = model.PointList[i + 1];

                        if (i == model.PointList.Count() - 2)
                            PointList.Add(new FromTo(from, to, FromToMeta.HasEndpoint));
                        else if (i == 0)
                            PointList.Add(new FromTo(from, to, FromToMeta.HasStartPoint));
                        else
                            PointList.Add(new FromTo(from, to));
                    }
                    loaded = true;
                }
            }
        }

        private ObservableCollection<UIElement> visuals = null;
        public ObservableCollection<UIElement> Visuals
        {
            get { return visuals; }
            private set { visuals = value; }
        }
        
        #endregion
        #region Dependency Properties
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint",
           typeof(Point), typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0),new PropertyChangedCallback(OnIsPositionChanged)));

        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint",
            typeof(Point), typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(new Point(0, 0),new PropertyChangedCallback(OnIsPositionChanged)));


        private static void OnIsPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InternalCryptoLineView l = (InternalCryptoLineView)d;

            l.HasComputed = false;
            if (l.model != null &&l.model.IsCopy) {
                l.model.IsCopy = false;
                l.loaded = false;
            }

            l.InvalidateVisual();
            if(l.helper != null)
                l.helper.DrawDecoration();
        }

        public static readonly DependencyProperty IsDraggedProperty = DependencyProperty.Register("IsDragged", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsDraggedValueChanged)));

        public bool IsDragged
        {
            get { return (bool)base.GetValue(IsDraggedProperty); }
            set
            {
                base.SetValue(IsDraggedProperty, value);
            }
        }

        public static readonly DependencyProperty HasComputedProperty = DependencyProperty.Register("HasComputed", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnHasComputedChanged)));

        public bool HasComputed
        {
            get { return (bool)base.GetValue(HasComputedProperty); }
            private set
            {
                base.SetValue(HasComputedProperty, value);
            }
        }

        public static readonly DependencyProperty IsEditingPointProperty = DependencyProperty.Register("IsEditingPoint", typeof(bool),
            typeof(InternalCryptoLineView), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnHasComputedChanged)));
        private VisualsHelper helper;

        public bool IsEditingPoint
        {
            get { return (bool)base.GetValue(IsEditingPointProperty); }
            set
            {
                base.SetValue(IsEditingPointProperty, value);
            }
        }

        public bool HasManualModification { get; set; }

        private static void OnIsDraggedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InternalCryptoLineView l = (InternalCryptoLineView)d;
            l.HasComputed = false;
            l.IsEditingPoint = false;
            l.loaded = false;
            if (l.IsDragged == false)
            {
                l.InvalidateVisual();
            }
        }

        public bool HasDecoration { get; set; }

        internal void Save()
        {
            if (model == null || !(PointList.Count > 0))
                return;

            Model.PointList = new List<Point>();
            foreach (var p in PointList)
            {
                Model.PointList.Add(p.From);
            }
            Model.PointList.Add(PointList[PointList.Count - 1].To);
        }

        private static void OnHasComputedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            InternalCryptoLineView l = (InternalCryptoLineView)d;
            if (l.model == null)
                return;

            if (l.HasComputed == true)
            {
                l.Save();
            }
        }

        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set 
            {
                SetValue(StartPointProperty, value);
            }
        }

        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set 
            {
                SetValue(EndPointProperty, value);
            }
        }

		#endregion
        #region Constructors
        public InternalCryptoLineView(ObservableCollection<UIElement> visuals)
        {
            Stroke = Brushes.Black;
            StrokeThickness = 2;
            this.Visuals = visuals;
            isSubstituteLine = true;
        }

        public InternalCryptoLineView(ConnectionModel connectionModel, ConnectorVisual source,
            ConnectorVisual target, ObservableCollection<UIElement> visuals, VisualsHelper helper)
        {
            this.Loaded += new RoutedEventHandler(CryptoLineView_Loaded);
            this.Model = connectionModel;
            this.StartPointSource = source;
            this.EndPointSource = target;
            this.Visuals = visuals;
            this.helper = helper;
        } 
        #endregion
        #region Public
        public void InvalidateAllLines()
        {
            var col = Visuals.OfType<CryptoLineView>();
            foreach (var x in col)
            {
                x.Line.InvalidateVisual();
            }
        } 
        #endregion
		#region Overrides

		protected override Geometry DefiningGeometry
		{
			get
			{
				StreamGeometry geometry = new StreamGeometry();
				geometry.FillRule = FillRule.EvenOdd;

				using (StreamGeometryContext context = geometry.Open())
				{
                    internalGeometryDraw(context);
				}

				geometry.Freeze();
				return geometry;
			}
		}		

		#endregion
		#region Private

        public void ClearIntersect()
        {
            foreach (FromTo fromTo in PointList)
                fromTo.Intersection.Clear();
        }

        public void DrawDecoration()
        {
            foreach (var element in Visuals)
            {
                if (element is CryptoLineView)
                {
                    CryptoLineView result = element as CryptoLineView;

                    if (result.Line.Equals(this))
                        continue;

                    foreach (FromTo fromTo in PointList)
                    {
                        foreach (FromTo resultFromTo in result.Line.PointList)
                        {
                            if (LineUtil.FindIntersection(fromTo.From, fromTo.To, resultFromTo.From, resultFromTo.To, out intersectPoint))
                            {
                                fromTo.Intersection.Add(intersectPoint);
                                //resultFromTo.Intersection.Add(intersectPoint);
                            }
                        }
                    }
                }
            }
        }
       
		private void internalGeometryDraw(StreamGeometryContext context)
		{
            makeOrthogonalPoints();

            context.BeginFigure(StartPoint, true, false);


            foreach (FromTo fromTo in PointList)
            {
                if (fromTo.Intersection.Count > 0)
                {
                    foreach (IntersectPoint interPoint in fromTo.Intersection)
                    {
                        switch (fromTo.DirSort)
                        {
                            case DirSort.X_ASC:
                                if (interPoint.Mode == IntersectPointMode.NormalIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X - offset, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - offset), new Point(interPoint.Point.X + offset, interPoint.Point.Y), true, true);
                                }
                                else if (interPoint.Mode == IntersectPointMode.InnerIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 3.5), new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 3.5), new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
                                }
                                break;
                            case DirSort.X_DESC:
                                if (interPoint.Mode == IntersectPointMode.NormalIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X + offset, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - offset), new Point(interPoint.Point.X - offset, interPoint.Point.Y), true, true);
                                }
                                else if (interPoint.Mode == IntersectPointMode.InnerIntersect)
                                {
                                    context.LineTo(new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y - 3.5), new Point(interPoint.Point.X - 2.5, interPoint.Point.Y), true, true);
                                    context.QuadraticBezierTo(new Point(interPoint.Point.X, interPoint.Point.Y + 3.5), new Point(interPoint.Point.X + 2.5, interPoint.Point.Y), true, true);
                                }
                                break;
                        }
                    }
                    context.LineTo(fromTo.To, true, true);
                }
                else
                {
                    context.LineTo(fromTo.To, true, true);
                }
            }
        }

        /// <summary>
        /// Will try to connect "node1" with "node2".
        /// This method updates the neighbor information of these nodes and adds new helper nodes into
        /// "nodeList".
        /// </summary>
        /// <param name="node1">The first node to connect.</param>
        /// <param name="node2">The second node to connect.</param>
        /// <param name="nodeList">List of all nodes. This method will add new helper nodes to this list.</param>
        /// <param name="quadTreePlugins">The quad tree of the plugin's visuals.</param>
        /// <returns></returns>
        private bool TryConnectNodes(Node node1, Node node2, List<Node> nodeList, QuadTree<FakeNode> quadTreePlugins)
        {
            if (node1 == node2)
                return true;
            if (node1.Vertices.Contains(node2))
                return true;
            if (node1.HelpingPointConnectableVertices.Contains(node2))
                return true;
            if (node1.NotConnectableVertices.Contains(node2))
                return false;

            bool canConnect = true;
            // no helping point required?
            if (node1.Point.X == node2.Point.X ||
                node1.Point.Y == node2.Point.Y)
            {
                canConnect = LineUtil.PerformOrthogonalPointConnection(node1, node2, quadTreePlugins);
            }
            else
            {
                Point help = new Point(node1.Point.X, node2.Point.Y);

                if (!LineUtil.PerformOrthogonalPointConnection(node1, help, node2, nodeList, quadTreePlugins))
                {
                    help = new Point(node2.Point.X, node1.Point.Y);
                    if (!LineUtil.PerformOrthogonalPointConnection(node1, help, node2, nodeList, quadTreePlugins))
                    {
                        canConnect = false;
                    }
                }
            }

            if (!canConnect)
            {
                node1.NotConnectableVertices.Add(node2);
                node2.NotConnectableVertices.Add(node1);
            }

            return canConnect;
        }

        /// <summary>
        /// Will try to find a path with stops taken from the list "potentialStopNodes".
        /// Calling this method will update neighbor information in the nodes.
        /// Furthermore, this method will add new helper nodes into "nodeList", which will be needed when
        /// searching for the concrete path afterwards.
        /// </summary>
        /// <param name="startNode">Start node of the path.</param>
        /// <param name="endNode">End node of the path.</param>
        /// <param name="potentialStopNodes">All nodes which can be potentially be used as stop nodes.</param>
        /// <param name="nodeList">List of all nodes. This method will add new helper nodes to this list.</param>
        /// <param name="quadTreePlugins">The quad tree of the plugin's visuals.</param>
        /// <returns>Whether a path has been found.</returns>
        private bool SearchPath(Node startNode, Node endNode, List<Node> potentialStopNodes, List<Node> nodeList, QuadTree<FakeNode> quadTreePlugins)
        {
            const uint maxNumberOfStops = 2;    //This is a tradeoff between performance and line complexity
            uint numberOfStops = 0;
            bool hasFoundPath = false;
            do
            {
                hasFoundPath = SearchPathWithStops(startNode, endNode, numberOfStops, potentialStopNodes, nodeList, quadTreePlugins);
                numberOfStops++;
            } while (numberOfStops <= maxNumberOfStops && !hasFoundPath && numberOfStops <= potentialStopNodes.Count);

            return hasFoundPath;
        }

        /// <summary>
        /// Will try to find a path with exactly "numberOfStops" stops, taken from the list "potentialStopNodes".
        /// </summary>
        /// <param name="startNode">Start node of the path.</param>
        /// <param name="endNode">End node of the path.</param>
        /// <param name="numberOfStops">Number of stops the path should exactly have.</param>
        /// <param name="potentialStopNodes">All nodes which can be potentially be used as stop nodes</param>
        /// <param name="nodeList">List of all nodes. This method will add new helper nodes to this list.</param>
        /// <param name="quadTreePlugins">The quad tree of the plugin's visuals.</param>
        /// <returns>Whether a path has been found.</returns>
        private bool SearchPathWithStops(Node startNode, Node endNode, uint numberOfStops, IEnumerable<Node> potentialStopNodes, List<Node> nodeList, QuadTree<FakeNode> quadTreePlugins)
        {
            if (startNode == endNode)
            {
                return true;
            }
            if (numberOfStops == 0)
            {
                return TryConnectNodes(startNode, endNode, nodeList, quadTreePlugins);
            }

            bool hasFoundPath = false;
            foreach (var nextStopNode in potentialStopNodes)
            {
                if (TryConnectNodes(startNode, nextStopNode, nodeList, quadTreePlugins))
                {
                    var stopNodesLeft = potentialStopNodes.Where(node => node != nextStopNode);
                    //If connection to "nextStopNode" is possible, go down the path recursively:
                    if (SearchPathWithStops(nextStopNode, endNode, numberOfStops - 1, stopNodesLeft, nodeList, quadTreePlugins))
                    {
                        //Do not return here immediately, because we want to find the *best* path with "numberOfStops" stops.
                        hasFoundPath = true;
                    }
                }
            }

            return hasFoundPath;
        }

        /// <summary>
        /// Returns all potential stop nodes, which are gathered by taking the edges ("routing points")
        /// of all visual components currently available.
        /// </summary>
        /// <returns>List of all potential stop nodes.</returns>
        private IEnumerable<Node> GetPotentialStopNodes()
        {
            foreach (var element in Visuals.OfType<ComponentVisual>())
            {
                for (int routPoint = 0; routPoint < 4; ++routPoint)
                {
                    Point point = element.GetRoutingPoint(routPoint);
                    yield return new Node() { Point = point };
                }
            }
        }

        private void makeOrthogonalPoints()
        {
            if (!IsEditingPoint)
            {
                if (HasManualModification)
                {
                    //Keep manually modified connections "as is", but adjust start and end point if necessary:
                    if (!AdjustManuallyModifiedLine(PointList, StartPoint, EndPoint))
                    {
                        //Adjustment of manually modified line failed, so switch back to "automatic mode":
                        HasManualModification = false;
                    }
                }

                if (!HasManualModification)
                {
                    bool failed = false;
                    if (!isSubstituteLine && !IsDragged && !HasComputed && !loaded)
                    {
                        var startNode = new Node() { Point = LineUtil.Cheat42(StartPoint, StartPointSource, 1) };
                        var endNode = new Node() { Point = LineUtil.Cheat42(EndPoint, EndPointSource, -1) };
                        var nodeList = new List<Node>() { startNode, endNode };
                        var potentialStopNodes = GetPotentialStopNodes().ToList();
                        //nodeList contains all nodes (start, end and potential stop nodes):
                        nodeList.AddRange(potentialStopNodes);
                        var quadTreePlugins = helper.PluginTree;

                        LinkedList<Node> path = null;
                        if (SearchPath(startNode, endNode, potentialStopNodes, nodeList, quadTreePlugins))
                        {
                            //If a connection is found, use Dijskstra algorithm anyway to find the best one.
                            //It will run on "nodeList", which may contain some additional stops added by "SearchPath" now.
                            var dijkstra = new Dijkstra<Node>();
                            path = dijkstra.findPath(nodeList, startNode, endNode);
                        }

                        if (path != null)
                        {
                            var list = path.ToList();
                            PointList.Clear();
                            Point startPoint = StartPoint, curPoint, prevPoint = startPoint;
                            bool isStart = true;
                            for (int c = 0; c < list.Count; ++c)
                            {
                                var i = list[c];
                                curPoint = i.Point;
                                //this.PointList.Add(new FromTo(prevPoint, curPoint));
                                if ((startPoint.X != curPoint.X && startPoint.Y != curPoint.Y))
                                {
                                    if (isStart)
                                    {
                                        this.PointList.Add(new FromTo(startPoint, prevPoint, FromToMeta.HasStartPoint));
                                        isStart = false;
                                    }
                                    else
                                        this.PointList.Add(new FromTo(startPoint, prevPoint));

                                    startPoint = prevPoint;
                                }
                                if (c == list.Count - 1)
                                    if ((startPoint.X != EndPoint.X && startPoint.Y != EndPoint.Y))
                                    {
                                        this.PointList.Add(new FromTo(startPoint, curPoint));
                                        startPoint = curPoint;
                                    }

                                prevPoint = curPoint;
                            }
                            this.PointList.Add(new FromTo(startPoint, EndPoint, FromToMeta.HasEndpoint));
                            AdjustLineSegments(this.PointList);

                            HasComputed = true;
                            raiseComputationDone(true);
                            return;
                        }
                        failed = true;
                    }

                    //Failsafe
                    if (IsDragged || failed || isSubstituteLine)
                    {
                        if (StartPoint.X < EndPoint.X)
                        {
                            PointList.Clear();
                            PointList.Add(new FromTo(StartPoint, new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y)));
                            PointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, StartPoint.Y), new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y)));
                            PointList.Add(new FromTo(new Point((EndPoint.X + StartPoint.X) / 2, EndPoint.Y), EndPoint));
                        }
                        else
                        {
                            if (StartPoint.X > EndPoint.X)
                            {
                                PointList.Clear();
                                PointList.Add(new FromTo(StartPoint, new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y)));
                                PointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, StartPoint.Y), new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y)));
                                PointList.Add(new FromTo(new Point((StartPoint.X + EndPoint.X) / 2, EndPoint.Y), EndPoint));
                            }
                        }
                    }
                    raiseComputationDone(false);
                }
            }

            HasComputed = true; //Set to "true" here as well, to avoid unnecessary recomputation.
            raiseComputationDone(true);
        }

        /// <summary>
        /// Adjusts only start and end points of an already existing path which was modified manually.
        /// </summary>
        /// <param name="points">The points of the old line.</param>
        /// <param name="newStartPoint">The new start point to adjust to.</param>
        /// <param name="newEndPoint">The new end point to adjust to.</param>
        /// <returns>Whether adjustment was possible.</returns>
        private static bool AdjustManuallyModifiedLine(IEnumerable<FromTo> points, Point newStartPoint, Point newEndPoint)
        {
            //Zip point lists to create adjacent points:
            var adjacentPoints = points.Zip(points.Skip(1), (a, b) => (First: a, Second: b));
            //Get start point and its neighbor:
            var (startPoint, startPointNeighbor) = adjacentPoints.SingleOrDefault(
                pair => pair.First.MetaData == FromToMeta.HasStartPoint || pair.First.MetaData == FromToMeta.HasEndStartPoint);
            //Get end point and its neighbor:
            var (endPointNeighbor, endPoint) = adjacentPoints.SingleOrDefault(
                pair => pair.Second.MetaData == FromToMeta.HasEndpoint || pair.Second.MetaData == FromToMeta.HasEndStartPoint);

            if (startPoint == null || startPointNeighbor == null || endPoint == null || endPointNeighbor == null)
            {
                return false;
            }

            if (startPoint.IsXDir == startPointNeighbor.IsXDir || endPoint.IsXDir == endPointNeighbor.IsXDir)
            {
                //start and end points need to have different directions in relation to their neighbors for the adjustment to work.
                return false;
            }

            //Adjust start point (and neighbor):
            if (newStartPoint != startPoint.From)
            {
                var startDiff = Point.Subtract(newStartPoint, startPoint.From);
                var startAdj = startPoint.IsXDir ? new Vector(0, startDiff.Y) : new Vector(startDiff.X, 0);
                startPoint.From = newStartPoint;
                startPoint.To = Point.Add(startPoint.To, startAdj);
                startPointNeighbor.From = startPoint.To;
            }

            //Adjust end point (and neighbor):
            if (newEndPoint != endPoint.To)
            {
                var endDiff = Point.Subtract(newEndPoint, endPoint.To);
                var endAdj = endPoint.IsXDir ? new Vector(0, endDiff.Y) : new Vector(endDiff.X, 0);
                endPoint.To = newEndPoint;
                endPoint.From = Point.Add(endPoint.From, endAdj);
                endPointNeighbor.To = endPoint.From;
            }

            return true;
        }

        /// <summary>
        /// Adjust the segments of a computed line path in order to avoid collisions with other lines.
        /// </summary>
        private void AdjustLineSegments(IEnumerable<FromTo> segments)
        {
            //Create triplets of all neighbor segments by zipping the segments enumeration twice:
            var segmentTriplets = segments.Zip(segments.Skip(1), (a, b) => (previous: a, current: b)).Zip(segments.Skip(2), (p, c) => (p.previous, p.current, next: c));

            //Go through all triplets and adjust the middle element ("curSegment"):
            foreach (var (prevSegment, curSegment, nextSegment) in segmentTriplets)
            {
                if (curSegment.MetaData == FromToMeta.None)    //Only adjust line segments which do not contain end or start points.
                {
                    // X / Y direction needs to be different between neighbor segments for the adjustment to work:
                    if (prevSegment.IsXDir != curSegment.IsXDir && curSegment.IsXDir != nextSegment.IsXDir)
                    {
                        if (CheckCrossLineSegmentCollision(curSegment))
                        {
                            //Current segment collides, so try to adjust it:
                            if (AdjustLineSegment(curSegment))
                            {
                                //Current segment was adjusted, so adjust neightbor segments accordingly:
                                prevSegment.To = curSegment.From;
                                nextSegment.From = curSegment.To;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adjust a single segment of a line in order to avoid collisions with other lines.
        /// </summary>
        /// <returns>If the passed segment could be adjusted.</returns>
        private bool AdjustLineSegment(FromTo segment)
        {
            var quadTreeLines = helper.FromToTree;
            var quadTreePlugins = helper.PluginTree;
            const uint maxAdjustmentTries = 6;

            for (uint adjustmentTry = 0; adjustmentTry < maxAdjustmentTries; adjustmentTry++)
            {
                var adjustedSegment = CreateAdjustedLineSegmentAlternative(segment, adjustmentTry);
                if (!quadTreePlugins.QueryAny(adjustedSegment.GetRectangle()))
                {
                    //No collision with plugins. Now check collision with other lines:
                    if (!CheckCrossLineSegmentCollision(adjustedSegment))
                    {
                        //The adjustment does not collide, so use it:
                        segment.From = adjustedSegment.From;
                        segment.To = adjustedSegment.To;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks whether the passed line segment collides with any other relevant line in an "unpleasant" way.
        /// </summary>
        private bool CheckCrossLineSegmentCollision(FromTo segment)
        {
            var quadTreeLines = helper.FromToTree;
            foreach (var crossLine in quadTreeLines.Query(segment.GetRectangle()))
            {
                var crossLineView = crossLine.LogicalParent;
                //Check if found cross line is a different one, is not dragged and runs in the same direction (X or Y):
                if (crossLineView != this && !crossLineView.IsDragged && crossLine.FromTo.IsXDir == segment.IsXDir)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Creates one out of many possible alternatives of an adjusted line segment.
        /// This is done by moving the line segment successively to the left and right resp. to the top and bottom.
        /// Does not check for collisions here.
        /// </summary>
        /// <param name="segment">The line segment to adjust.</param>
        /// <param name="alternativeIndex">An index controlling which adjustment alternative should be used.</param>
        /// <returns>The adjusted line.</returns>
        private FromTo CreateAdjustedLineSegmentAlternative(FromTo segment, uint alternativeIndex)
        {
            const double adjustmentStepMargin = 10; //Controls which margin should be added per alternative step
            var sign = ((alternativeIndex % 2) == 0 ? 1 : -1);  //Alternates between 1 and -1 with each alternative
            var gap = (alternativeIndex + 2) / 2; //Numerical series: 1, 1, 2, 2, 3, 3, ...
            var adjustment = sign * gap * adjustmentStepMargin;

            switch (segment.DirSort)
            {
                case DirSort.X_ASC:
                case DirSort.X_DESC:
                    return new FromTo(
                        Point.Add(segment.From, new Vector(0, adjustment)), 
                        Point.Add(segment.To, new Vector(0, adjustment)));
                case DirSort.Y_ASC:
                case DirSort.Y_DESC:
                    return new FromTo(
                        Point.Add(segment.From, new Vector(adjustment, 0)),
                        Point.Add(segment.To, new Vector(adjustment, 0)));
            }
            return segment;
        }

        private void raiseComputationDone(bool b)
        {
            if (this.ComputationDone != null)
                ComputationDone.Invoke(this, new ComputationDoneEventArgs() { IsPathComputationDone = b });
        }

        internal void reset()
        {
            if (NonActiveColorBrush == null)
            {
                NonActiveColorBrush = new SolidColorBrush(ColorHelper.GetLineColor(Model.ConnectionType));
            }
            Stroke = NonActiveColorBrush;
            StrokeThickness = 2;
        }
		#endregion
        #region EventsHandler
        void ConnectorSourceUpdate(object sender, EventArgs e)
        {
            loaded = false;
            HasComputed = false;
        }

        void CryptoLineView_Loaded(object sender, RoutedEventArgs e)
        {
            Color color = ColorHelper.GetLineColor(Model.ConnectionType);
            Stroke = new SolidColorBrush(color);
            StrokeThickness = 2;
        }
        #endregion
        #region IUpdateableView Members

        public void update()
        {
            if (this.Model.Active)
            {
                if (ActiveColorBrush == null)
                {
                    Color ActiveColor = ColorHelper.GetLineColor(this.Model.ConnectionType);
                    ActiveColor.ScR = (ActiveColor.ScR * 2f < 255 ? ActiveColor.ScR * 2f : 255f);
                    ActiveColor.ScG = (ActiveColor.ScG * 2f < 255 ? ActiveColor.ScG * 2f : 255f);
                    ActiveColor.ScB = (ActiveColor.ScB * 2f < 255 ? ActiveColor.ScB * 2f : 255f);
                    ActiveColorBrush = new SolidColorBrush(ActiveColor);
                }
                Stroke = ActiveColorBrush;                
                StrokeThickness = 4;
            }
            else
            {
                reset();
            }
        }

        public void Rearrange()
        {
            HasManualModification = false;
            HasComputed = false;
            loaded = false;
            InvalidateVisual();
            InvalidateArrange();
            InvalidateMeasure();
            UpdateLayout();
        }

        #endregion
    }

    #region Custom Class
    public class FakeNode : QuadTreeLib.IHasRect
    {
        public System.Drawing.RectangleF Rectangle { get; set; }
        public ConnectorVisual Source { get; set; }
        public ConnectorVisual Target { get; set; }
        public InternalCryptoLineView LogicalParent { get; set; }
        public FromTo FromTo { get; set; }
    }

    public class LineUtil 
    {
        private static double baseoffset = 10;

        public static bool IsBetween(double min, double max, double between)
        {
            return min <= between && between <= max;
        }

        public static bool FindIntersection(Point StartPoint, Point EndPoint, Point StartPointSec, Point EndPointSec, out IntersectPoint intersectPoint)
        {
            if (StartPoint.X != EndPoint.X &&
                StartPoint.Y != EndPoint.Y)
            {
                intersectPoint = null;
                return false;
            }
            if (StartPointSec.X != EndPointSec.X &&
                StartPointSec.Y != EndPointSec.Y)
            {
                intersectPoint = null;
                return false;
            }

            // parallel, also overlapping case
            if (StartPoint.X == EndPoint.X && StartPointSec.X == EndPointSec.X ||
                StartPoint.Y == EndPoint.Y && StartPointSec.Y == EndPointSec.Y)
            {
                intersectPoint = null;
                return false;
            }
            else
            {
                // orthogonal but maybe not intersected
                Point up, down, left, right;
                if (StartPoint.X == EndPoint.X)
                {
                    up = StartPoint;
                    down = EndPoint;
                    left = StartPointSec;
                    right = EndPointSec;
                }
                else
                {
                    up = StartPointSec;
                    down = EndPointSec;
                    left = StartPoint;
                    right = EndPoint;
                }

                if (up.Y < down.Y)
                {
                    double swap = up.Y;
                    up.Y = down.Y;
                    down.Y = swap;
                }

                if (left.X > right.X)
                {
                    double swap = left.X;
                    left.X = right.X;
                    right.X = swap;
                }
                //check if is intersected at all
                if (IsBetween(down.Y, up.Y, left.Y) && IsBetween(left.X, right.X, up.X))
                {
                    if (up.Y == left.Y ||
                        down.Y == left.Y ||
                        left.X == up.X || right.X == up.X)
                    {
                        intersectPoint = new IntersectPoint(new Point(up.X, left.Y), IntersectPointMode.InnerIntersect);
                    }
                    else
                    {
                        intersectPoint = new IntersectPoint(new Point(up.X, left.Y), IntersectPointMode.NormalIntersect);
                    }
                    return true;
                }
                intersectPoint = null;
                return false;
            }
        }

        public static bool IsConnectionPossible(Point p1, Point p2, QuadTreeLib.QuadTree<WorkspaceManager.View.VisualComponents.CryptoLineView.FakeNode> quadTree)
        {
            if (p1.X != p2.X && p1.Y != p2.Y)
                throw new ArgumentException("only 90� allowed");

            System.Drawing.RectangleF queryRect;
            if (p1.Y != p2.Y)
            {
                Point up = p2.Y < p1.Y ? p2 : p1;
                Point down = p2.Y < p1.Y ? p1 : p2;

                queryRect = new System.Drawing.RectangleF((float)up.X, (float)up.Y, (float)1, (float)(down.Y - up.Y));
            }
            else
            {
                Point left = p2.X < p1.X ? p2 : p1;
                Point right = p2.X < p1.X ? p1 : p2;

                queryRect = new System.Drawing.RectangleF((float)left.X, (float)left.Y, (float)(right.X - left.X), (float)1);
            }
            var b = !quadTree.QueryAny(queryRect);
            return b;
        }

        public static bool PerformOrthogonalPointConnection(Node n1, Point p2, Node n3, List<Node> nodeList, QuadTreeLib.QuadTree<WorkspaceManager.View.VisualComponents.CryptoLineView.FakeNode> quadTreePlugins)
        {

            if (LineUtil.IsConnectionPossible(n1.Point, p2, quadTreePlugins) && LineUtil.IsConnectionPossible(n3.Point, p2, quadTreePlugins))
            {
                Node n2 = new Node() { Point = p2 };

                n2.Vertices.Add(n1);
                n2.Vertices.Add(n3);
                n1.Vertices.Add(n2);
                n3.Vertices.Add(n2);

                n1.HelpingPointConnectableVertices.Add(n3);
                n3.HelpingPointConnectableVertices.Add(n1);

                nodeList.Add(n2);
                return true;
            }

            return false;
        }

        public static bool PerformOrthogonalPointConnection(Node p1, Node p2, QuadTreeLib.QuadTree<WorkspaceManager.View.VisualComponents.CryptoLineView.FakeNode> quadTree)
        {
            if (IsConnectionPossible(p1.Point, p2.Point, quadTree))
            {
                p1.Vertices.Add(p2);
                p2.Vertices.Add(p1);
                return true;
            }
            return false;
        }

        public static Point Cheat42(Point EndPoint, ConnectorVisual EndPointSource, int flipper)
        {
            double xoffset = 0;
            double yoffset = 0;
            switch (EndPointSource.Orientation)
            {
                case ConnectorOrientation.East:
                    xoffset = baseoffset;
                    break;
                case ConnectorOrientation.West:
                    xoffset = -baseoffset;
                    break;
                case ConnectorOrientation.North:
                    yoffset = -baseoffset;
                    break;
                case ConnectorOrientation.South:
                    yoffset = baseoffset;
                    break;
            }
            return new Point(EndPoint.X + xoffset, EndPoint.Y + yoffset);
        }
    }
    #endregion
    #region Converter
    public class IsOneTrueBindingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = values.OfType<bool>();
            var b = val.Any(x => x == true);
            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsOneFalseBindingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = values.OfType<bool>();
            var b = val.Any(x => x == false);
            return b;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GeoTrimBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            var geo = (PathGeometry)value;
            var subGeo = geo.Clone();

            if (subGeo.Figures[0].Segments.Count <= 1)
                return subGeo;

            subGeo.Figures[0].StartPoint = ((LineSegment)subGeo.Figures[0].Segments[0]).Point;
            subGeo.Figures[0].Segments.RemoveAt(0);
            subGeo.Figures[0].Segments.RemoveAt(subGeo.Figures[0].Segments.Count - 1);

            return subGeo;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NegateBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
    #endregion
}
