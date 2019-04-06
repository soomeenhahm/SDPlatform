using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Plankton;
using PlanktonGh;

namespace SDPlatform.MeshGrowth
{
    public class MeshGrowthSystem
    {
        /// <credit>
        /// This class is written based on Long Nquyen's MeshGrowth script 
        /// shared and taught from the workshop at ICD, the University of Stuttgart 
        /// in April 2018.
        /// https://icd.uni-stuttgart.de/?p=22773
        /// 
        /// Modified by Soomeen Hahm (soomeenhahm.com)
        /// first modified version edited in Feb 2019.
        /// </credit>

        private PlanktonMesh ptMesh; //convension: private variable - lowerCase / public variable - UpperCase

        public bool Grow = false;
        public int MaxVertexCount;

        public bool UseRTree; //usually use 

        public double EdgeLengthConstraintWeight;
        public double CollisionDistance; //usually 1
        public double CollisionWeight;
        public double BendingResistanceWeight;

        private List<Vector3d> totalWeightedMoves;
        private List<double> totalWeights;
        public List<Circle> Repellers;

        public MeshGrowthSystem(Mesh startingMesh)
        {
            ptMesh = startingMesh.ToPlanktonMesh();
        }

        public void Update()
        {
            if (Grow) SplitAllLongEdges();

            totalWeightedMoves = new List<Vector3d>();
            totalWeights = new List<double>();

            for (int i = 0; i < ptMesh.Vertices.Count; i++)
            {
                totalWeightedMoves.Add(new Vector3d(0.0, 0.0, 0.0));
                totalWeights.Add(0.0);
            }

            if (UseRTree) ProcessCollisionUsingRTree();
            else ProcessCollision();

            ProcessBendingResistance();
            ProcessEdgeLengthConstraint();
            ProcessRepeller();

            UpdateVertexPositions();
        }

        private void SplitAllLongEdges()
        {
            int halfEdgeCount = ptMesh.Halfedges.Count;

            for (int i = 0; i < halfEdgeCount; i += 2)
            {
                if (ptMesh.Vertices.Count < MaxVertexCount &&
                    ptMesh.Halfedges.GetLength(i) > 0.99 * CollisionDistance)
                {
                    SplitEdge(i);
                }
            }
        }

        private void ProcessCollision()
        {
            int vertexCount = ptMesh.Vertices.Count;

            for (int i = 0; i < vertexCount; i++)
                for (int j = i + 1; j < vertexCount; j++) //!! j = i+1 - sorting
                {
                    Vector3d move = ptMesh.Vertices[j].ToPoint3d() - ptMesh.Vertices[i].ToPoint3d();
                    double currentDistance = move.Length;
                    if (currentDistance > CollisionDistance) continue; //if too far, skip this loop

                    move *= 0.5 * (currentDistance - CollisionDistance) / currentDistance;

                    totalWeightedMoves[i] += move * CollisionWeight;
                    totalWeightedMoves[j] -= move * CollisionWeight;
                    totalWeights[i] += CollisionWeight;
                    totalWeights[j] += CollisionWeight;
                }
        }

        private void ProcessCollisionUsingRTree()
        {
            RTree rTree = new RTree();

            for (int i = 0; i < ptMesh.Vertices.Count; i++)
                rTree.Insert(ptMesh.Vertices[i].ToPoint3d(), i);

            for (int i = 0; i < ptMesh.Vertices.Count; i++)
            {
                Point3d vI = ptMesh.Vertices[i].ToPoint3d();
                Sphere searchSphere = new Sphere(vI, CollisionDistance);

                List<int> collisionIndices = new List<int>();

                rTree.Search(
                    searchSphere,
                    (sender, args) => { if (i < args.Id) collisionIndices.Add(args.Id); }); //(sender, args) => { ... }

                foreach (int j in collisionIndices)
                {
                    Vector3d move = ptMesh.Vertices[j].ToPoint3d() - ptMesh.Vertices[i].ToPoint3d();
                    double currentDistance = move.Length;

                    move *= 0.5 * (currentDistance - CollisionDistance) / currentDistance;

                    totalWeightedMoves[i] += move * CollisionWeight;
                    totalWeightedMoves[j] -= move * CollisionWeight;
                    totalWeights[i] += CollisionWeight;
                    totalWeights[j] += CollisionWeight;
                }
            }
        }


        private void ProcessRepeller()
        {
            if (Repellers.Count > 0)
            {
                int vertexCount = ptMesh.Vertices.Count;

                for (int i = 0; i < vertexCount; i++)
                {
                    Point3d Loc = ptMesh.Vertices[i].ToPoint3d();
                    foreach (Circle repeller in Repellers)
                    {
                        double distanceToRepeller = Loc.DistanceTo(repeller.Center);
                        Vector3d repulsion = Loc - repeller.Center;
                        double repellerWeight = repeller.Radius;

                        if (repulsion.Length < repellerWeight)
                        {
                            totalWeightedMoves[i] += repulsion * repellerWeight;
                            totalWeights[i] += repellerWeight;

                        }
                    }
                }
            }
   
        }

