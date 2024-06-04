// Copyright HTC Corporation All Rights Reserved.
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using VIVE.OpenXR.FacialTracking;
using VIVE.OpenXR.Hand;
using VIVE.OpenXR.Tracker;

// Reference to Unity's OpenXR package

namespace VIVE.OpenXR.Editor
{
	internal class ModifyAndroidManifest : OpenXRFeatureBuildHooks
	{
		public override int callbackOrder => 1;

		public override Type featureType => typeof(VIVEFocus3Feature);

		protected override void OnPreprocessBuildExt(BuildReport report)
		{
		}

		private static string _manifestPath;

		protected override void OnPostGenerateGradleAndroidProjectExt(string path)
		{
			_manifestPath = GetManifestPath(path);

			var androidManifest = new AndroidManifest(_manifestPath);
			//androidManifest.AddVIVECategory();
			androidManifest.AddViveSDKVersion();
			androidManifest.AddUnityVersion();
			androidManifest.AddOpenXRPermission();
			androidManifest.AddOpenXRFeatures();
			androidManifest.Save();
		}

		protected override void OnPostprocessBuildExt(BuildReport report)
		{
			if (File.Exists(_manifestPath))
				File.Delete(_manifestPath);
		}

		private string _manifestFilePath;

		private string GetManifestPath(string basePath)
		{
			if (!string.IsNullOrEmpty(_manifestFilePath)) return _manifestFilePath;

			var pathBuilder = new StringBuilder(basePath);
			pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
			pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
			pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
			_manifestFilePath = pathBuilder.ToString();

			return _manifestFilePath;
		}

		private class AndroidXmlDocument : XmlDocument
		{
			private string m_Path;
			protected XmlNamespaceManager nsMgr;
			public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";

			public AndroidXmlDocument(string path)
			{
				m_Path = path;
				using (var reader = new XmlTextReader(m_Path))
				{
					reader.Read();
					Load(reader);
				}

				nsMgr = new XmlNamespaceManager(NameTable);
				nsMgr.AddNamespace("android", AndroidXmlNamespace);
			}

			public string Save()
			{
				return SaveAs(m_Path);
			}

			public string SaveAs(string path)
			{
				using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
				{
					writer.Formatting = Formatting.Indented;
					Save(writer);
				}

				return path;
			}
		}

		[InitializeOnLoad]
		public static class CheckIfSimultaneousInteractionEnabled
		{
			const string LOG_TAG = "CheckIfSimultaneousInteractionEnabled ";
			static StringBuilder m_sb = null;
			static StringBuilder sb
			{
				get
				{
					if (m_sb == null) { m_sb = new StringBuilder(); }
					return m_sb;
				}
			}
			static void DEBUG(StringBuilder msg) { Debug.Log(msg); }

			internal const string MENU_NAME = "VIVE/Interaction Mode/Enable Simultaneous Interaction";

			private static bool m_IsEnabled = false;
			public static bool IsEnabled { get { return m_IsEnabled; } }

			static CheckIfSimultaneousInteractionEnabled()
			{
				m_IsEnabled = EditorPrefs.GetBool(MENU_NAME, false);

				/// Delaying until first editor tick so that the menu
				/// will be populated before setting check state, and
				/// re-apply correct action
				EditorApplication.delayCall += () =>
				{
					PerformAction(m_IsEnabled);
				};
			}

			[MenuItem(MENU_NAME, priority = 601)]
			private static void ToggleAction()
			{
				/// Toggling action
				PerformAction(!m_IsEnabled);
			}

			public static void PerformAction(bool enabled)
			{
				/// Set checkmark on menu item
				Menu.SetChecked(MENU_NAME, enabled);

				/// Saving editor state
				EditorPrefs.SetBool(MENU_NAME, enabled);

				m_IsEnabled = enabled;

				sb.Clear().Append(LOG_TAG).Append(m_IsEnabled ? "Enable " : "Disable ").Append("Simultaneous Interaction."); DEBUG(sb);
			}

			[MenuItem(MENU_NAME, validate = true, priority = 601)]
			public static bool ValidateEnabled()
			{
				Menu.SetChecked(MENU_NAME, m_IsEnabled);
				return true;
			}
		}

		private class AndroidManifest : AndroidXmlDocument
		{
			private readonly XmlElement IntetnFilterElement;
			private readonly XmlElement ManifestElement;
			private readonly XmlElement ApplicationElement;

			public AndroidManifest(string path) : base(path)
			{
				IntetnFilterElement = SelectSingleNode("/manifest/application/activity/intent-filter") as XmlElement;
				ManifestElement = SelectSingleNode("/manifest") as XmlElement;
				ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
			}

