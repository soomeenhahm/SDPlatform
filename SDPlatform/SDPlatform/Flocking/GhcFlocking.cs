using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SDPlatform.Flocking
{
    public class GhcFlocking : GH_Component
    {
        /// <credit>
        /// This class is written based on Long Nquyen's FlockSystem script 
        /// shared and taught from the workshop at ICD, the University of Stuttgart 
        /// in March 2017.
        /// https://icd.uni-stuttgart.de
        /// 
        /// Modified by Soomeen Hahm (soomeenhahm.com)
        /// first modified version edited in Feb 2019.
        /// </credit>

        private FlockSystem flockSystem;

        public GhcFlocking()
          : base("Flocking Simulation", "Flocking",
              "Flocking imulation",
              "SDPlatform", "Simulation")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "Play", "Play", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("3D", "3D", "3D", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Count", "Count", "Count", GH_ParamAccess.item, 500);
            pManager.AddNumberParameter("TimeStep", "TimeStep", "TimeStep", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("NeighbourRadius", "Neighbouradius", "NeighbourRadius", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Align", "Align", "Align", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Cohesion", "Cohesion", "Cohesion", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separate", "Separate", "Separate", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Sep Dist", "Sep Dist", "Sep Dist", GH_ParamAccess.item, 5.0);
            pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            pManager.AddCircleParameter("Attractors", "Attractors", "Attractors", GH_ParamAccess.list);
            //pManager[10].Optional = true;
            pManager.AddBooleanParameter("Parallel", "Parallel", "Parallel", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("R-Tree", "R-Tree", "R-Tree", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Locations", "Locations", "The agent locations", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent velocities", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool iReset = true;
            bool iPlay = false;
            bool i3D = false;
            int iCount = 0;
            double iTimeStep = 0.0;
            double iNeighbourRadius = 0.0;
            double iAlg = 0.0;
            double iCoh = 0.0;
            double iSep = 0.0;
            double iSepDist = 0.0;
            List<Circle> iRepellers = new List<Circle>();
            List<Circle> iAttractors = new List<Circle>();
            bool iUseParallel = false;
            bool iUseRTree = false;

            DA.GetData("Reset", ref iReset);
            DA.GetData("Play", ref iPlay);
            DA.GetData("3D", ref i3D);
            DA.GetData("Count", ref iCount);
            DA.GetData("TimeStep", ref iTimeStep);
            DA.GetData("NeighbourRadius", ref iNeighbourRadius);
            DA.GetData("Align", ref iAlg);
            DA.GetData("Cohesion", ref iCoh);
            DA.GetData("Separate", ref iSep);
            DA.GetData("Sep Dist", ref iSepDist);
            DA.GetDataList("Repellers", iRepellers);
            DA.GetDataList("Attractors", iAttractors);
            DA.GetData("Parallel", ref iUseParallel);
            DA.GetData("R-Tree", ref iUseRTree);

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(iCount, i3D);

            }
            else
            {
                flockSystem.Timestep = iTimeStep;
                flockSystem.NeighbourRadius = iNeighbourRadius;
                flockSystem.AlgStr = iAlg;
                flockSystem.CohStr = iCoh;
                flockSystem.SepStr = iSep;
                flockSystem.SepDis = iSepDist;
                flockSystem.Repellers = iRepellers;
                flockSystem.Attractors = iAttractors;
                flockSystem.UseParallel = iUseParallel;
                flockSystem.UseRTree = iUseRTree;

                if (iUseRTree)
                    flockSystem.UpdateUsingRTree();
                else
                    flockSystem.Update();

                if (iPlay) ExpireSolution(true);

            }

            //List<Point3d> positions = new List<Point3d>(); //Slower
            List<GH_Point> positions = new List<GH_Point>(); //Grasshopper.Kernel.Types; required
            //List<Vector3d> velocities = new List<Vector3d>(); 
            List<GH_Vector> velocities = new List<GH_Vector>(); //using GH objects will be faster

            foreach (FlockAgent agent in flockSystem.Agents)
            {
                //positions.Add(new Point3d(agent.Loc)); //Using GH_Point rather than Point3d will be faster
                positions.Add(new GH_Point(agent.Loc));
                //velocities.Add(new Vector3d(agent.Vel)); //same
                velocities.Add(new GH_Vector(agent.Vel));
            }

            DA.SetDataList("Locations", positions);
            DA.SetDataList("Velocities", velocities);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.FL;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c7d8a1ed-b18f-4c13-a89c-3238aefe6505"); }
        }
    }
}