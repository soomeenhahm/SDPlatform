using System;
using System.Collections.Generic;

using System.Threading.Tasks; //For Parallel

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SDPlatform.Flocking
{
    public class FlockSystem
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
        
        public List<FlockAgent> Agents;

        public double Timestep;
        public double NeighbourRadius;
        public double FieldOfView;
        public double AlgStr;
        public double CohStr;
        public double SepStr;
        public double SepDis;
        public double MinSpeed;
        public double MaxSpeed;
        public List<Circle> Repellers;
        public List<Circle> Attractors;
        public bool UseParallel;
        public bool UseRTree;
        public bool IUseFlock;

        public double BoundingBoxSize;

        public FlockSystem(int agentCount, bool is3D)
        {
            BoundingBoxSize = 30.0;

            Agents = new List<FlockAgent>();

            if (is3D)
            {
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(Util.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 30.0), Util.GetRandomUnitVector() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
            }
            else
            {
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(Util.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 0.0), Util.GetRandomUnitVectorXY() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
            }
        }

        public void Update()
        {
            if (!UseParallel)
                foreach (FlockAgent agent in Agents)
                {
                    List<FlockAgent> neighbours = FindNeighbours(agent);
                    agent.ComputeDesiredVelocity(neighbours);
                }
            else
                //Parallel Computing Syntax 01----------------------------------------------------------------------------------------
                //Parallel.ForEach(Agents, ComputeAllAgentDesiredVelocity); //ForEach (a list of object, an action with one object) //(a List, a Deligate)

                //Parallel Computing Syntax 02----------------------------------------------------------------------------------------
                //Alternative ways to write aboved line (anonymous function defined in-place without a name)              
                Parallel.ForEach(Agents, (FlockAgent agent) => //Parallel.ForEach(Agents, agent =>  //as no need to define the type again since ForEach needs same type input values
                {
                    List<FlockAgent> neighbours = FindNeighbours(agent);
                    agent.ComputeDesiredVelocity(neighbours);
                });

            foreach (FlockAgent agent in Agents) agent.Update();
        }

        private List<FlockAgent> FindNeighbours(FlockAgent agent)
        {
            List<FlockAgent> neighbours = new List<FlockAgent>();

            foreach (FlockAgent neighbour in Agents)
                if (neighbour != agent && neighbour.Loc.DistanceTo(agent.Loc) < NeighbourRadius)
                    neighbours.Add(neighbour);

            return neighbours;
        }

        public void UpdateUsingRTree()
        {
            //Other Spacial Data Structures: Spatial Grid(Not Suitable for low density point cloud), Octree(2D)/Quadtree(3D), KDTree(fastest), RTree(used often on maps)
            //Current RTree does not support parallel in GH but in theory can be combined with parallel and will be faster

            RTree rTree = new RTree();

            for (int i = 0; i < Agents.Count; i++)
                rTree.Insert(Agents[i].Loc, i);

            foreach (FlockAgent agent in Agents)
            {
                List<FlockAgent> neighbours = new List<FlockAgent>();

                //Declare and initiate a deligate (because "Search" function need this type of deligate)---------------------
                EventHandler<RTreeEventArgs> rTreeCallBack =
                (object sender, RTreeEventArgs args) =>
                {
                    if (Agents[args.Id] != agent)
                        neighbours.Add(Agents[args.Id]);
                };
                //-----------------------------------------------------------------------------------------------------------

                rTree.Search(new Sphere(agent.Loc, NeighbourRadius), rTreeCallBack); //rTreeCallBack is a function = deligate 
                agent.ComputeDesiredVelocity(neighbours);
            }

            foreach (FlockAgent agent in Agents) agent.Update();
        }

    }
}