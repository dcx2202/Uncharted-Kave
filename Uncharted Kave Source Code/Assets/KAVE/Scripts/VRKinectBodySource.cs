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

using Windows.Kinect;
using UnityEngine;

namespace VRKave
{
    public class VRKinectBodySource : MonoBehaviour
    {
        public KinectSensor Sensor { get; private set; }
        private BodyFrameReader _reader;
        private Body[] _data = null;
        public Windows.Kinect.Vector4 Floor { get; private set; }

        public Body[] GetData()
        {
            return _data;
        }

        void Start()
        {
            Sensor = KinectSensor.GetDefault();
            if (Sensor != null)
            {
                _reader = Sensor.BodyFrameSource.OpenReader();
                if (!Sensor.IsOpen)
                    Sensor.Open();
            }
        }

        void Update()
        {
            if (Sensor == null)
                Sensor = KinectSensor.GetDefault();

            if (Sensor != null)
            {
                if (!Sensor.IsOpen)
                    Sensor.Open();

                if (Sensor.IsOpen)
                {
                    if (_reader == null)
                        _reader = Sensor.BodyFrameSource.OpenReader();
                    else
                    {
                        UpdateBodyData();
                    }
                }
            }
        }

        private void UpdateBodyData()
        {
            var frame = _reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_data == null)
                    _data = new Body[Sensor.BodyFrameSource.BodyCount];
                frame.GetAndRefreshBodyData(_data);
                Floor = frame.FloorClipPlane;
                frame.Dispose();
                //frame = null;
            }
        }

        void OnApplicationQuit()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (Sensor == null) return;
            if (Sensor.IsOpen)
                Sensor.Close();
            Sensor = null;
        }
    }
}
