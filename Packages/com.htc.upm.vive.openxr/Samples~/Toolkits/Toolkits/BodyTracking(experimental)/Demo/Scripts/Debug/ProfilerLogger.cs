// Copyright HTC Corporation All Rights Reserved.

using System.IO;
using System.Text;
using UnityEngine;

namespace VIVE.OpenXR.Toolkits.BodyTracking.Demo
{
	public class ProfilerLogger : MonoBehaviour
	{
		const string LOG_TAG = "VIVE.OpenXR.Toolkits.BodyTracking.Demo.ProfilerLogger ";
		private StringBuilder m_sb = null;
		internal StringBuilder sb {
			get {
				if (m_sb == null) { m_sb = new StringBuilder(); }
				return m_sb;
			}
		}
		void DEBUG(StringBuilder sb)
		{
			sb.Insert(0, LOG_TAG);
			Debug.Log(sb);
		}

		private string logFilePath;

		private void Awake()
		{
			logFilePath = Path.Combine(Application.persistentDataPath, "profiler_log");
			sb.Clear().Append("Profiler log file path: ").Append(logFilePath); DEBUG(sb);

			UnityEngine.Profiling.Profiler.logFile = logFilePath;
			UnityEngine.Profiling.Profiler.enableBinaryLog = true;
			UnityEngine.Profiling.Profiler.enabled = true;
		}

		public void ExitGame()
		{
			UnityEngine.Profiling.Profiler.enableBinaryLog = false;
			UnityEngine.Profiling.Profiler.enabled = false;

			/*string rawFilePath = logFilePath + ".raw";
			string destinationPath = "/sdcard/profiler_log.raw";
			File.Move(rawFilePath, destinationPath);
			sb.Clear().Append("Profiler log file saved to: ").Append(destinationPath); DEBUG(sb);*/

			Application.Quit();
		}
	}
}
