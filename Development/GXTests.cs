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

using Gurux.DLMS.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Gurux.DLMS.Enums;
using System.IO;
using System.Xml;
using Gurux.Common;
using Gurux.DLMS.Reader;
using System.Text;

namespace Gurux.DLMS.Conformance.Test
{
    /// <summary>
    /// This class includes tests implemantations.
    /// </summary>
    class GXTests
    {
        /// <summary>
        /// Get tests for COSEM objects.
        /// </summary>
        /// <returns>COSEM object tests.</returns>
        private static string[] GetTests()
        {
            return typeof(GXTests).Assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith("Gurux.DLMS.Conformance.Test.Tests.Objects.") && r.EndsWith(".xml"))
                .ToArray();
        }

        /// <summary>
        /// Get Logical Name referencing tests.
        /// </summary>
        /// <returns>Logical Name referencing tests.</returns>
        private static string[] GetLNTests()
        {
            return typeof(GXTests).Assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith("Gurux.DLMS.Conformance.Test.Tests.LN.") && r.EndsWith(".xml"))
                .ToArray();
        }

        /// <summary>
        /// Get Short Name referencing tests.
        /// </summary>
        /// <returns>Short Name referencing tests.</returns>
        private static string[] GetSNTests()
        {
            return typeof(GXTests).Assembly.GetManifestResourceNames()
                .Where(r => r.StartsWith("Gurux.DLMS.Conformance.Test.Tests.SN.") && r.EndsWith(".xml"))
                .ToArray();
        }

        /// <summary>
        /// Execute basic tests.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="output">Generated output.</param>
        public static bool Basic(GXSettings settings, GXOutput output)
        {
            Reader.GXDLMSReader reader = new Reader.GXDLMSReader(settings.client, settings.media, settings.trace, settings.iec);
            reader.WaitTime = settings.WaitTime;
            try
            {
                settings.media.Open();
            }
            catch (System.Net.Sockets.SocketException e)
            {
                //If failed to make connection to the meter.
                output.Errors.Add("Failed to connect to the meter: " + e.Message);
                output.MakeReport();
                return false;
            }
            if (settings.trace > TraceLevel.Error)
            {
                Console.WriteLine("------------------------------------------------------------");
                Console.WriteLine("Initialize connection.");
            }
            reader.InitializeConnection();
            if (settings.trace > TraceLevel.Error)
            {
                Console.WriteLine("Get association view.");
            }
            reader.GetAssociationView(false);
            if (settings.client.UseLogicalNameReferencing)
            {
                output.PreInfo.Add("Testing using Logical Name referencing.");
            }
            else
            {
                output.PreInfo.Add("Testing using Short Name referencing.");
            }
            output.PreInfo.Add("Authentication level: " + settings.client.Authentication);
            output.PreInfo.Add("Total amount of objects: " + settings.client.Objects.Count.ToString());
            if (settings.client.UseLogicalNameReferencing)
            {
                if (settings.trace > TraceLevel.Error)
                {
                    Console.WriteLine("Finding Logical Device Name and SAP.");
                }
                GXDLMSObject ldn = settings.client.Objects.FindByLN(ObjectType.None, "0.0.42.0.0.255");
                GXDLMSObjectCollection saps = settings.client.Objects.GetObjects(ObjectType.SapAssignment);
                if (ldn == null && saps.Count == 0)
                {
                    output.Errors.Add("Logical Device Name or SAP is not implemented. Read more: GB: 4.1.8.4.");
                }
                if (settings.trace > TraceLevel.Error)
                {
                    if (ldn != null)
                    {
                        reader.Read(ldn, 2);
                        output.PreInfo.Add("Meter Logical Device Name is: " + (ldn as GXDLMSData).Value.ToString() + ".");
                    }
                    if (saps.Count != 0)
                    {
                        output.PreInfo.Add("SAP is not implemented.");
                    }
                }
            }
            //Check OBIS codes.
            foreach (GXDLMSObject it in settings.client.Objects)
            {
                if (it.Description == "Invalid")
                {
                    output.Errors.Add("Invalid OBIS code " + it.LogicalName + " for <a target=\"_blank\" href=http://www.gurux.fi/" + it.GetType().FullName + ">" + it.ObjectType + "</a>.");
                    if (settings.trace > TraceLevel.Warning)
                    {
                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine(it.LogicalName + ": Invalid OBIS code.");
                    }
                }
            }

            //Read structures of Cosem objects.
            List<KeyValuePair<string, List<GXDLMSXmlPdu>>> cosemTests = new List<KeyValuePair<string, List<GXDLMSXmlPdu>>>();
            GXDLMSTranslator translator = new GXDLMSTranslator(TranslatorOutputType.SimpleXml);
            foreach (string it in GetTests())
            {
                using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream(it))
                using (StreamReader sr = new StreamReader(stream))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(sr.ReadToEnd());
                    XmlNodeList list = doc.SelectNodes("/Messages/GetRequest/GetRequestNormal");
                    ObjectType ot = ObjectType.None;
                    foreach (XmlNode node in list)
                    {
                        ot = (ObjectType)int.Parse(node.SelectNodes("AttributeDescriptor/ClassId")[0].Attributes["Value"].Value);
                        //If this object type is skipped.
                        if (settings.excludedObjects.Contains(ot))
                        {
                            output.Info.Add("Skipping " + ot.ToString() + " object types.");
                            break;
                        }
                        int index = int.Parse(node.SelectNodes("AttributeDescriptor/AttributeId")[0].Attributes["Value"].Value);
                        //Update logical name.
                        foreach (GXDLMSObject obj in settings.client.Objects.GetObjects(ot))
                        {
                            if ((obj.GetAccess(index) & AccessMode.Read) != 0)
                            {
                                string tmp = GXCommon.ToHex(LogicalNameToBytes(obj.LogicalName), false);
                                foreach (XmlNode n in list)
                                {
                                    XmlAttribute ln = n.SelectNodes("AttributeDescriptor/InstanceId")[0].Attributes["Value"];
                                    ln.Value = tmp;
                                }
                                cosemTests.Add(new KeyValuePair<string, List<GXDLMSXmlPdu>>(ot.ToString(), settings.client.LoadXml(doc.InnerXml)));
                            }
                        }
                        break;
                    }
                }
            }
            foreach (KeyValuePair<string, List<GXDLMSXmlPdu>> it in cosemTests)
            {
                try
                {
                    Execute(settings, reader, it.Key, it.Value, output);
                }
                catch (Exception ex)
                {
                    if (settings.trace > TraceLevel.Off)
                    {
                        Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            List<ObjectType> unknownDataTypes = new List<ObjectType>();
            foreach (GXDLMSObject o in settings.client.Objects)
            {
                if (!unknownDataTypes.Contains(o.ObjectType))
                {
                    bool found = false;
                    foreach (KeyValuePair<string, List<GXDLMSXmlPdu>> t in cosemTests)
                    {
                        if (o.ObjectType.ToString() == t.Key)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        unknownDataTypes.Add(o.ObjectType);
                        output.Warnings.Add("<a target=\"_blank\" href=http://www.gurux.fi/" + o.GetType().FullName + ">" + o.ObjectType + "</a> is not tested.");
                    }
                }
            }
            //Check InactivityTimeout.
            bool fail = false;
            int rc = reader.RetryCount;
            int wt = reader.WaitTime;
            int inactivityTimeout;
            if (settings.client.InterfaceType == InterfaceType.HDLC)
            {
                GXDLMSHdlcSetup s = (GXDLMSHdlcSetup)settings.client.Objects.GetObjects(ObjectType.IecHdlcSetup)[0];
                s.InactivityTimeout = 0;
                reader.Read(s, 8);
                inactivityTimeout = s.InactivityTimeout;
                output.PreInfo.Add("HdlcSetup default inactivity timeout value is " + inactivityTimeout + " seconds.");
                if ((s.GetAccess(8) & AccessMode.Write) != 0)
                {
                    //Wait second.
                    s.InactivityTimeout = 1;
                    reader.Write(s, 8);
                    Thread.Sleep(2000);
                    try
                    {
                        reader.WaitTime = 1000;
                        reader.RetryCount = 0;
                        reader.Read(s, 8);
                    }
                    catch (Exception)
                    {
                        //This should fails.
                        fail = true;
                    }
                    reader.InitializeConnection();
                    s.InactivityTimeout = inactivityTimeout;
                    reader.Write(s, 8);
                    if (!fail)
                    {
                        output.Errors.Add("HdlcSetup failed. InactivityTimeout don't work.");
                    }
                }
                else
                {
                    output.PreInfo.Add("HdlcSetup inactivity timeout is not tested.");
                }
            }
            else
            {
                GXDLMSTcpUdpSetup s = (GXDLMSTcpUdpSetup)settings.client.Objects.GetObjects(ObjectType.TcpUdpSetup)[0];
                s.InactivityTimeout = 0;
                reader.Read(s, 6);
                inactivityTimeout = s.InactivityTimeout;
                output.PreInfo.Add("TcpUdpSetup default inactivity timeout value is " + inactivityTimeout + " seconds.");
                if ((s.GetAccess(6) & AccessMode.Write) != 0)
                {
                    //Wait second.
                    s.InactivityTimeout = 1;
                    reader.Write(s, 6);
                    Thread.Sleep(2000);
                    try
                    {
                        reader.WaitTime = 1000;
                        reader.RetryCount = 0;
                        reader.Read(s, 6);
                    }
                    catch (Exception)
                    {
                        //This should fails.
                        fail = true;
                    }
                    reader.InitializeConnection();
                    s.InactivityTimeout = inactivityTimeout;
                    reader.Write(s, 6);
                    if (!fail)
                    {
                        output.Errors.Add("TcpUdpSetup failed. InactivityTimeout don't work.");
                    }
                }
                else
                {
                    output.PreInfo.Add("TcpUdpSetup inactivity timeout is not tested.");
                }
            }
            reader.WaitTime = wt;
            reader.RetryCount = rc;
            output.MakeReport();
            return true;
        }

        /// <summary>
        /// Connect to the meter and try to send password in two part.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="output">Generated output.</param>
        public static void PasswordInTwoPart(GXSettings settings, GXOutput output)
        {
            if (settings.client.InterfaceType == InterfaceType.HDLC)
            {
                Reader.GXDLMSReader reader = new Reader.GXDLMSReader(settings.client, settings.media, settings.trace, settings.iec);
                //GXDLMSClient cl = new GXDLMSClient(true, 16, 1, Authentication.Low, null, InterfaceType.HDLC);
                //Reader.GXDLMSReader reader = new Reader.GXDLMSReader(cl, settings.media, settings.trace, settings.iec);
                reader.WaitTime = settings.WaitTime;
                settings.media.Open();
                settings.client.Limits.MaxInfoRX = settings.client.Limits.MaxInfoTX = 64;
                reader.SNRMRequest();
                settings.client.Authentication = Authentication.Low;
                settings.client.Password = ASCIIEncoding.ASCII.GetBytes("Gurux");
                reader.AarqRequest();
            }
        }

        /// <summary>
        /// Connect to the meter and try to send HDLC frame that is too big.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="output">Generated output.</param>
        public static void InvalidHdlcFrame(GXSettings settings, GXOutput output)
        {
            if (settings.client.InterfaceType == InterfaceType.HDLC)
            {
                Reader.GXDLMSReader reader = new Reader.GXDLMSReader(settings.client, settings.media, settings.trace, settings.iec);
                reader.WaitTime = settings.WaitTime;
                settings.media.Open();
                settings.client.Limits.MaxInfoRX = settings.client.Limits.MaxInfoTX = 64;
                reader.SNRMRequest();
                byte[] data = GXDLMSTranslator.HexToBytes("7E A0 41 03 21 10 B1 EA E6 E6 00 60 33 A1 09 06 07 60 85 74 05 08 01 01 8A 02 07 80 8B 07 60 85 74 05 08 02 01 AC 07 80 05 47 75 72 75 78 BE 10 04 0E 01 00 00 00 06 5F 1F 04 00 00 1E 1D FF FF ED 2C 7E");
                GXReplyData reply = new GXReplyData();
                reader.ReadDataBlock(data, reply);
            }
        }

        /// <summary>
        /// Execute connection tests.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="output">Generated output.</param>
        public static bool Init(GXSettings settings, GXOutput output)
        {
            //            PasswordInTwoPart(settings, output);
            //            InvalidHdlcFrame(settings, output);

            List<string> files = new List<string>();
            Reader.GXDLMSReader reader = null;
            if (settings.client.UseLogicalNameReferencing)
            {
                files.AddRange(GetLNTests());
            }
            else
            {
                files.AddRange(GetSNTests());
            }
            //Read additional tests.
            foreach (string name in files)
            {
                List<GXDLMSXmlPdu> actions;
                using (Stream stream = typeof(Program).Assembly.GetManifestResourceStream(name))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        actions = settings.client.Load(sr);
                    }
                }
                if (actions.Count == 0)
                {
                    continue;
                }
                try
                {
                    reader = new Reader.GXDLMSReader(settings.client, settings.media, settings.trace, settings.iec);
                    reader.WaitTime = settings.WaitTime;
                    try
                    {
                        settings.media.Open();
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        //If failed to make connection to the meter.
                        output.Errors.Add("Failed to connect to the meter: " + e.Message);
                        output.MakeReport();
                        return false;
                    }
                    //Send SNRM if not in xml.
                    if (settings.client.InterfaceType == InterfaceType.HDLC)
                    {
                        if (!ContainsCommand(actions, Command.Snrm))
                        {
                            reader.SNRMRequest();
                        }
                    }

                    //Send AARQ if not in xml.
                    if (!ContainsCommand(actions, Command.Aarq))
                    {
                        if (!ContainsCommand(actions, Command.Snrm))
                        {
                            reader.AarqRequest();
                        }
                    }
                    Execute(settings, reader, name, actions, output);
                }
                catch (Exception ex)
                {
                    output.Errors.Add(name + " failed.");
                }
                finally
                {
                    //Send AARQ if not in xml.
                    if (!ContainsCommand(actions, Command.DisconnectRequest))
                    {
                        reader.Disconnect();
                    }
                    else
                    {
                        settings.media.Close();
                    }
                }
            }
            output.MakeReport();
            return true;
        }

        /// <summary>
        /// Execute extra tests.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="output">Generated output.</param>
        public static bool Extra(GXSettings settings, GXOutput output)
        {
            List<string> files = new List<string>();
            FileAttributes attr = File.GetAttributes(settings.path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                files.AddRange(Directory.GetFiles(settings.path, "*.xml"));
            }
            else
            {
                files.Add(settings.path);
            }
            Reader.GXDLMSReader reader = null;
            //Read additional tests.
            string name;
            foreach (string file in files)
            {
                name = Path.GetFileNameWithoutExtension(file);
                if (settings.trace > TraceLevel.Warning)
                {
                    Console.WriteLine("------------------------------------------------------------");
                    Console.WriteLine(name);
                }
                List<GXDLMSXmlPdu> actions = settings.client.Load(file);
                if (actions.Count == 0)
                {
                    continue;
                }
                try
                {
                    reader = new Reader.GXDLMSReader(settings.client, settings.media, settings.trace, settings.iec);
                    reader.WaitTime = settings.WaitTime;
                    try
                    {
                        settings.media.Open();
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        //If failed to make connection to the meter.
                        output.Errors.Add("Failed to connect to the meter: " + e.Message);
                        output.MakeReport();
                        return false;
                    }
                    //Send SNRM if not in xml.
                    if (settings.client.InterfaceType == InterfaceType.HDLC)
                    {
                        if (!ContainsCommand(actions, Command.Snrm))
                        {
                            reader.SNRMRequest();
                        }
                    }

                    //Send AARQ if not in xml.
                    if (!ContainsCommand(actions, Command.Aarq))
                    {
                        if (!ContainsCommand(actions, Command.Snrm))
                        {
                            reader.AarqRequest();
                        }
                    }
                    Execute(settings, reader, name, actions, output);
                }
                catch (Exception ex)
                {
                    output.Errors.Add(name + " failed.");
                }
                finally
                {
                    //Send AARQ if not in xml.
                    if (!ContainsCommand(actions, Command.DisconnectRequest))
                    {
                        reader.Disconnect();
                    }
                    else
                    {
                        settings.media.Close();
                    }
                }
            }
            output.MakeReport();
            return true;
        }

        private static string GetLogicalName(string ln)
        {
            byte[] buff = GXCommon.HexToBytes(ln);
            return (buff[0] & 0xFF) + "." + (buff[1] & 0xFF) + "." + (buff[2] & 0xFF) + "." +
                   (buff[3] & 0xFF) + "." + (buff[4] & 0xFF) + "." + (buff[5] & 0xFF);
        }

        private static void Execute(GXSettings settings, GXDLMSReader reader, string name, List<GXDLMSXmlPdu> actions, GXOutput output)
        {
            GXReplyData reply = new GXReplyData();
            string ln = null;
            int index = 0;
            ObjectType ot = ObjectType.None;
            List<KeyValuePair<ObjectType, int>> succeeded = new List<KeyValuePair<ObjectType, int>>();
            foreach (GXDLMSXmlPdu it in actions)
            {
                if (it.Command == Command.Snrm && settings.client.InterfaceType == InterfaceType.WRAPPER)
                {
                    continue;
                }
                if (it.Command == Command.DisconnectRequest && settings.client.InterfaceType == InterfaceType.WRAPPER)
                {
                    break;
                }
                //Send
                if (it.IsRequest())
                {
                    XmlNode i = it.XmlNode.SelectNodes("GetRequestNormal")[0];
                    if (i == null)
                    {
                        ot = ObjectType.None;
                        index = 0;
                        ln = null;
                    }
                    else
                    {
                        ot = (ObjectType)int.Parse(i.SelectNodes("AttributeDescriptor/ClassId")[0].Attributes["Value"].Value);
                        index = int.Parse(i.SelectNodes("AttributeDescriptor/AttributeId")[0].Attributes["Value"].Value);
                        ln = (i.SelectNodes("AttributeDescriptor/InstanceId")[0].Attributes["Value"].Value);
                        ln = GetLogicalName(ln);
                    }
                    reply.Clear();
                    if (settings.trace > TraceLevel.Warning)
                    {
                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine(it.ToString());
                    }
                    byte[][] tmp = settings.client.PduToMessages(it);
                    reader.ReadDataBlock(tmp, reply);
                }
                else if (reply.Data.Size != 0)
                {
                    List<string> list = it.Compare(reply.ToString());
                    if (list.Count != 0)
                    {
                        if (ot == ObjectType.None)
                        {
                            foreach (string err in list)
                            {
                                output.Errors.Add(err);
                            }
                        }
                        else
                        {
                            //   output.Errors.Add("<div class=\"description\">\r\n");
                            output.Errors.Add("<a target=\"_blank\" href=http://www.gurux.fi/Gurux.DLMS.Objects.GXDLMS" + ot + ">" + ot + "</a> " + ln + " attribute " + index + " is <div class=\"tooltip\">invalid.");
                            output.Errors.Add("<span class=\"tooltiptext\">");
                            output.Errors.Add("Expected:</b><br/>");
                            output.Errors.Add(it.PduAsXml.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "<br/>"));
                            output.Errors.Add("<br/><b>Actual:</b><br/>");
                            output.Errors.Add(reply.ToString().Replace("<", "&lt;").Replace(">", "&gt;").Replace("\r\n", "<br/>"));
                            output.Errors.Add("</span></div>");
                            //   output.Errors.Add("</div>");
                        }
                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine("Test" + name + "failed. Invalid reply: " + string.Join("\n", list.ToArray()));
                    }
                    else
                    {
                        if (ot == ObjectType.None)
                        {
                            output.Info.Add(name + " succeeded.");
                        }
                        else
                        {
                            succeeded.Add(new KeyValuePair<ObjectType, int>(ot, index));
                        }
                    }
                    if (settings.trace > TraceLevel.Warning)
                    {
                        Console.WriteLine("------------------------------------------------------------");
                        Console.WriteLine(reply.ToString());
                    }
                }
            }
            if (succeeded.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<div class=\"tooltip\">" + ln);
                sb.Append("<span class=\"tooltiptext\">");
                foreach (var it in succeeded)
                {
                    sb.Append("Index: " + it.Value + "<br/>");
                }
                sb.Append("</span></div>");
                sb.Append("&nbsp;" + settings.converter.GetDescription(ln, succeeded[0].Key)[0] + "&nbsp;" + "<a target=\"_blank\" href=http://www.gurux.fi/Gurux.DLMS.Objects.GXDLMS" + ot + ">" + ot + "</a>."); 
                output.Info.Add(sb.ToString());
            }
        }

        static bool ContainsCommand(List<GXDLMSXmlPdu> actions, Command command)
        {
            foreach (GXDLMSXmlPdu it in actions)
            {
                if (it.Command == command)
                {
                    return true;
                }
            }
            return false;
        }


        internal static byte[] LogicalNameToBytes(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new byte[6];
            }
            string[] items = value.Split('.');
            // If data is string.
            if (items.Length != 6)
            {
                throw new ArgumentException("Invalid Logical Name");
            }
            byte[] buff = new byte[6];
            byte pos = 0;
            foreach (string it in items)
            {
                buff[pos] = Convert.ToByte(it);
                ++pos;
            }
            return buff;
        }
    }
}
