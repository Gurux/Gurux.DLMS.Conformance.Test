//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using Gurux.Net;
using Gurux.Serial;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Diagnostics;

namespace Gurux.DLMS.Conformance.Test
{
    public class Program
    {
        static int Main(string[] args)
        {
            SetAddRemoveProgramsIcon();
            GXOutput ouput = new Test.GXOutput();
            GXSettings settings = new GXSettings();
            List<KeyValuePair<GXDLMSXmlPdu, List<string>>> differences = new List<KeyValuePair<GXDLMSXmlPdu, List<string>>>();
            GXDLMSConverter c = new GXDLMSConverter();
            try
            {
                if (args.Length == 0)
                {
                    GXProperties dlg = new GXProperties(settings);
                    if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return 0;
                    }
                }
                else
                {
                    //Handle command line parameters.
                    int ret = GXSettings.GetParameters(args, settings);
                    if (ret != 0)
                    {
                        return ret;
                    }
                }
                ////////////////////////////////////////
                //Check media connection settings.
                if (!(settings.media is GXSerial ||
                    settings.media is GXNet))
                {
                    throw new Exception("Unknown media type.");
                }

                //Read basic tests to pass DLMS Conformance tests.
                //Breen Book: 4.1.8.4 Mandatory contents of a COSEM logical device
                //1. Read Association view.
                //2. Check Logical Device Name
                //3. Check SAP.    
                if ((settings.tests & ConformanceTest.Cosem) != 0)
                {
                    ouput.writer.Write("<h2>Cosem tests:</h2>");
                    GXTests.Basic(settings, ouput);
                }
            }
            catch (Exception e)
            {
                if (settings.trace > TraceLevel.Off)
                {
                    Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Console.WriteLine("Gurux Conformance Test ended.");
                    Console.WriteLine(e.Message);
                }
                return 1;
            }
            try
            {
                if ((settings.tests & ConformanceTest.Init) != 0)
                {
                    ouput.writer.Write("<h2>Initialization tests:</h2>");
                    GXTests.Init(settings, ouput);
                }
            }
            catch (Exception ex)
            {
                if (settings.trace > TraceLevel.Off)
                {
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine(ex.Message);
                }
                return 1;
            }
            try
            {
                if (settings.path != null)
                {
                    ouput.writer.Write("<h2>Extra tests:</h2>");
                    GXTests.Extra(settings, ouput);
                }
            }
            catch (Exception ex)
            {
                if (settings.trace > TraceLevel.Off)
                {
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine(ex.Message);
                }
                return 1;
            }
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("Gurux Conformance Test ended.");
            ouput.writer.Flush();
            Process.Start(ouput.GetName());
            return 0;
        }

        /// <summary>
        /// Set the icon in add/remove programs.
        /// </summary>
        private static void SetAddRemoveProgramsIcon()
        {
            // only run if clickonce deployed, on first run only
            if (!System.Diagnostics.Debugger.IsAttached && ApplicationDeployment.IsNetworkDeployed
            && ApplicationDeployment.CurrentDeployment.IsFirstRun)
            {
                try
                {
                    string icon = string.Format("{0},0", System.Reflection.Assembly.GetExecutingAssembly().Location);
                    RegistryKey myUninstallKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                    string[] mySubKeyNames = myUninstallKey.GetSubKeyNames();
                    for (int i = 0; i < mySubKeyNames.Length; i++)
                    {
                        RegistryKey myKey = myUninstallKey.OpenSubKey(mySubKeyNames[i], true);
                        object myValue = myKey.GetValue("DisplayName");
                        if (myValue != null && myValue.ToString() == "GXDLMSDirector")
                        {
                            myKey.SetValue("DisplayIcon", icon);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
