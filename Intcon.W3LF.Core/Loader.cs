/*
 *    Warcraft 3 Loader Form - A C# forms application modification upon the original W3L by Acid and Keres
 *	Copyright (C) 2011  MusicDemon
 *
 *	This program is free software: you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation, either version 3 of the License, or
 *	(at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

// This feature is used for the XPAM edition however, you could use it as well!
//#define XPAM

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace Intcon.W3LF
{
    /// <summary>
    /// The wrapper around W3L.
    /// </summary>
    public static class Loader
    {

        #region API
        /// <summary>
        /// The call that is made to W3L.dll.
        /// </summary>
        /// <param name="CommandLine">Command line arguments. <remarks>Add the full execution path in quotation.</remarks></param>
        /// <param name="ExePath">The path to war3.exe.</param>
        /// <param name="ErrorMsg">Any error message that occured while patching/loading war3.exe.</param>
        /// <returns>A code about how the library has handled the call.</returns>
        [DllImport("w3l.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int DoInject(string CommandLine, string ExePath, [Out] out string ErrorMsg);
        // UNDONE: Changed the size of the API definition.
        //static extern Int32 DoInject(
        //    [MarshalAs(UnmanagedType.LPStr)] string CommandLine,
        //    [MarshalAs(UnmanagedType.LPStr)] string ExePath,
        //    [Out][MarshalAs(UnmanagedType.LPStr)] out string ErrorMsg
        //    );
        #endregion
        /// <summary>
        /// A list of Warcraft III startup options.
        /// </summary>
        [Flags()]
        public enum StartOptions : byte
        {
            /// <summary>
            /// No special arguments.
            /// </summary>
            None = 0,
            /// <summary>
            /// Starts Warcraft III in windowed mode.
            /// </summary>
            Window = 1,
            /// <summary>
            /// Starts Warcraft III with OpenGL.
            /// </summary>
            OpenGl = 2,
            /// <summary>
            /// Starts Warcraft III with Software Transform and Lighting Video mode
            /// </summary>
            SWTNL = 4,
            /// <summary>
            /// Starts classic Warcraft III.
            /// </summary>
            Classic = 8
        }
        /// <summary>
        /// This class is used for the registry check.
        /// </summary>
        static class ServerData
        {
            // Direct connection.
            public static string Server = "server.eurobattle.net";
            public static int Zone = 8;
            public static string Name = "Eurobattle.net";
            // Proxied.
            public static string GP_Server = "localhost";
            public static int GP_Zone = 8;
            public static string GP_Name = "Eurobattle.net GProxy";
        }
        /// <summary>
        /// Attempts to start Warcraft III (and GProxy.)
        /// </summary>
        /// <param name="RunGP">Determine whether or not to check data for GProxy as well.</param>
        /// <returns>True if call(s) has/have succeeded; else false.</returns>
#if XPAM
        public static bool RunW3(bool RunGP)
#else
        public static bool RunW3()
#endif
        {
            try
            {
                // Check if war3.exe and w3lh.dll exist.
                try
                {
                    if (!File.Exists(Settings.W3Path + "\\war3.exe"))
                    {
                        MessageBox.Show("The given directory does not contain war3.exe, please make sure your registry contains the right information.\r\n\r\nPath: " + Settings.W3Path);
                        return false;
                    }
                    if (!File.Exists(Settings.W3Path + "\\w3lh.dll"))
                    {
                        if (File.Exists(Environment.CurrentDirectory + "\\w3lh.dll"))
                        {
                            File.Copy(Environment.CurrentDirectory + "\\w3lh.dll", Settings.W3Path + "\\w3lh.dll");
                        }
                        else
                        {
                            MessageBox.Show("w3lh.dll copy in the directory \"" + Environment.CurrentDirectory + "\\w3lh.dll\" does not exist.\r\nPlease reinstall EuroLoader.", "Error", MessageBoxButtons.OK);
                            return false;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("The application does not have the rights to either read or write to the source/destination directory.", "Error", MessageBoxButtons.OK);
                    return false;
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("w3lh.dll was not found, please reinstall the application.", "Error", MessageBoxButtons.OK);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unhandled exception was thrown.\r\n\r\n" + ex.Message, "Error", MessageBoxButtons.OK);
                    return false;
                }

                // Checks the registry for a server.eurobattle.net entry.
                List<string> Entries = new List<string>((string[])Registry.CurrentUser.OpenSubKey("Software\\Blizzard Entertainment\\Warcraft III").GetValue("Battle.net Gateways"));
                //int count = (Entries.Count - 2) / 3;
                int server = 0;
                bool HadServer = false;
                for (int i = 2; i < Entries.Count; i++)
                {
                    if (Entries[i].ToLower() == ServerData.Server)
                    {

                        if (i + 1 > Entries.Count - 1)
                            Entries.Add(ServerData.Zone.ToString());
                        else
                            if (Entries[i + 1] != ServerData.GP_Zone.ToString()) Entries[i + 1] = ServerData.Zone.ToString();
                        if (i + 2 > Entries.Count - 1)
                            Entries.Add(ServerData.Name);
                        else
                            if (Entries[i + 2] != ServerData.Name) Entries[i + 2] = ServerData.Name;
#if XPAM
                        if (!RunGP) {
#endif
                        server = i / 3; HadServer = true;
#if XPAM
                        }
#endif
                    }
                    if (Entries[i].ToLower() == ServerData.GP_Server)
                    {
                        if (i + 1 > Entries.Count - 1)
                            Entries.Add(ServerData.Zone.ToString());
                        else
                            if (Entries[i + 1] != ServerData.Zone.ToString()) Entries[i + 1] = ServerData.Zone.ToString();
                        if (i + 2 > Entries.Count - 1)
                            Entries.Add(ServerData.GP_Name);
                        else
                            if (Entries[i + 2] != ServerData.GP_Name) Entries[i + 2] = ServerData.GP_Name;
#if XPAM
                        if (RunGP) { server = i / 3; HadServer = true; }
#endif
                    }
                }
                if (!HadServer)
#if XPAM
                    if (RunGP)
                    { Entries.AddRange(new string[] { ServerData.GP_Name, ServerData.GP_Zone.ToString(), ServerData.GP_Server }); server = ((Entries.Count - 2) / 3); }
                    else
                    { 
#endif
                    Entries.AddRange(new string[] { ServerData.Name, ServerData.Zone.ToString(), ServerData.Server }); server = ((Entries.Count - 2) / 3);
#if XPAM        
            }
#endif
                if (!(((Entries.Count - 2) / 3) < server))
                    Entries[1] = (server.ToString().Length == 2 ? server.ToString() : "0" + server.ToString());
                Registry.CurrentUser.OpenSubKey("Software\\Blizzard Entertainment\\Warcraft III", true).SetValue("Battle.net Gateways", Entries.ToArray(), RegistryValueKind.MultiString);
                // Checks for the given arguments.
                StartOptions StartOption = Settings.StartupOptions;
                string Args = "";
                if ((StartOption & StartOptions.Window) == StartOptions.Window) Args += " -window ";
                if ((StartOption & StartOptions.OpenGl) == StartOptions.OpenGl) Args += " -opengl ";
                if ((StartOption & StartOptions.SWTNL) == StartOptions.SWTNL) Args += " -swtnl ";
                if ((StartOption & StartOptions.Classic) == StartOptions.Classic) Args += " -classic ";
                string Msg;
                /*
                 * Why Settings.W3Path + "\\war3.exe":
                 *  The command line will not accept one argument.
                 *  Structure:
                 *      <Path of the to-execute application> [arguments]
                 *  If you do:
                 *      -window
                 *  Windows/Warcraft III will think you're starting up -window with no arguments.
                 *  Therefore you must have at least two arguments.
                 */
                switch (DoInject('"' + Settings.W3Path + "\\war3.exe" + '"' + Args, Settings.W3Path + "\\war3.exe", out Msg))
                {
                    case 0: if (Msg != null) MessageBox.Show(Msg, "Startup message"); return true; // No error.
                    case 2: MessageBox.Show("Unable to find war3.exe in\r\n" + Msg, "Error", MessageBoxButtons.OK); return false;
                    case 1:
                    default: MessageBox.Show(Msg, "Error", MessageBoxButtons.OK); return false;
                }
            }
            catch (AccessViolationException)
            {
                MessageBox.Show("An exception was thrown: Access Violation.", "Error", MessageBoxButtons.OK);
                return false;
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Loader was unable to find w3l.dll.\r\nReinstalling the application could solve the issue.", "Error", MessageBoxButtons.OK);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception was thrown:\r\n\r\n" + ex.Message, "Error", MessageBoxButtons.OK);
                return false;
            }
        }
    }
}