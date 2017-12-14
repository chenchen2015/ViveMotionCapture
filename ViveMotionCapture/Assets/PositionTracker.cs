// Vive Motion Capture
// version: 1.0
// For Data as Art class Fall 2017 (Northwestern University and School of the Art Institute of Chicago)
// Chen Chen
// License: MIT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

namespace ViveMotionCapture
{
	public class PositionTracker : MonoBehaviour
	{
		public GameObject[] trackedDevices;
		public float captureFPS = 100.0f;
		public string teamName = "";

		private float captureDeltaT;
		private readonly string SaveFolder = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments) + "\\ViveMotionCapture\\";
		private bool isCapturing = false;
		private uint captureCounter = 0;
		private float captureStartTime = 0.0f;
		private TrackedData data = new TrackedData ();

		private bool refTimeSnapPending = false;
		private bool keyTimeSnapPending = false;


		private string dateToday {
			get{ return DateTime.Now.ToString ("MM-dd-yyyy"); }
		}

		private float captureTimeNow {
			get { return Time.realtimeSinceStartup - captureStartTime; }
		}
			
		// Use this for initialization
		void Start ()
		{
			captureDeltaT = 1.0f / captureFPS;

			if (!Directory.Exists (SaveFolder))
				Directory.CreateDirectory (SaveFolder);
		}

		private void Update ()
		{
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (isCapturing)
					StopCapture ();
				else
					StartCapture ();
			}

			refTimeSnapPending |= Input.GetKeyDown (KeyCode.R);
			keyTimeSnapPending |= Input.GetKeyDown (KeyCode.K);

		}

		private void StartCapture ()
		{
			if (isCapturing)
				return;

			// Clear existing data
			data.Clear ();

			// Add time of the session
			data.sessionDTime = DateTime.Now.ToString ("G");
			// Add team name
			data.teamName = teamName;

			// Start capture
			isCapturing = true;
			InvokeRepeating ("FixedFPSCapture", 0.0f, captureDeltaT);

			Debug.Log ("Capture started!");
		}

		private void StopCapture ()
		{
			if (!isCapturing)
				return;

			// Stop capture
			isCapturing = false;
			CancelInvoke ("FixedFPSCapture");
			captureCounter = 0;

			// Convert existing capture to JSON
			string json = JsonConvert.SerializeObject (data, Formatting.Indented,
				              new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
			              );
			Debug.Log ("Capture finished, saving file...");

			// Write JSON to file
			WriteJSON (json);
		}

		private void WriteJSON (string json, string path = null)
		{
			if (string.IsNullOrEmpty (path) || !Directory.Exists (path))
				path = SaveFolder;

			// Get a new filename
			int sessionID = 0;
			string fname = path + teamName.Replace (" ", "-") + "_session_" + dateToday + sessionID.ToString ("_000") + ".json";
			while (File.Exists (fname)) {
				sessionID += 1;
				fname = path + teamName.Replace (" ", "-") + "_session_" + dateToday + sessionID.ToString ("_000") + ".json";
			}

			// Write JSON to file
			using (FileStream fs = new FileStream (fname, FileMode.Create)) {
				using (StreamWriter writer = new StreamWriter (fs)) {
					writer.Write (json);
				}
			}

			Debug.Log ("File created at " + fname);
		}

		private void FixedFPSCapture ()
		{
			if (isCapturing) {
				if (captureCounter == 0)
					captureStartTime = Time.realtimeSinceStartup;

				captureCounter += 1;

				// Wait until the next capture timestamp
				while (captureTimeNow < (captureCounter * captureDeltaT)) {
				}

				// If reference time stamp snap is pending
				if (refTimeSnapPending) {
					refTimeSnapPending = false;
					data.refTimeStamp = captureTimeNow;

					// Capture each devices
					foreach (GameObject obj in trackedDevices) {
						if (!data.trackedDevice.ContainsKey (obj.name))
							data.trackedDevice.Add (obj.name, new TrackedDevice ());

						data.trackedDevice [obj.name].refPosition = new SerializedVec3 (obj.transform.position);
						data.trackedDevice [obj.name].refRotation = new SerializedVec3 (obj.transform.eulerAngles);
					}

					Debug.Log (string.Format ("Reference time stamp is set at {0:F3} s", captureTimeNow));
				}

				// If key time stamp snap is pending
				if (keyTimeSnapPending) {
					keyTimeSnapPending = false;
					data.keyTimeStamp.Add (captureTimeNow);
					Debug.Log (string.Format ("Added #{0:D} key time stamp at {1:F3} s", data.keyTimeStamp.Count, captureTimeNow));
				}

				// Record current time stamp
				data.timeStamp.Add (captureTimeNow);

				// Capture each devices
				foreach (GameObject obj in trackedDevices) {
					if (!data.trackedDevice.ContainsKey (obj.name))
						data.trackedDevice.Add (obj.name, new TrackedDevice ());
					
					data.trackedDevice [obj.name].position.Add (new SerializedVec3 (obj.transform.position));
					data.trackedDevice [obj.name].rotation.Add (new SerializedVec3 (obj.transform.eulerAngles));
				}

				// Display performance
				if ((captureCounter % 50) == 0) {
					Debug.Log (string.Format ("Capturing, time = {0:F3} s, average FPS = {1:F2}", captureTimeNow, captureCounter / captureTimeNow));
				}
			}
		}
	}
}