        private void ProcessBendingResistance()
        {
            int halfEdgeCount = ptMesh.Halfedges.Count;

            for (int k = 0; k < halfEdgeCount; k += 2)
            {
                int i = ptMesh.Halfedges[k].StartVertex;
                int j = ptMesh.Halfedges[k + 1].StartVertex;
                int p = ptMesh.Halfedges[ptMesh.Halfedges[k].PrevHalfedge].StartVertex;
                int q = ptMesh.Halfedges[ptMesh.Halfedges[k + 1].PrevHalfedge].StartVertex;

                Point3d vI = ptMesh.Vertices[i].ToPoint3d();
                Point3d vJ = ptMesh.Vertices[j].ToPoint3d();
                Point3d vP = ptMesh.Vertices[p].ToPoint3d();
                Point3d vQ = ptMesh.Vertices[q].ToPoint3d();

                Vector3d nP = Vector3d.CrossProduct(vJ - vI, vP - vI);
                Vector3d nQ = Vector3d.CrossProduct(vQ - vI, vJ - vI);

                Vector3d planeNormal = nP + nQ; //Normalized automatically (?)
                Point3d planeOrigin = 0.25 * (vI + vJ + vP + vQ);

                Plane plane = new Plane(planeOrigin, planeNormal);

                totalWeightedMoves[i] += (plane.ClosestPoint(vI) - vI) * BendingResistanceWeight;
                totalWeightedMoves[j] += (plane.ClosestPoint(vJ) - vJ) * BendingResistanceWeight;
                totalWeightedMoves[p] += (plane.ClosestPoint(vP) - vP) * BendingResistanceWeight;
                totalWeightedMoves[q] += (plane.ClosestPoint(vQ) - vQ) * BendingResistanceWeight;

                totalWeights[i] += BendingResistanceWeight;
                totalWeights[j] += BendingResistanceWeight;
                totalWeights[p] += BendingResistanceWeight;
                totalWeights[q] += BendingResistanceWeight;
            }
        }

        private void ProcessEdgeLengthConstraint()
        {
            for (int k = 0; k < ptMesh.Halfedges.Count; k += 2)
            {
                int i = ptMesh.Halfedges[k].StartVertex;
                int j = ptMesh.Halfedges[k + 1].StartVertex;

                Point3d vI = ptMesh.Vertices[i].ToPoint3d();
                Point3d vJ = ptMesh.Vertices[j].ToPoint3d();

                if (vI.DistanceTo(vJ) < CollisionDistance) continue;

                Vector3d move = vJ - vI;
                move *= 0.5;

                totalWeightedMoves[i] += move * EdgeLengthConstraintWeight;
                totalWeightedMoves[j] -= move * EdgeLengthConstraintWeight;

                totalWeights[i] += EdgeLengthConstraintWeight;
                totalWeights[j] += EdgeLengthConstraintWeight;
            }
        }

        private void UpdateVertexPositions()
        {
            for (int i = 0; i < ptMesh.Vertices.Count; i++)
            {
                if (totalWeights[i] == 0.0) continue; //to avoid too much nested {}, prevent error for devising 0.0 on next line

                Vector3d move = totalWeightedMoves[i] / totalWeights[i];
                Point3d newPosition = ptMesh.Vertices[i].ToPoint3d() + move;
                ptMesh.Vertices.SetVertex(i, newPosition);
            }
        }

        private void SplitEdge(int edgeIndex)
        {
            int newHalfEdgeIndex = ptMesh.Halfedges.SplitEdge(edgeIndex);

            ptMesh.Vertices.SetVertex(
                ptMesh.Vertices.Count - 1,
                0.5 * (ptMesh.Vertices[ptMesh.Halfedges[edgeIndex].StartVertex].ToPoint3d() + ptMesh.Vertices[ptMesh.Halfedges[edgeIndex + 1].StartVertex].ToPoint3d()));

            if (ptMesh.Halfedges[edgeIndex].AdjacentFace >= 0)
                ptMesh.Faces.SplitFace(newHalfEdgeIndex, ptMesh.Halfedges[edgeIndex].PrevHalfedge);

            if (ptMesh.Halfedges[edgeIndex + 1].AdjacentFace >= 0)
                ptMesh.Faces.SplitFace(edgeIndex + 1, ptMesh.Halfedges[ptMesh.Halfedges[edgeIndex + 1].NextHalfedge].NextHalfedge);
        }

        public Mesh GetRhinoMesh()
        {
            return ptMesh.ToRhinoMesh();
        }


    }
}