using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;


namespace SDPlatform.Utilities
{
    public class GhcProjectPtsToMesh : GH_Component
    {

        public GhcProjectPtsToMesh()
          : base("Project Pts To Mesh", "Project Pts",
              "Project points to a target mesh",
              "SDPlatform", "Utilities")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
            pManager.AddMeshParameter("TargetMesh", "TargetMesh", "TargetMesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "Direction", "Direction", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("ProjectedPoints", "ProjectedPoints", "ProjectedPoints", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> iPoints = new List<Point3d>();
            Mesh iTargetMesh = null;
            Vector3d iDirection = new Vector3d();

            DA.GetDataList("Points", iPoints);
            DA.GetData("TargetMesh", ref iTargetMesh);
            DA.GetData("Direction", ref iDirection);
            if (iDirection.IsTiny()) iDirection = new Vector3d(0, 0, 1);

            int[] indices;
            var prj_points = Intersection.ProjectPointsToMeshesEx(new[] { iTargetMesh }, iPoints.AsEnumerable(), iDirection, 0, out indices);

            DA.SetDataList("ProjectedPoints", prj_points.ToList());
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.pp;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3432d8f2-b13e-457a-9faf-fb2f42396c3a"); }
        }
    }
}