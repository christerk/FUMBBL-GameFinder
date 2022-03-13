using GamefinderVisualizer.Models;
using GraphX.Common.Enums;
using GraphX.Common.Interfaces;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Measure;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GamefinderVisualizer
{
    internal class GamefinderLayout : DefaultParameterizedLayoutAlgorithmBase<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>, GamefinderLayoutParameters>
    {
        readonly IDictionary<DataVertex, Size> _sizes;

        public GamefinderLayout(BidirectionalGraph<DataVertex, DataEdge> visitedGraph, IDictionary<DataVertex, Point> vertexPositions, IDictionary<DataVertex, Size> vertexSizes, GamefinderLayoutParameters parameters)
            : base(visitedGraph, vertexPositions, parameters)
        {
            _sizes = vertexSizes;
        }

        public override bool SupportsObjectFreeze => false;

        public override void Compute(CancellationToken cancellationToken)
        {
            //calculate the size of the circle
            var usableVertices = VisitedGraph.Vertices.Where(v => v.SkipProcessing != ProcessingOptionEnum.Freeze).ToList();
            usableVertices.Sort((a, b) =>
            {
                var ret = a.SortId - b.SortId;
                if (ret != 0)
                {
                    return ret;
                }

                return b.VType - a.VType;
            });

            var numTeams = usableVertices.Count(v => v.VType == DataVertex.VertexType.Team);

            double teamRadius = 200;
            double coachRadius = teamRadius + 100;

            //
            //precalculation
            //
            double spacing = 2 * Math.PI / (numTeams);
            double angle = 0;
            int prevId = 0;
            int sameIdCount = 0;
            foreach (var v in usableVertices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (v.VType == DataVertex.VertexType.Team)
                {
                    //if ( ReportOnIterationEndNeeded )
                    VertexPositions[v] = new Point(Math.Cos(angle) * teamRadius, Math.Sin(angle) * teamRadius);

                    if (prevId != v.GroupId)
                    {
                        sameIdCount = 0;
                        prevId = v.GroupId;
                    }
                    sameIdCount++;
                }
                else
                {
                    var coachAngle = angle - spacing - (sameIdCount - 1) * spacing / 2.0;
                    VertexPositions[v] = new Point(Math.Cos(coachAngle) * coachRadius, Math.Sin(coachAngle) * coachRadius);

                    angle -= spacing;
                }

                angle += spacing;
            }
        }

        public override void ResetGraph(IEnumerable<DataVertex> vertices, IEnumerable<DataEdge> edges)
        {
        }
    }
}