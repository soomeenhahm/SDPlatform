using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SDPlatform.MeshGrowth
{
    public class GhcMeshGrowth : GH_Component
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

        private MeshGrowthSystem myMeshGrowthSystem;

        public GhcMeshGrowth()
          : base("Differential Growth", "Diff Growth",
              "Differential mesh growth simulation",
              "SDPlatform", "Simulation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item);
            pManager.AddMeshParameter("Starting Mesh", "StartingMesh", "StartingMesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Subiteration Count", "Subiteration Count", "Subiteration Count", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Grow", "Grow", "Grow", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Max. Vertex Count", "Max. Vertex Count", "Max. Vertex Count", GH_ParamAccess.item);
            pManager.AddNumberParameter("Edge Length Constraint Weight", "Edge Length Constraint Weight", "Edge Length Constraint Weight", GH_ParamAccess.item);
            pManager.AddNumberParameter("Collision Distance", "Collision Distance", "Collision Distance", GH_ParamAccess.item);
            pManager.AddNumberParameter("Collision Weight", "Collision Weight", "Collision Weight", GH_ParamAccess.item);
            pManager.AddNumberParameter("Bending Resistance Weight", "Bending Resistance Weight", "Bending Resistance Weight", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Use R-Tree", "Use R-Tree", "Use R-Tree", GH_ParamAccess.item);
            pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = true;
            Mesh iStartingMesh = null;
            int iSubiterationCount = 0;
            bool iGrow = false;
            int iMaxVertexCount = 0;
            double iEdgeLengthConstrainWeight = 0.0;
            double iCollisionDistance = 0.0;
            double iCollisionWeight = 0.0;
            double iBendingResistanceWeight = 0.0;
            bool iUseRTree = true;
            List<Circle> iRepellers = new List<Circle>();

            DA.GetData("Reset", ref iReset);
            DA.GetData("Starting Mesh", ref iStartingMesh);
            DA.GetData("Subiteration Count", ref iSubiterationCount);
            DA.GetData("Grow", ref iGrow);
            DA.GetData("Max. Vertex Count", ref iMaxVertexCount);
            DA.GetData("Edge Length Constraint Weight", ref iEdgeLengthConstrainWeight);
            DA.GetData("Collision Distance", ref iCollisionDistance);
            DA.GetData("Collision Weight", ref iCollisionWeight);
            DA.GetData("Bending Resistance Weight", ref iBendingResistanceWeight);
            DA.GetData("Use R-Tree", ref iUseRTree);
            DA.GetDataList("Repellers", iRepellers);

            if (iReset || myMeshGrowthSystem == null) myMeshGrowthSystem = new MeshGrowthSystem(iStartingMesh); //to avoid red error in the start of script

            myMeshGrowthSystem.Grow = iGrow;
            myMeshGrowthSystem.MaxVertexCount = iMaxVertexCount;
            myMeshGrowthSystem.EdgeLengthConstraintWeight = iEdgeLengthConstrainWeight;
            myMeshGrowthSystem.CollisionWeight = iCollisionWeight;
            myMeshGrowthSystem.BendingResistanceWeight = iBendingResistanceWeight;
            myMeshGrowthSystem.UseRTree = iUseRTree;
            myMeshGrowthSystem.CollisionDistance = iCollisionDistance; //This happens every frame live
            myMeshGrowthSystem.Repellers = iRepellers;

            myMeshGrowthSystem.Update();
            DA.SetData("Mesh", myMeshGrowthSystem.GetRhinoMesh());
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.DG;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("733a0ed8-ae05-48f5-a808-032aefb2711b"); }
        }
    }
}