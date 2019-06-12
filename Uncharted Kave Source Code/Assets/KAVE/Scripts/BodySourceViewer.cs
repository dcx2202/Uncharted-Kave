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

using UnityEngine;
using Windows.Kinect;

namespace VRKave
{
    public class BodySourceViewer : MonoBehaviour
    {
        public GameObject _avatar;

        private VRKinectBodySource _bodySource;
        private KinectSensor _sensor;
        private GameObject[] _avatars;
        private bool[] _activeAvatars;

        void Start()
        {
            _bodySource = gameObject.GetComponent<VRKinectBodySource>();
        }

        void Update()
        {

            if (_sensor == null)
            {
                _sensor = _bodySource.Sensor;
                if (_sensor == null) return;

                _avatars = new GameObject[_sensor.BodyFrameSource.BodyCount];
                _activeAvatars = new bool[_sensor.BodyFrameSource.BodyCount];
            }

            Body[] kinectBodies = _bodySource.GetData();
            if (kinectBodies == null)
            {
                //Destroy all avatars that exist
                for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
                {
                    if (_avatars[bodyIndex] == null) continue;

                    Destroy(_avatars[bodyIndex]);
                    _avatars[bodyIndex] = null;
                    _activeAvatars[bodyIndex] = false;
                }
                return;
            }

            for (int bodyIndex = 0; bodyIndex < _sensor.BodyFrameSource.BodyCount; bodyIndex++)
            {
                if (kinectBodies[bodyIndex] != null)
                {
                    if (!_activeAvatars[bodyIndex] && kinectBodies[bodyIndex].IsTracked)
                    {
                        _avatars[bodyIndex] = Instantiate(_avatar);
                        _avatars[bodyIndex].transform.SetParent(gameObject.transform);
                        _avatars[bodyIndex].GetComponent<JointPositionControl>().SetBodyIndex(bodyIndex);
                        _avatars[bodyIndex].GetComponent<JointOrientationControl>().SetBodyIndex(bodyIndex);
                        _activeAvatars[bodyIndex] = true;
                    }
                    else if (_activeAvatars[bodyIndex] && !kinectBodies[bodyIndex].IsTracked)
                    {
                        //Destroy the avatar if it exists but the corresponding body is not being tracked
                        Destroy(_avatars[bodyIndex]);
                        _avatars[bodyIndex] = null;
                        _activeAvatars[bodyIndex] = false;
                    }
                }
                else
                {
                    //Destroy the avatar if the body is null
                    Destroy(_avatars[bodyIndex]);
                    _avatars[bodyIndex] = null;
                    _activeAvatars[bodyIndex] = false;
                }
            }
        }
    }
}
