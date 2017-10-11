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
using Gurux.Common;
using Gurux.DLMS.Enums;
using Gurux.Net;
using Gurux.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace Gurux.DLMS.Conformance.Test
{
    /// <summary>
    /// Conformance test types.
    /// </summary>
    public enum ConformanceTest
    {
        /// <summary>
        /// All tests.
        /// </summary>
        All = -1,
        /// <summary>
        /// Test COSEM objects.
        /// </summary>
        Cosem = 1,
        /// <summary>
        /// Test connection initialization.
        /// </summary>
        Init = 2
    }

    public class GXSettings
    {
        public IGXMedia media = null;
        public TraceLevel trace = TraceLevel.Info;
        public bool iec = false;
        public string path = null;
        public GXDLMSXmlClient client = new GXDLMSXmlClient(TranslatorOutputType.SimpleXml);
        /// <summary>
        /// Executed conformance tests.
        /// </summary>
        public ConformanceTest tests = ConformanceTest.Cosem;
        /// <summary>
        /// Excluded object types.
        /// </summary>
        public List<ObjectType> excludedObjects = new List<ObjectType>();

        internal static int GetParameters(string[] args, GXSettings settings)
        {
            List<GXCmdParameter> parameters = GXCommon.GetParameters(args, "bh:p:c:s:r:it:a:p:wP:x:S:e:C:");
            GXNet net = null;
            foreach (GXCmdParameter it in parameters)
            {
                switch (it.Tag)
                {
                    case 'w':
                        settings.client.InterfaceType = InterfaceType.WRAPPER;
                        break;
                    case 'r':
                        if (string.Compare(it.Value, "sn", true) == 0)
                        {
                            settings.client.UseLogicalNameReferencing = false;
                        }
                        else if (string.Compare(it.Value, "ln", true) == 0)
                        {
                            settings.client.UseLogicalNameReferencing = true;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid reference option.");
                        }
                        break;
                    case 'h':
                        //Host address.
                        if (settings.media == null)
                        {
                            settings.media = new GXNet();
                        }
                        net = settings.media as GXNet;
                        net.HostName = it.Value;
                        break;
                    case 't':
                        //Trace.
                        try
                        {
                            settings.trace = (TraceLevel)Enum.Parse(typeof(TraceLevel), it.Value);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid Authentication option. (Error, Warning, Info, Verbose, Off)");
                        }
                        break;
                    case 'p':
                        //Port.
                        if (settings.media == null)
                        {
                            settings.media = new GXNet();
                        }
                        net = settings.media as GXNet;
                        net.Port = int.Parse(it.Value);
                        break;
                    case 'P'://Password
                        settings.client.Password = ASCIIEncoding.ASCII.GetBytes(it.Value);
                        break;
                    case 'i':
                        //IEC.
                        settings.iec = true;
                        break;
                    case 'e':
                        //Exclude object type.
                        foreach (string ot in it.Value.Split(','))
                        {
                            settings.excludedObjects.Add((ObjectType)Enum.Parse(typeof(ObjectType), ot));
                        }
                        break;
                    case 'S'://Serial Port
                        settings.media = new GXSerial();
                        GXSerial serial = settings.media as GXSerial;
                        string[] tmp = it.Value.Split(':');
                        serial.PortName = tmp[0];
                        if (tmp.Length > 1)
                        {
                            serial.BaudRate = int.Parse(tmp[1]);
                            serial.DataBits = int.Parse(tmp[2].Substring(0, 1));
                            serial.Parity = (Parity)Enum.Parse(typeof(Parity), tmp[2].Substring(1, tmp[2].Length - 2));
                            serial.StopBits = (StopBits)int.Parse(tmp[2].Substring(tmp[2].Length - 1, 1));
                        }
                        else
                        {
                            serial.BaudRate = 9600;
                            serial.DataBits = 8;
                            serial.Parity = Parity.None;
                            serial.StopBits = StopBits.One;
                        }
                        break;
                    case 'a':
                        try
                        {
                            settings.client.Authentication = (Authentication)Enum.Parse(typeof(Authentication), it.Value);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid Authentication option. (None, Low, High, HighMd5, HighSha1, HighGmac, HighSha256)");
                        }
                        break;
                    case 'C':
                        settings.tests = 0;
                        foreach (string ot in it.Value.Split(','))
                        {
                            settings.tests |= (ConformanceTest)Enum.Parse(typeof(ConformanceTest), ot);
                        }
                        break;
                    case 'o':
                        break;
                    case 'c':
                        settings.client.ClientAddress = int.Parse(it.Value);
                        break;
                    case 's':
                        settings.client.ServerAddress = int.Parse(it.Value);
                        break;
                    case 'x':
                        settings.path = it.Value;
                        break;
                    case '?':
                        switch (it.Tag)
                        {
                            case 'c':
                                throw new ArgumentException("Missing mandatory client option.");
                            case 's':
                                throw new ArgumentException("Missing mandatory server option.");
                            case 'h':
                                throw new ArgumentException("Missing mandatory host name option.");
                            case 'p':
                                throw new ArgumentException("Missing mandatory port option.");
                            case 'r':
                                throw new ArgumentException("Missing mandatory reference option.");
                            case 'a':
                                throw new ArgumentException("Missing mandatory authentication option.");
                            case 'S':
                                throw new ArgumentException("Missing mandatory Serial port option.\n");
                            case 't':
                                throw new ArgumentException("Missing mandatory trace option.\n");
                            case 'e':
                                throw new ArgumentException("Missing mandatory exclude object type option.\n");
                            default:
                                ShowHelp();
                                return 1;
                        }
                    default:
                        ShowHelp();
                        return 1;
                }
            }
            if (settings.media == null)
            {
                ShowHelp();
                return 1;
            }
            return 0;
        }

        static void ShowHelp()
        {
            Console.WriteLine("Run Gurux Conformance Tests for DLMS/COSEM device.");
            Console.WriteLine("Read more: https://www.gurux.fi/GuruxCTT");
            Console.WriteLine("Gurux.Conformance.Test -h [Meter IP Address] -p [Meter Port No] -c 16 -s 1 -r SN");
            Console.WriteLine(" -h \t host name or IP address.");
            Console.WriteLine(" -p \t port number or name (Example: 1000).");
            Console.WriteLine(" -S [COM1:9600:8None1]\t serial port.");
            Console.WriteLine(" -i IEC is a start protocol.");
            Console.WriteLine(" -a \t Authentication (None, Low, High).");
            Console.WriteLine(" -P \t Password for authentication.");
            Console.WriteLine(" -c \t Client address. (Default: 16)");
            Console.WriteLine(" -s \t Server address. (Default: 1)");
            Console.WriteLine(" -n \t Server address as serial number.");
            Console.WriteLine(" -r [sn, sn]\t Short name or Logican Name (default) referencing is used.");
            Console.WriteLine(" -w WRAPPER profile is used. HDLC is default.");
            Console.WriteLine(" -t [Error, Warning, Info, Verbose] Trace messages.");
            Console.WriteLine(" -x input XML file.");
            Console.WriteLine(" -C Executed Conformance tests (All, Init, Read)");
            Console.WriteLine(" -e Exclude object types (Data, Register, etc.)");
            Console.WriteLine("Example:");
            Console.WriteLine("Gurux Conformance Tests using TCP/IP connection.");
            Console.WriteLine("GuruxDlmsSample -r SN -c 16 -s 1 -h [Meter IP Address] -p [Meter Port No]");
            Console.WriteLine("Gurux Conformance Tests using serial port connection.");
            Console.WriteLine("GuruxDlmsSample -r SN -c 16 -s 1 -S COM1:9600:8None1 -i");
            Console.WriteLine("GuruxDlmsSample -S COM1:9600:8None1 -c 16 -s 1 -a Low -P [password]");
        }

    }
}