			private XmlAttribute CreateAndroidAttribute(string key, string value)
			{
				XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
				attr.Value = value;
				return attr;
			}

			private const string FAKE_VERSION = "0.0.0";

			private static string SearchPackageVersion(string packageName)
			{
				var listRequest = Client.List(true);
				do
				{
					if (listRequest.IsCompleted)
					{
						if (listRequest.Result == null)
						{
							Debug.Log("List result: is empty");
							return FAKE_VERSION;
						}

						foreach (var pi in listRequest.Result)
						{
							//Debug.Log("List has: " + pi.name + " == " + packageName);
							if (pi.name == packageName)
							{
								Debug.Log("Found " + packageName);

								return pi.version;
							}
						}
						break;
					}
					Thread.Sleep(100);
				} while (true);
				return FAKE_VERSION;
			}

			internal void AddViveSDKVersion()
			{
				var newUsesFeature = CreateElement("meta-data");
				newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "com.htc.ViveOpenXR.SdkVersion"));
				newUsesFeature.Attributes.Append(CreateAndroidAttribute("value", SearchPackageVersion("com.htc.upm.vive.openxr")));
				ApplicationElement.AppendChild(newUsesFeature);
			}

			internal void AddUnityVersion()
			{
				var newUsesFeature = CreateElement("meta-data");
				newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "com.htc.vr.content.UnityVersion"));
				newUsesFeature.Attributes.Append(CreateAndroidAttribute("value", Application.unityVersion));
				ApplicationElement.AppendChild(newUsesFeature);
			}

			internal void AddVIVECategory()
			{
				var md = IntetnFilterElement.AppendChild(CreateElement("category"));
				md.Attributes.Append(CreateAndroidAttribute("name", "com.htc.intent.category.VRAPP"));
			}

			internal void AddOpenXRPermission()
			{
				var md = ManifestElement.AppendChild(CreateElement("uses-permission"));
				md.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.permission.OPENXR"));

				md = ManifestElement.AppendChild(CreateElement("uses-permission"));
				md.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.permission.OPENXR_SYSTEM"));

				var md2 = IntetnFilterElement.AppendChild(CreateElement("category"));
				md2.Attributes.Append(CreateAndroidAttribute("name", "org.khronos.openxr.intent.category.IMMERSIVE_HMD"));
			}

			internal void AddOpenXRFeatures()
			{
				bool enableHandtracking = false;
				bool enableTracker = false;
				bool enableEyetracking = false;
				bool enableLipexpression = false;

				var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
				if (null == settings)
					return;

				foreach (var feature in settings.GetFeatures<OpenXRInteractionFeature>())
				{
					if ((feature is ViveWristTracker || feature is ViveXRTracker) && feature.enabled)
					{
						enableHandtracking = true;
						enableTracker = true;
					}
					if (feature is EyeGazeInteraction && feature.enabled)
					{
						enableEyetracking = true;
					}
					if (feature is ViveHandInteraction && feature.enabled)
					{
						enableHandtracking = true;
					}
				}

				foreach (var feature in settings.GetFeatures<OpenXRFeature>())
				{
					if (feature is ViveHandTracking && feature.enabled)
					{
						enableHandtracking = true;
					}
					if (feature is ViveFacialTracking && feature.enabled)
					{
						enableEyetracking = true;
						enableLipexpression = true;
					}
				}

				if (enableHandtracking)
				{
					var newUsesFeature = CreateElement("uses-feature");
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.handtracking"));
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
					ManifestElement.AppendChild(newUsesFeature);
				}

				if (enableTracker)
				{
					var newUsesFeature = CreateElement("uses-feature");
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.tracker"));
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
					ManifestElement.AppendChild(newUsesFeature);
				}

				if (enableEyetracking)
				{
					var newUsesFeature = CreateElement("uses-feature");
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.eyetracking"));
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
					ManifestElement.AppendChild(newUsesFeature);
				}

				if (enableLipexpression)
				{
					var newUsesFeature = CreateElement("uses-feature");
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.lipexpression"));
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
					ManifestElement.AppendChild(newUsesFeature);
				}

				if (CheckIfSimultaneousInteractionEnabled.IsEnabled)
				{
					var newUsesFeature = CreateElement("uses-feature");
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("name", "wave.feature.simultaneous_interaction"));
					newUsesFeature.Attributes.Append(CreateAndroidAttribute("required", "true"));
					ManifestElement.AppendChild(newUsesFeature);
				}
			}
		}
	}
}

