/*
If you use or adapt this software in your research please consult
the author at afonso.goncalves@m-iti.org on how to cite it.

Copyright (C) 2017  Afonso Gonçalves 

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

using System;
using System.Linq;
using UnityEngine;
using Kinect = Windows.Kinect;

namespace VRKave
{
    public class VRBodyJoints : MonoBehaviour
    {

        public GameObject[] Joint = new GameObject[1];
        public GameObject BodySourceManager;
        private VRKinectBodySource _bodyManager;

        void Start()
        {
            String[] arguments = Environment.GetCommandLineArgs();

            for (int n = 1; n < arguments.Length; n++)
            {
                switch (arguments[n])
                {
                    case ("-arm"):
                        //Todo: Some behaviour
                        break;
                }
            }
        }

        void Update()
        {
            if (BodySourceManager == null)
            {
                return;
            }

            _bodyManager = BodySourceManager.GetComponent<VRKinectBodySource>();
            if (_bodyManager == null)
            {
                return;
            }

            Kinect.Body[] data = _bodyManager.GetData();
            if (data != null)
            {
                var body = ClosestBody(data);
                if (body != null)
                {
                    if (body.IsTracked)
                    {
                        Joint[0].transform.localPosition = GetVector3FromJoint(body.Joints[Kinect.JointType.Head]);
                    }
                }
            }


        }

        private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
        {
            return new Vector3(-joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        private Kinect.Body ClosestBody(Kinect.Body[] data)
        {
            float[] distances = new float[6];
            int bodyindex = -1;
            foreach (var body in data)
            {
                bodyindex++;
                distances[bodyindex] = 10000;
                if (body == null) continue;
                if (!body.IsTracked) continue;
                distances[bodyindex] = body.Joints[Kinect.JointType.SpineBase].Position.Z;
            }

            int closestBodyIndex = Array.IndexOf(distances, distances.Min());
            return data[closestBodyIndex];
        }
    }
}
