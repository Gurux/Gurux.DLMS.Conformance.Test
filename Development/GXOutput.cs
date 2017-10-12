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
using System.Collections.Generic;
using System.IO;
using System.Web.UI;

namespace Gurux.DLMS.Conformance.Test
{
    class GXOutput
    {
        public HtmlTextWriter writer;
        public List<string> Errors = new List<string>();
        public List<string> Warnings = new List<string>();
        public List<string> PreInfo = new List<string>();
        public List<string> Info = new List<string>();
        private string file;
        public string GetName()
        {
            return file;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXOutput()
        {
            //Get new name for the report.
            file = Path.Combine(Path.GetDirectoryName(typeof(GXOutput).Assembly.Location), "TestResults");
            if (!Directory.Exists(file))
            {
                Directory.CreateDirectory(file);
            }
            string[] list = Directory.GetFiles(file, "Gurux.Conformance.Test.Report*.htm");
            int version = list.Length;
            foreach (string it in list)
            {
                FileInfo fi = new FileInfo(it);
                string tmp = fi.Name.Substring(29, fi.Name.Length - 33);
                if (!string.IsNullOrEmpty(tmp))
                {
                    int t = int.Parse(tmp);
                    if (t > version)
                    {
                        version = t + 1;
                    }
                }
            }
            if (version == 0)
            {
                file = Path.Combine(file, "Gurux.Conformance.Test.Report.htm");
            }
            else
            {
                file = Path.Combine(file, "Gurux.Conformance.Test.Report" + version + ".htm");
            }
            Stream stream = File.Open(file, FileMode.Create);
            writer = new HtmlTextWriter(new StreamWriter(stream));
            string classValue = "ClassName";
            string urlValue = "http://www.gurux.org/";
            // The important part:
            writer.AddAttribute(HtmlTextWriterAttribute.Class, classValue);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.Write("<table width=\"100%\">");
            writer.Write("<tr>");
            writer.Write("<td><center><h1>Gurux Conformance Test Report</h1></center></td>");
            writer.Write("</tr>");
            writer.Write("</table>");
        }

        public void MakeReport()
        {
            foreach (string it in PreInfo)
            {
                writer.WriteLine(it);
                writer.Write("<br/>");
            }
            // Begin Errors.
            writer.RenderBeginTag("Errors");
            writer.Write("<h2>Errors</h2>");
            if (Errors.Count == 0)
            {
                writer.Write("No errors occurred.<br/>");
            }
            else
            {
                foreach (string it in Errors)
                {
                    writer.Write(it);
                    writer.Write("<br/>");
                }
            }
            writer.RenderEndTag();
            // Begin Warnings.
            writer.RenderBeginTag("Warnings");
            writer.Write("<h2>Warnings</h2>");
            if (Warnings.Count == 0)
            {
                writer.Write("No warnings occurred.<br/>");
            }
            else
            {
                foreach (string it in Warnings)
                {
                    writer.WriteLine(it);
                    writer.Write("<br/>");
                }
            }
            writer.RenderEndTag();
            // Begin Info.
            writer.RenderBeginTag("Info");
            writer.Write("<h2>Info</h2>");
            writer.RenderEndTag();
            foreach (string it in Info)
            {
                writer.WriteLine(it);
                writer.Write("<br/>");
            }
            PreInfo.Clear();
            Errors.Clear();
            Warnings.Clear();
            Info.Clear();
        }
    }
}
