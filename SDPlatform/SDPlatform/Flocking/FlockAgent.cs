using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SDPlatform.Flocking
{
    public class FlockAgent
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

        public Point3d Loc;
        public Vector3d Vel;

        private Vector3d desiredVel; //acc 

        public FlockSystem FlockSystem;

        public FlockAgent(Point3d loc, Vector3d vel)
        {
            Loc = loc;
            Vel = vel;
        }

        public void Update()
        {
            Vel = 0.97 * Vel + 0.03 * desiredVel;

            //To limit min/max speed
            if (Vel.Length > 8.0) Vel *= 8.0 / Vel.Length; //8.0 can be a external parameter (coming from GH)
            else if (Vel.Length < 4.0) Vel *= 4.0 / Vel.Length; //4.0 can be a external parameter

            Loc += Vel * FlockSystem.Timestep;
        }

        public void ComputeDesiredVelocity(List<FlockAgent> neighbours)
        {
            desiredVel = new Vector3d(0.0, 0.0, 0.0);

            double boundingBoxSize = FlockSystem.BoundingBoxSize;

            if (Loc.X < 0.0)
                desiredVel += new Vector3d(-Loc.X, 0.0, 0.0);
            else if (Loc.X > boundingBoxSize)
                desiredVel += new Vector3d(boundingBoxSize - Loc.X, 0.0, 0.0);

            if (Loc.Y < 0.0)
                desiredVel += new Vector3d(0.0, -Loc.Y, 0.0);
            else if (Loc.Y > boundingBoxSize)
                desiredVel += new Vector3d(0.0, boundingBoxSize - Loc.Y, 0.0);

            if (Loc.Z < 0.0)
                desiredVel += new Vector3d(0.0, 0.0, -Loc.Z);
            else if (Loc.Z > boundingBoxSize)
                desiredVel += new Vector3d(0.0, 0.0, boundingBoxSize - Loc.Z);

            if (neighbours.Count == 0)
                desiredVel += Vel; //maintain the current velocity
            else
            {
                //Alighment behaviour --------------------------------------------
                Vector3d alignment = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
                    alignment += neighbour.Vel;

                alignment /= neighbours.Count;
                desiredVel += FlockSystem.AlgStr * alignment;

                //Cohesion behaviour --------------------------------------------
                Point3d centre = Point3d.Origin;

                foreach (FlockAgent neighbour in neighbours)
                    centre += neighbour.Loc;

                centre /= neighbours.Count;
                Vector3d cohesion = centre - Loc;
                desiredVel += FlockSystem.CohStr * cohesion;

                //Seperation behaviour ------------------------------------------
                Vector3d separation = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
                {
                    double distanceToNeighbour = Loc.DistanceTo(neighbour.Loc);
                    if (distanceToNeighbour < FlockSystem.SepDis)
                    {
                        Vector3d getAway = Loc - neighbour.Loc;
                        separation += getAway / (getAway.Length * distanceToNeighbour); //is it same as "getAway / (getAway.Length * getAway.Length)" ?
                    }
                }
                desiredVel += FlockSystem.SepStr * separation;

                //Avoiding Repellers --------------------------------------------
                foreach (Circle repeller in FlockSystem.Repellers)
                {
                    double distanceToRepeller = Loc.DistanceTo(repeller.Center);
                    Vector3d repulsion = Loc - repeller.Center;
                    repulsion /= (repulsion.Length * distanceToRepeller);
                    repulsion *= 30.0 * repeller.Radius;
                    desiredVel += repulsion;
                }

                //Attractors  ----------------------------------------------------
                foreach (Circle attractor in FlockSystem.Attractors)
                {
                    double distanceToRepeller = Loc.DistanceTo(attractor.Center);
                    Vector3d repulsion = Loc - attractor.Center;
                    repulsion /= (repulsion.Length * distanceToRepeller);
                    repulsion *= 30.0 * attractor.Radius;
                    desiredVel -= repulsion;
                }

                //----------------------------------------------------------------
            }
        }


    }
}