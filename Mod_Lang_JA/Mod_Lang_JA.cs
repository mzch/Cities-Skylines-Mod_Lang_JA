﻿using ICities;
using System;
using System.IO;
using ColossalFramework.Plugins;
using System.Reflection;

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !
// ! This mod is based on Chinese Tradisional localization https://github.com/ccpz/cities-skylines-Mod_Lang_CHT
// ! Thank you a lot, Taiwanese creators!
// !
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

namespace Mod_Lang_JA
{
	public class Mod_Lang_JA : IUserMod
	{
		private string locale_name = "ja";

		private bool initialized = false;

		//
		//the following OS detect code is referring http://stackoverflow.com/questions/10138040/how-to-detect-properly-windows-linux-mac-operating-systems
		//
		public enum Platform
		{
			Windows,
			Linux,
			Mac
		}

		public static Platform RunningPlatform()
		{
			switch (Environment.OSVersion.Platform)
			{
			case PlatformID.Unix:
				// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
				// Instead of platform check, we'll do a feature checks (Mac specific root folders)
				if (Directory.Exists("/Applications")
					& Directory.Exists("/System")
					& Directory.Exists("/Users")
					& Directory.Exists("/Volumes"))
					return Platform.Mac;
				else
					return Platform.Linux;

			case PlatformID.MacOSX:
				return Platform.Mac;

			default:
				return Platform.Windows;
			}
		}

		//------------------------------------------------------------
		// Create destination path to copy the locale file to
		//------------------------------------------------------------
		private string getDestinationPath()
		{
			String dst_path = "";
			#if (DEBUG)
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("OS Type: {0}", RunningPlatform().ToString()));
			#endif
			switch (RunningPlatform())
			{
			case Platform.Windows:
				dst_path = "Files\\Locale\\" + locale_name + ".locale";
				break;
			case Platform.Mac:
				dst_path = "Cities.app/Contents/Resources/Files/Locale/" + locale_name + ".locale";
				break;
			case Platform.Linux:
				dst_path = "Files/Locale/" + locale_name + ".locale";
				break;
			}

			#if (DEBUG)
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("Destination {0}", dst_path));
			#endif

			return dst_path;
		}

		//------------------------------------------------------------
		// Force to reload the locale manager
		//------------------------------------------------------------
		private void resetLocaleManager(String loc_name)
		{
			// Reload Locale Manager
			ColossalFramework.Globalization.LocaleManager.ForceReload();

			string[] locales = ColossalFramework.Globalization.LocaleManager.instance.supportedLocaleIDs;
			for (int i = 0; i < locales.Length; i++)
			{
				#if (DEBUG)
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("Locale index: {0}, ID: {1}", i, locales[i]));
				#endif
				if (locales[i].Equals(loc_name))
				{
					#if (DEBUG)
					DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("Find locale {0} at index: {1}", loc_name, i));
					#endif
					ColossalFramework.Globalization.LocaleManager.instance.LoadLocaleByIndex(i);

					//thanks to: https://github.com/Mesoptier/SkylineToolkit/commit/d33f0bae67662df25bdf8ee2170d95a6999c3721
					ColossalFramework.SavedString lang_setting = new ColossalFramework.SavedString("localeID", "gameSettings");
					#if (DEBUG)
					DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("Current Language Setting: {0}", lang_setting.value));
					#endif
					lang_setting.value = locale_name;
					ColossalFramework.GameSettings.SaveAll();                                
					break;
				}
			}
		}

		//------------------------------------------------------------
		// Copy the locale file
		//------------------------------------------------------------
		private void copyLocaleFile(String dst_path)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			Stream src = asm.GetManifestResourceStream(asm.GetName().Name+"."+locale_name+".locale");
			#if (DEBUG)
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("File size: {0}", src.Length));
			#endif

			FileStream dst = File.OpenWrite(dst_path);

			byte[] buffer = new byte[8 * 1024];
			int len = 0;
			while ((len = src.Read(buffer, 0, buffer.Length)) > 0)
			{
				dst.Write(buffer, 0, len);
			}
			dst.Close();
			src.Close();
	
			#if (DEBUG)
			DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, String.Format("File write to: {0}", Path.GetFullPath(dst.Name)));
			#endif
		}

		//============================================================
		// Main
		//============================================================
		private void CopyLocaleAndReloadLocaleManager()
		{
			try
			{
				String dst_path = getDestinationPath();

				if (dst_path.Length > 0)
				{
					if (File.Exists(dst_path))
					{
                        //File.OpenWrite won't truncate file, so delete it first
                        File.Delete(dst_path);
						#if (DEBUG)
						DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Locale file is found, user has already used this mod before.");
						#endif
                        initialized = true;
                    }

                    copyLocaleFile(dst_path);

					if (initialized == false)
					{
						resetLocaleManager(locale_name);
					}
				}
			}
			catch (Exception e)
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, e.ToString());
			}
		}

		//============================================================
		// Modding API
		//============================================================
		public string Name
		{
			get
			{
                if (!initialized)
                {
                    CopyLocaleAndReloadLocaleManager();
                }
                initialized = true;

				return "Japanese localization Mod";
			}
		}

		public string Description
		{
			get { return "Japanese Localization the Final for Patch 1.17.1-f2, by volunteers on Discord: Cities Skylines Japan"; }
		}
	}
}