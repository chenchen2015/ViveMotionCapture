using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;

namespace ViveMotionCapture
{
	#region Capture Data Structures

	[System.Serializable, HideInInspector]
	class SerializedVec3
	{
		public float x;
		public float y;
		public float z;

		public SerializedVec3 ()
		{
			this.x = 0.0f;
			this.y = 0.0f;
			this.z = 0.0f;
		}

		public SerializedVec3 (Vector3 vec3)
		{
			this.x = vec3.x;
			this.y = vec3.y;
			this.z = vec3.z;
		}

		public static Vector3 ToVec3 (SerializedVec3 sv3)
		{
			return new Vector3 (sv3.x, sv3.y, sv3.z);
		}

		public static Quaternion ToQuat (SerializedVec3 sv3)
		{
			return Quaternion.Euler (ToVec3 (sv3));
		}
	}

	class TrackedDevice
	{
		public List<SerializedVec3> position = new List<SerializedVec3> ();
		public List<SerializedVec3> rotation = new List<SerializedVec3> ();

		public SerializedVec3 refPosition = new SerializedVec3 ();
		public SerializedVec3 refRotation = new SerializedVec3 ();
	}

	class TrackedData
	{
		public List<float> timeStamp = new List<float> ();
		public Dictionary<string, TrackedDevice> trackedDevice = new Dictionary<string, TrackedDevice> ();

		public float refTimeStamp = -1.0f;
		public List<float> keyTimeStamp = new List<float> ();

		public string sessionDTime;
		public string teamName;

		public int Count {
			get{ return timeStamp.Count (); }
		}

		public float RecordingLength {
			get{ return timeStamp.Last (); }
		}

		// Clear method
		public void Clear ()
		{
			timeStamp.Clear ();
			trackedDevice.Clear ();
			keyTimeStamp.Clear ();

			refTimeStamp = -1.0f;
			sessionDTime = null;
			teamName = null;
		}

		// Overload the index operator
		public TrackedDevice this [int idx] {
			get { 
				return trackedDevice [trackedDevice.Keys.ElementAt (idx)];
			}

			set { 
				trackedDevice [trackedDevice.Keys.ElementAt (idx)] = value;
			}
		}
	}

	#endregion

}