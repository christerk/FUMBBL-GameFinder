using Fumbbl.Gamefinder.Model;
using Fumbbl.Gamefinder.Model.Event;
using GamefinderVisualizer.Models;
using GraphX.Common.Enums;
using GraphX.Controls;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GamefinderVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MatchGraph _graph;
        private GamefinderGraph graph = new GamefinderGraph();
        private object _lockObj = new object();
        Dictionary<Team, DataVertex> tLookup = new();
        Dictionary<Coach, DataVertex> cLookup = new();
        Dictionary<Match, DataEdge> mLookup = new();
        private Rect viewportRect = new Rect(-400, -400, 800, 800);


        public MainWindow()
        {
            GamefinderModel gameFinder = new();
            _graph = new(gameFinder);
            _graph.CoachAdded += _graph_CoachAdded;
            _graph.CoachRemoved += _graph_CoachRemoved;
            _graph.TeamAdded += _graph_TeamAdded;
            _graph.TeamRemoved += _graph_TeamRemoved;
            _graph.MatchAdded += _graph_MatchAdded;
            _graph.MatchRemoved += _graph_MatchRemoved;
            _graph.UpdateComplete += _graph_UpdateComplete;

            InitializeComponent();

            ZoomControl.SetViewFinderVisibility(zoomctrl, Visibility.Hidden);

            zoomctrl.ZoomToContent(viewportRect);

            InitializeGamefinder();
            InitializeGraph();

            Loaded += Window_Loaded;
        }

        private void _graph_UpdateComplete(object? sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Area.GenerateGraph(true, true);
                Area.ShowAllEdgesLabels(false);
                zoomctrl.ZoomToContent(viewportRect);
            });
        }

        private void _graph_MatchRemoved(object? sender, EventArgs e)
        {
            Match? match = ((MatchUpdatedArgs)e).Match;

            if (match is not null && mLookup.ContainsKey(match))
            {
                graph.RemoveEdge(mLookup[match]);
            }
        }

        private void _graph_MatchAdded(object? sender, EventArgs e)
        {
            Match? match = ((MatchUpdatedArgs)e).Match;

            if (match is not null)
            {
                (var t1, var t2) = (match.Team1, match.Team2);
                var edge = new DataEdge(tLookup[t1], tLookup[t2]) { IsMatch = true, Match = match };
                graph.AddEdge(edge);
            }
        }

        private void _graph_TeamRemoved(object? sender, EventArgs e)
        {
            Team? team = ((TeamUpdatedArgs)e).Team;

            if (team is not null)
            {
                graph.RemoveVertex(tLookup[team]);
            }
        }

        private void _graph_TeamAdded(object? sender, EventArgs e)
        {
            Team? team = ((TeamUpdatedArgs)e).Team;

            if (team is not null)
            {
                var vertex = new DataVertex() { SortId = team.Id, VType = DataVertex.VertexType.Team, Label = team.Name };
                vertex.GroupId = team.Coach.Id;
                tLookup.Add(team, vertex);
                graph.AddVertex(vertex);

                var cVertex = cLookup[team.Coach];
                cVertex.SortId = team.Id;
                var edge = new DataEdge(cVertex, vertex) { IsMatch = false, State = "", Match = null };
                graph.AddEdge(edge);
            }

        }

        private void _graph_CoachRemoved(object? sender, EventArgs e)
        {
            Coach? coach = ((CoachUpdatedArgs)e).Coach;

            if (coach is not null && cLookup.ContainsKey(coach)) {
                graph.RemoveVertex(cLookup[coach]);
                coaches.Remove(coach);
                cLookup.Remove(coach);
            }
        }

        private void _graph_CoachAdded(object? sender, EventArgs e)
        {
            Coach? coach = ((CoachUpdatedArgs)e).Coach;

            if (coach is not null)
            {
                var vertex = new DataVertex() { SortId = 0, VType = DataVertex.VertexType.Coach, Coach = coach, Label = coach.Name };
                coaches.Add(coach);
                graph.AddVertex(vertex);
                cLookup.Add(coach, vertex);
            }

        }

        List<Coach> coaches = new();
        Random r = new Random();
        int cNum = 0, tNum = 0;

        private void Simulate()
        {
            var maxCoaches = 7;
            var addCoach = coaches.Count < maxCoaches || r.Next(0,20) == 0;
            if (addCoach)
            {
                cNum++;
                Coach c = new Coach() { Id = cNum, Name = $"Coach {cNum}" };
                _graph.AddCoach(c);

                var numTeams = r.Next(3, 6) / 2;
                for (var i = 0; i < numTeams; i++)
                {
                    tNum++;
                    Team t = new Team(_graph, c) { Id = tNum, Name = $"Team {tNum}" };
                    _graph.AddTeam(t);
                }
            }

            if (coaches.Count > maxCoaches)
            {
                var removedCoach = coaches.First();
                _graph.RemoveCoach(removedCoach);
            }

            foreach (var c in coaches.ToArray())
            {
                var matches = _graph.GetMatches(c).ToList();
                if (matches.Count > 0)
                {
                    var launchedMatch = matches.FirstOrDefault(m => m?.MatchState?.TriggerLaunchGame ?? false);

                    if (launchedMatch != null)
                    {
                        continue;
                    }

                    var match = matches[r.Next(matches.Count)];
                    if (!match.MatchState.IsHidden)
                    {
                        var ownTeam = match.Team1.Coach.Equals(c) ? match.Team1 : match.Team2;

                        if (!match.MatchState.IsDefault && r.Next(20) == 0)
                        {
                            match.Act(TeamAction.Cancel, ownTeam);
                        }
                        else
                        {
                            if (match.MatchState.TriggerStartDialog)
                            {
                                match.Act(TeamAction.Start, ownTeam);
                            }
                            else
                            {
                                match.Act(TeamAction.Accept, ownTeam);
                            }
                        }
                    }
                }
                c.Ping();
            }
        }

        private void InitializeGamefinder()
        {
            _graph.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Area.GenerateGraph(true, true);
            Area.ShowAllEdgesLabels(false);

            Task.Run(() =>
            {
                while(true)
                {
                    Thread.Sleep(1000);
                    Simulate();
                }
            });
        }

        private void InitializeGraph()
        {
            var logicCore = new GamefinderGraphLogicCore() { Graph = graph };

            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Circular;
            logicCore.DefaultLayoutAlgorithmParams = logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.Circular);
            
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams.HorizontalGap = 300;
            logicCore.DefaultOverlapRemovalAlgorithmParams.VerticalGap = 300;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.AsyncAlgorithmCompute = false;

            var vertexPositions = new Dictionary<DataVertex, GraphX.Measure.Point>();
            var vertexSizes = new Dictionary<DataVertex, GraphX.Measure.Size>();
            var parameters = new GamefinderLayoutParameters();
            logicCore.ExternalLayoutAlgorithm = new GamefinderLayout(graph, vertexPositions, vertexSizes, parameters);

            Area.LogicCore = logicCore;
            Area.ShowAllEdgesLabels(false);
        }

        private void Relayout_Click(object sender, RoutedEventArgs e)
        {
            Area.RelayoutGraph();
            zoomctrl.ZoomToContent(viewportRect);
        }

        private void Randomgraph_Click(object sender, RoutedEventArgs e)
        {
            Area.GenerateGraph(true, true);
            zoomctrl.ZoomToContent(viewportRect);
        }
    }
}
