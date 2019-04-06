using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace SDPlatform.Utilities
{
    public class GhcProjectToMesh : GH_Component
    {

        public GhcProjectToMesh()
          : base("Project Mesh To Mesh", "Project Mesh",
              "Project a mesh to a target mesh",
              "SDPlatform", "Utilities")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("NewMesh", "NewMesh", "NewMesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("TargetMesh", "TargetMesh", "TargetMesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "Direction", "Direction", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("ProjectedMesh", "ProjectedMesh", "ProjectedMesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh iNewMesh= null;
            Mesh iTargetMesh = null;
            Vector3d iDirection = new Vector3d(); 
            //Mesh iProjectedMesh = null;

            DA.GetData("NewMesh", ref iNewMesh);
            DA.GetData("TargetMesh", ref iTargetMesh);
            DA.GetData("Direction", ref iDirection);
            if (iDirection.IsTiny()) iDirection = new Vector3d(0, 0, 1);

            //Option 1
            //ConvertPoint3fListToPoint3dEnumerable(); 

            //Option2
            //Step1: converts all contents (from Point3f List to Point3d list)
            //Step2: "allPoint3d.AsEnumerable()" to convert List to Enumerable

            //Option3
            var allPoint3d = iNewMesh.Vertices.Select(p => new Point3d(p.X, p.Y, p.Z)); //Need System.Linq

            int[] indices;
            var prj_points = Intersection.ProjectPointsToMeshesEx(new[] { iTargetMesh }, allPoint3d.AsEnumerable(), iDirection, 0, out indices);
            for (int i = 0; i < prj_points.Length; i++)
                iNewMesh.Vertices.SetVertex(i, prj_points[i]);

            DA.SetData("ProjectedMesh", iNewMesh);
        }

        public Point3d[] ConvertPoint3fListToPoint3dEnumerable(Mesh iNewMesh)
        {
            Point3d[] allVertices = new Point3d[iNewMesh.Vertices.Count];
            int n = 0;
            foreach (Point3d p in iNewMesh.Vertices)
            {
                allVertices[n] = p;
                n++;
            }

            return allVertices;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.PM;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("bdb2f340-5d1e-4d5e-9a6d-af4b38254a14"); }
        }
    }
}