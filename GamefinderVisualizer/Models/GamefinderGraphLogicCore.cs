using GraphX.Common.Enums;
using GraphX.Common.Exceptions;
using GraphX.Common.Interfaces;
using GraphX.Logic.Models;
using GraphX.Measure;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GamefinderVisualizer.Models
{
    public class GamefinderGraphLogicCore : IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>
    {
        private readonly GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> _coreLogicCore;

        public GamefinderGraphLogicCore()
        {
            _coreLogicCore = new GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>();
        }

        public IDictionary<DataVertex, Point> Compute(CancellationToken cancellationToken)
        {
            // Simplified variant of the core class implementation
            // Primarily to remove Debug.WriteLine calls
            if (_coreLogicCore.Graph == null)
                throw new GX_InvalidDataException("LogicCore -> Graph property not set!");

            IDictionary<DataVertex, Point> resultCoords;

            if (AlgorithmStorage.Layout != null)
            {
                AlgorithmStorage.Layout.Compute(cancellationToken);
                resultCoords = AlgorithmStorage.Layout.VertexPositions;
                return resultCoords;
            }
            else
                throw new Exception("No Layout Algorithm");
        }

        #region Passthrough
        public bool GenerateAlgorithmStorage(Dictionary<DataVertex, Size> vertexSizes,
            IDictionary<DataVertex, Point> vertexPositions)
            => _coreLogicCore.GenerateAlgorithmStorage(vertexSizes, vertexPositions);

        public IAlgorithmFactory<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> AlgorithmFactory => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AlgorithmFactory;

        public IAlgorithmStorage<DataVertex, DataEdge> AlgorithmStorage { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AlgorithmStorage; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AlgorithmStorage = value; }
        public BidirectionalGraph<DataVertex, DataEdge> Graph { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).Graph; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).Graph = value; }

        public bool IsFiltered => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).IsFiltered;

        public bool IsFilterRemoved => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).IsFilterRemoved;

        public bool AsyncAlgorithmCompute { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AsyncAlgorithmCompute; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AsyncAlgorithmCompute = value; }
        public bool EdgeCurvingEnabled { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EdgeCurvingEnabled; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EdgeCurvingEnabled = value; }
        public double EdgeCurvingTolerance { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EdgeCurvingTolerance; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EdgeCurvingTolerance = value; }
        public bool EnableParallelEdges { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EnableParallelEdges; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).EnableParallelEdges = value; }
        public int ParallelEdgeDistance { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ParallelEdgeDistance; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ParallelEdgeDistance = value; }

        public bool IsEdgeRoutingEnabled => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).IsEdgeRoutingEnabled;

        public bool IsCustomLayout => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).IsCustomLayout;

        public LayoutAlgorithmTypeEnum DefaultLayoutAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultLayoutAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultLayoutAlgorithm = value; }
        public OverlapRemovalAlgorithmTypeEnum DefaultOverlapRemovalAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultOverlapRemovalAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultOverlapRemovalAlgorithm = value; }
        public EdgeRoutingAlgorithmTypeEnum DefaultEdgeRoutingAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultEdgeRoutingAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultEdgeRoutingAlgorithm = value; }
        public ILayoutParameters DefaultLayoutAlgorithmParams { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultLayoutAlgorithmParams; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultLayoutAlgorithmParams = value; }
        public IOverlapRemovalParameters DefaultOverlapRemovalAlgorithmParams { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultOverlapRemovalAlgorithmParams; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultOverlapRemovalAlgorithmParams = value; }
        public IEdgeRoutingParameters DefaultEdgeRoutingAlgorithmParams { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultEdgeRoutingAlgorithmParams; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).DefaultEdgeRoutingAlgorithmParams = value; }
        public IExternalLayout<DataVertex, DataEdge> ExternalLayoutAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalLayoutAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalLayoutAlgorithm = value; }
        public IExternalOverlapRemoval<DataVertex> ExternalOverlapRemovalAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalOverlapRemovalAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalOverlapRemovalAlgorithm = value; }
        public IExternalEdgeRouting<DataVertex, DataEdge> ExternalEdgeRoutingAlgorithm { get => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalEdgeRoutingAlgorithm; set => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ExternalEdgeRoutingAlgorithm = value; }

        public Queue<IGraphFilter<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>> Filters => ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).Filters;

        public void ComputeEdgeRoutesByVertex(DataVertex dataVertex, Point? vertexPosition = null, Size? vertexSize = null)
        {
            ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ComputeEdgeRoutesByVertex(dataVertex, vertexPosition, vertexSize);
        }

        public bool AreVertexSizesNeeded()
        {
            return ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AreVertexSizesNeeded();
        }

        public bool AreOverlapNeeded()
        {
            return ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).AreOverlapNeeded();
        }

        public IExternalLayout<DataVertex, DataEdge> GenerateLayoutAlgorithm(Dictionary<DataVertex, Size> vertexSizes, IDictionary<DataVertex, Point> vertexPositions)
        {
            return ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).GenerateLayoutAlgorithm(vertexSizes, vertexPositions);
        }

        public IExternalOverlapRemoval<DataVertex> GenerateOverlapRemovalAlgorithm(Dictionary<DataVertex, Rect> rectangles)
        {
            return ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).GenerateOverlapRemovalAlgorithm(rectangles);
        }

        public IExternalEdgeRouting<DataVertex, DataEdge> GenerateEdgeRoutingAlgorithm(Size desiredSize, IDictionary<DataVertex, Point> vertexPositions, IDictionary<DataVertex, Rect> rectangles)
        {
            return ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).GenerateEdgeRoutingAlgorithm(desiredSize, vertexPositions, rectangles);
        }

        public void Clear(bool clearStorages = true)
        {
            ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).Clear(clearStorages);
        }

        public void PushFilters()
        {
            ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).PushFilters();
        }

        public void ApplyFilters()
        {
            ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).ApplyFilters();
        }

        public void PopFilters()
        {
            ((IGXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>)_coreLogicCore).PopFilters();
        }

        public void Dispose()
        {
            ((IDisposable)_coreLogicCore).Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
