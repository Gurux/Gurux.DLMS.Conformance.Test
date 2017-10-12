using Gurux.Common;
using Gurux.DLMS.Enums;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.Net;
using Gurux.Serial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gurux.DLMS.Conformance.Test
{
    public partial class GXProperties : Form
    {
        Form MediaPropertiesForm = null;
        IGXMedia SelectedMedia;
        GXSettings Target;

        public GXProperties(GXSettings settings)
        {
            Target = settings;
            Target.client.UseLogicalNameReferencing = false;
            InitializeComponent();
            LNSettings.Dock = SNSettings.Dock = DockStyle.Fill;
            SecurityCB.Items.AddRange(new object[] { Security.None, Security.Authentication,
                                      Security.Encryption, Security.AuthenticationEncryption
                                                   });
            NetProtocolCB.Items.AddRange(new object[] { NetworkType.Tcp, NetworkType.Udp });
            ServerAddressTypeCB.SelectedIndexChanged += new System.EventHandler(this.ServerAddressTypeCB_SelectedIndexChanged);
            NetworkSettingsGB.Width = this.Width - NetworkSettingsGB.Left;
            CustomSettings.Bounds = SerialSettingsGB.Bounds = TerminalSettingsGB.Bounds = NetworkSettingsGB.Bounds;
            ManufacturerCB.DrawMode = MediasCB.DrawMode = ServerAddressTypeCB.DrawMode = AuthenticationCB.DrawMode = DrawMode.OwnerDrawFixed;
            StartProtocolCB.Items.Add(StartProtocolType.IEC);
            StartProtocolCB.Items.Add(StartProtocolType.DLMS);

            GXNet net = new GXNet() { Protocol = NetworkType.Tcp };
            GXSerial serial = new GXSerial();
            MediasCB.Items.Add(net);
            MediasCB.Items.Add(serial);

            //Initialize serial settings.
            string[] ports = GXSerial.GetPortNames();
            this.SerialPortCB.Items.AddRange(ports);

            GXManufacturerCollection Manufacturers = new GXManufacturerCollection();
            if (GXManufacturerCollection.IsFirstRun())
            {
                if (MessageBox.Show(this, Properties.Resources.InstallManufacturersOnlineTxt, Properties.Resources.CTT, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {
                    GXManufacturerCollection.UpdateManufactureSettings();
                }
            }
            GXManufacturerCollection.ReadManufacturerSettings(Manufacturers);
            int pos = 0;
            foreach (GXManufacturer it in Manufacturers)
            {
                int index = this.ManufacturerCB.Items.Add(it);
                if (it.Identification == Properties.Settings.Default.SelectedManufacturer)
                {
                    pos = index;
                }
            }
            ManufacturerCB.SelectedIndex = pos;
            if (Properties.Settings.Default.WaitTime != 0)
            {
                WaitTimeTB.Value = Properties.Settings.Default.WaitTime;
            }

            if (Properties.Settings.Default.Media == "Net")
            {
                MediasCB.SelectedIndex = 0;
                net.Settings = Properties.Settings.Default.MediaSettings;
            }
            else
            {
                MediasCB.SelectedIndex = 1;
                serial.Settings = Properties.Settings.Default.MediaSettings;
            }
            if (SerialPortCB.Items.Count != 0)
            {
                SerialPortCB.SelectedItem = serial.PortName;
            }
            HostNameTB.Text = net.HostName;
            PortTB.Text = net.Port.ToString();
            NetProtocolCB.SelectedItem = net.Protocol;
            ShowConformance(Target.client.ProposedConformance);
        }

        private void ServerAddressTypeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GXServerAddress server = (GXServerAddress)ServerAddressTypeCB.SelectedItem;
            PhysicalServerAddressTB.Hexadecimal = server.HDLCAddress != HDLCAddressType.SerialNumber;
            this.PhysicalServerAddressTB.Value = Convert.ToDecimal(server.PhysicalAddress);
            this.LogicalServerAddressTB.Value = server.LogicalAddress;
            if (!PhysicalServerAddressTB.Hexadecimal)
            {
                PhysicalServerAddressLbl.Text = "Serial Number:";
            }
            else
            {
                PhysicalServerAddressLbl.Text = "Physical Server:";

            }
        }

        private void InitialSettingsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.gurux.fi/index.php?q=GXDLMSDirectorExample");
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateConformance()
        {
            Enums.Conformance c = (Enums.Conformance)0;
            if (UseLNCB.Checked)
            {
                if (GeneralProtectionCB.Checked)
                {
                    c |= Enums.Conformance.GeneralProtection;
                }
                if (GeneralBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.GeneralBlockTransfer;
                }
                if (Attribute0SetReferencingCB.Checked)
                {
                    c |= Enums.Conformance.Attribute0SupportedWithSet;
                }
                if (PriorityManagementCB.Checked)
                {
                    c |= Enums.Conformance.PriorityMgmtSupported;
                }
                if (Attribute0GetReferencingCB.Checked)
                {
                    c |= Enums.Conformance.Attribute0SupportedWithGet;
                }
                if (GetBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.BlockTransferWithGetOrRead;
                }
                if (SetBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.BlockTransferWithSetOrWrite;
                }
                if (ActionBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.BlockTransferWithAction;
                }
                if (MultipleReferencesCB.Checked)
                {
                    c |= Enums.Conformance.MultipleReferences;
                }
                if (DataNotificationCB.Checked)
                {
                    c |= Enums.Conformance.DataNotification;
                }
                if (AccessCB.Checked)
                {
                    c |= Enums.Conformance.Access;
                }
                if (GetCB.Checked)
                {
                    c |= Enums.Conformance.Get;
                }
                if (SetCB.Checked)
                {
                    c |= Enums.Conformance.Set;
                }
                if (SelectiveAccessCB.Checked)
                {
                    c |= Enums.Conformance.SelectiveAccess;
                }
                if (EventNotificationCB.Checked)
                {
                    c |= Enums.Conformance.EventNotification;
                }
                if (ActionCB.Checked)
                {
                    c |= Enums.Conformance.EventNotification;
                }
            }
            else
            {
                if (SNGeneralProtectionCB.Checked)
                {
                    c |= Enums.Conformance.GeneralProtection;
                }
                if (SNGeneralBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.GeneralBlockTransfer;
                }
                if (ReadCB.Checked)
                {
                    c |= Enums.Conformance.Read;
                }
                if (WriteCB.Checked)
                {
                    c |= Enums.Conformance.Write;
                }
                if (UnconfirmedWriteCB.Checked)
                {
                    c |= Enums.Conformance.UnconfirmedWrite;
                }
                if (ReadBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.BlockTransferWithGetOrRead;
                }
                if (WriteBlockTransferCB.Checked)
                {
                    c |= Enums.Conformance.BlockTransferWithSetOrWrite;
                }
                if (SNMultipleReferencesCB.Checked)
                {
                    c |= Enums.Conformance.MultipleReferences;
                }
                if (InformationReportCB.Checked)
                {
                    c |= Enums.Conformance.InformationReport;
                }
                if (SNDataNotificationCB.Checked)
                {
                    c |= Enums.Conformance.DataNotification;
                }
                if (ParameterizedAccessCB.Checked)
                {
                    c |= Enums.Conformance.ParameterizedAccess;
                }
            }
            Target.client.ProposedConformance = c;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //Check security settings.
                if ((Security)SecurityCB.SelectedItem != Security.None ||
                    ((GXAuthentication)AuthenticationCB.SelectedItem).Type == Authentication.HighGMAC)
                {
                    if (SystemTitleTB.Text.Trim().Length == 0)
                    {
                        throw new ArgumentException("Invalid system title.");
                    }
                    if (AuthenticationKeyTB.Text.Trim().Length == 0)
                    {
                        throw new ArgumentException("Invalid authentication key.");
                    }
                    if (BlockCipherKeyTB.Text.Trim().Length == 0)
                    {
                        throw new ArgumentException("Invalid block cipher key.");
                    }
                }
                GXServerAddress server = (GXServerAddress)ServerAddressTypeCB.SelectedItem;
                if (server.HDLCAddress == HDLCAddressType.SerialNumber && PhysicalServerAddressTB.Value == 0)
                {
                    throw new Exception("Invalid Serial Number.");
                }
                GXManufacturer man = (GXManufacturer)ManufacturerCB.SelectedItem;
                Target.client.Authentication = ((GXAuthentication)this.AuthenticationCB.SelectedItem).Type;
                if (Target.client.Authentication != Authentication.None)
                {
                    if (PasswordAsciiCb.Checked)
                    {
                        Target.client.Password = ASCIIEncoding.ASCII.GetBytes(PasswordTB.Text);
                    }
                    else
                    {
                        Target.client.Password = GXDLMSTranslator.HexToBytes(this.PasswordTB.Text);
                    }
                }
                else
                {
                    Target.client.Password = null;
                }
                Target.media = SelectedMedia;
                if (SelectedMedia is GXSerial)
                {
                    if (this.SerialPortCB.Text.Length == 0)
                    {
                        throw new Exception("Invalid serial port.");
                    }
                    ((GXSerial)SelectedMedia).PortName = this.SerialPortCB.Text;
                }
                else if (SelectedMedia is GXNet)
                {
                    if (this.HostNameTB.Text.Length == 0)
                    {
                        throw new Exception("Invalid host name.");
                    }
                    ((GXNet)SelectedMedia).HostName = this.HostNameTB.Text;
                    int port;
                    if (!Int32.TryParse(this.PortTB.Text, out port))
                    {
                        throw new Exception("Invalid port number.");
                    }
                    ((GXNet)SelectedMedia).Port = port;
                    ((GXNet)SelectedMedia).Protocol = (NetworkType)NetProtocolCB.SelectedItem;
                }
                Properties.Settings.Default.Media = SelectedMedia.MediaType;
                Properties.Settings.Default.MediaSettings = SelectedMedia.Settings;
                Properties.Settings.Default.SelectedManufacturer = man.Identification;
                Properties.Settings.Default.WaitTime = Convert.ToInt32(WaitTimeTB.Value);
                Target.WaitTime = Properties.Settings.Default.WaitTime * 1000;
                GXAuthentication authentication = (GXAuthentication)AuthenticationCB.SelectedItem;
                HDLCAddressType HDLCAddressing = ((GXServerAddress)ServerAddressTypeCB.SelectedItem).HDLCAddress;
                Properties.Settings.Default.HDLCAddressing = (int)HDLCAddressing;
                Properties.Settings.Default.ClientAddress = Target.client.ClientAddress = Convert.ToInt32(ClientAddTB.Value);
                if (HDLCAddressing == HDLCAddressType.SerialNumber)
                {
                    int address = Convert.ToInt32(PhysicalServerAddressTB.Value);
                    Properties.Settings.Default.PhysicalServerAddress = address;
                    Target.client.ServerAddress = GXDLMSClient.GetServerAddress(address);
                }
                else if (HDLCAddressing == HDLCAddressType.Default)
                {
                    Properties.Settings.Default.PhysicalServerAddress = Convert.ToInt32(PhysicalServerAddressTB.Value);
                    Properties.Settings.Default.LogicalServerAddress = Convert.ToInt32(LogicalServerAddressTB.Value);
                    Target.client.ServerAddress = GXDLMSClient.GetServerAddress(Properties.Settings.Default.LogicalServerAddress,
                        Properties.Settings.Default.PhysicalServerAddress);
                }
                Target.client.UseLogicalNameReferencing = this.UseLNCB.Checked;
                Target.iec = (StartProtocolType)this.StartProtocolCB.SelectedItem == StartProtocolType.IEC;
                Target.client.Ciphering.Security = (Security)SecurityCB.SelectedItem;
                Target.client.Ciphering.SystemTitle = GetAsHex(SystemTitleTB.Text, SystemTitleAsciiCb.Checked);
                Target.client.Ciphering.BlockCipherKey = GetAsHex(BlockCipherKeyTB.Text, BlockCipherKeyAsciiCb.Checked);
                Target.client.Ciphering.AuthenticationKey = GetAsHex(AuthenticationKeyTB.Text, AuthenticationKeyAsciiCb.Checked);
                Target.client.Ciphering.InvocationCounter = UInt32.Parse(InvocationCounterTB.Text);
                Target.client.CtoSChallenge = GXCommon.HexToBytes(ChallengeTB.Text);
                if (man.UseIEC47)
                {
                    Target.client.InterfaceType = InterfaceType.WRAPPER;
                }
                UpdateConformance();
                Properties.Settings.Default.Save();
            }
            catch (Exception Ex)
            {
                this.DialogResult = DialogResult.None;
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static byte[] GetAsHex(string value, bool ascii)
        {
            if (ascii)
            {
                return ASCIIEncoding.ASCII.GetBytes(value);
            }
            return GXCommon.HexToBytes(value);
        }

        private void ManufacturerCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GXManufacturer man = (GXManufacturer)ManufacturerCB.SelectedItem;
                StartProtocolCB.SelectedItem = man.StartProtocol;
                this.ClientAddTB.Value = man.GetActiveAuthentication().ClientAddress;
                AuthenticationCB.Items.Clear();
                foreach (GXAuthentication it in man.Settings)
                {
                    int pos = AuthenticationCB.Items.Add(it);
                    if ((int)it.Type == Properties.Settings.Default.Authentication)
                    {
                        this.AuthenticationCB.SelectedIndex = pos;
                    }
                }
                ServerAddressTypeCB.Items.Clear();
                HDLCAddressType type = (HDLCAddressType)Properties.Settings.Default.HDLCAddressing;
                foreach (GXServerAddress it in ((GXManufacturer)ManufacturerCB.SelectedItem).ServerSettings)
                {
                    ServerAddressTypeCB.Items.Add(it);
                    if (it.HDLCAddress == type)
                    {
                        ServerAddressTypeCB.SelectedItem = it;
                    }
                }
                UpdateStartProtocol();
                SecurityCB.SelectedItem = man.Security;
                SystemTitleAsciiCb.CheckedChanged -= SystemTitleAsciiCb_CheckedChanged;
                BlockCipherKeyAsciiCb.CheckedChanged -= BlockCipherKeyAsciiCb_CheckedChanged;
                AuthenticationKeyAsciiCb.CheckedChanged -= AuthenticationKeyAsciiCb_CheckedChanged;

                SystemTitleAsciiCb.Checked = IsAscii(man.SystemTitle);
                if (SystemTitleAsciiCb.Checked)
                {
                    SystemTitleTB.Text = ASCIIEncoding.ASCII.GetString(man.SystemTitle);
                }
                else
                {
                    SystemTitleTB.Text = GXCommon.ToHex(man.SystemTitle, true);
                }

                BlockCipherKeyAsciiCb.Checked = IsAscii(man.BlockCipherKey);
                if (BlockCipherKeyAsciiCb.Checked)
                {
                    SystemTitleTB.Text = ASCIIEncoding.ASCII.GetString(man.BlockCipherKey);
                }
                else
                {
                    BlockCipherKeyTB.Text = GXCommon.ToHex(man.BlockCipherKey, true);
                }

                AuthenticationKeyAsciiCb.Checked = IsAscii(man.AuthenticationKey);
                if (AuthenticationKeyAsciiCb.Checked)
                {
                    SystemTitleTB.Text = ASCIIEncoding.ASCII.GetString(man.AuthenticationKey);
                }
                else
                {
                    AuthenticationKeyTB.Text = GXCommon.ToHex(man.AuthenticationKey, true);
                }

                InvocationCounterTB.Text = "0";
                ChallengeTB.Text = "";

                SystemTitleAsciiCb.CheckedChanged += SystemTitleAsciiCb_CheckedChanged;
                BlockCipherKeyAsciiCb.CheckedChanged += BlockCipherKeyAsciiCb_CheckedChanged;
                AuthenticationKeyAsciiCb.CheckedChanged += AuthenticationKeyAsciiCb_CheckedChanged;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void UpdateStartProtocol()
        {
            //If IEC47 is used DLMS is only protocol.
            GXManufacturer man = this.ManufacturerCB.SelectedItem as GXManufacturer;
            UseLNCB.Checked = man.UseLogicalNameReferencing;
            if (SelectedMedia is GXNet && man != null)
            {
                StartProtocolCB.Enabled = !man.UseIEC47;
            }
            else
            {
                StartProtocolCB.Enabled = true;
            }
            if (!StartProtocolCB.Enabled)
            {
                StartProtocolCB.SelectedItem = StartProtocolType.DLMS;
            }
        }

        public static bool IsAscii(byte[] value)
        {
            if (value == null)
            {
                return false;
            }
            foreach (byte it in value)
            {
                if (it < 0x21 || it > 0x7E)
                {
                    return false;
                }
            }
            return true;
        }

        private void SystemTitleAsciiCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (SystemTitleAsciiCb.Checked)
                {
                    if (!IsAscii(GXCommon.HexToBytes(SystemTitleTB.Text)))
                    {
                        SystemTitleAsciiCb.CheckedChanged -= SystemTitleAsciiCb_CheckedChanged;
                        SystemTitleAsciiCb.Checked = !SystemTitleAsciiCb.Checked;
                        SystemTitleAsciiCb.CheckedChanged += SystemTitleAsciiCb_CheckedChanged;
                        throw new ArgumentOutOfRangeException(Properties.Resources.InvalidASCII);
                    }
                    SystemTitleTB.Text = ASCIIEncoding.ASCII.GetString(GXCommon.HexToBytes(SystemTitleTB.Text));
                }
                else
                {
                    SystemTitleTB.Text = GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(SystemTitleTB.Text), true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BlockCipherKeyAsciiCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (BlockCipherKeyAsciiCb.Checked)
                {
                    if (!IsAscii(GXCommon.HexToBytes(BlockCipherKeyTB.Text)))
                    {
                        BlockCipherKeyAsciiCb.CheckedChanged -= BlockCipherKeyAsciiCb_CheckedChanged;
                        BlockCipherKeyAsciiCb.Checked = !BlockCipherKeyAsciiCb.Checked;
                        BlockCipherKeyAsciiCb.CheckedChanged += BlockCipherKeyAsciiCb_CheckedChanged;
                        throw new ArgumentOutOfRangeException(Properties.Resources.InvalidASCII);
                    }
                    BlockCipherKeyTB.Text = ASCIIEncoding.ASCII.GetString(GXCommon.HexToBytes(BlockCipherKeyTB.Text));
                }
                else
                {
                    BlockCipherKeyTB.Text = GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(BlockCipherKeyTB.Text), true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void AuthenticationKeyAsciiCb_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (AuthenticationKeyAsciiCb.Checked)
                {
                    if (!IsAscii(GXCommon.HexToBytes(AuthenticationKeyTB.Text)))
                    {
                        AuthenticationKeyAsciiCb.CheckedChanged -= AuthenticationKeyAsciiCb_CheckedChanged;
                        AuthenticationKeyAsciiCb.Checked = !AuthenticationKeyAsciiCb.Checked;
                        AuthenticationKeyAsciiCb.CheckedChanged += AuthenticationKeyAsciiCb_CheckedChanged;
                        throw new ArgumentOutOfRangeException(Properties.Resources.InvalidASCII);
                    }
                    AuthenticationKeyTB.Text = ASCIIEncoding.ASCII.GetString(GXCommon.HexToBytes(AuthenticationKeyTB.Text));
                }
                else
                {
                    AuthenticationKeyTB.Text = GXCommon.ToHex(ASCIIEncoding.ASCII.GetBytes(AuthenticationKeyTB.Text), true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ManufacturerCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= ManufacturerCB.Items.Count)
            {
                return;
            }

            // Draw the background of the item.
            e.DrawBackground();

            // Should we draw the focus rectangle?
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }

            Font f = new Font(e.Font, FontStyle.Regular);
            // Create a new background brush.
            Brush b = new SolidBrush(e.ForeColor);
            // Draw the item.
            GXManufacturer target = (GXManufacturer)ManufacturerCB.Items[e.Index];
            if (target == null)
            {
                return;
            }
            string name = target.Name;
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }

        /// <summary>
        /// Draw media name to media compobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediasCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= MediasCB.Items.Count)
            {
                return;
            }

            // Draw the background of the item.
            e.DrawBackground();

            // Should we draw the focus rectangle?
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }

            Font f = new Font(e.Font, FontStyle.Regular);
            // Create a new background brush.
            Brush b = new SolidBrush(e.ForeColor);
            // Draw the item.
            Gurux.Common.IGXMedia target = (Gurux.Common.IGXMedia)MediasCB.Items[e.Index];
            if (target == null)
            {
                return;
            }
            string name = target.MediaType;
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }

        private void MediasCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedMedia = (Gurux.Common.IGXMedia)MediasCB.SelectedItem;
                if (SelectedMedia is GXSerial || SelectedMedia is GXNet)
                {
                    MediaPropertiesForm = null;
                    CustomSettings.Visible = false;
                    SerialSettingsGB.Visible = SelectedMedia is GXSerial;
                    NetworkSettingsGB.Visible = SelectedMedia is GXNet;
                    TerminalSettingsGB.Visible = false;
                    if (SelectedMedia is GXNet && this.PortTB.Text == "")
                    {
                        this.PortTB.Text = "4059";
                    }
                }
                else
                {
                    SerialSettingsGB.Visible = NetworkSettingsGB.Visible = TerminalSettingsGB.Visible = false;
                    CustomSettings.Visible = true;
                    CustomSettings.Controls.Clear();
                    MediaPropertiesForm = SelectedMedia.PropertiesForm;
                    (MediaPropertiesForm as IGXPropertyPage).Initialize();
                    while (MediaPropertiesForm.Controls.Count != 0)
                    {
                        Control ctr = MediaPropertiesForm.Controls[0];
                        if (ctr is Panel)
                        {
                            if (!ctr.Enabled)
                            {
                                MediaPropertiesForm.Controls.RemoveAt(0);
                                continue;
                            }
                        }
                        CustomSettings.Controls.Add(ctr);
                        ctr.Visible = true;
                    }
                }
                UpdateStartProtocol();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AuthenticationCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GXAuthentication authentication = (GXAuthentication)AuthenticationCB.SelectedItem;
                PasswordTB.Enabled = authentication.Type != Authentication.None && authentication.Type != Authentication.HighGMAC && authentication.Type != Authentication.HighECDSA;
                this.ClientAddTB.Value = authentication.ClientAddress;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AuthenticationCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= AuthenticationCB.Items.Count)
            {
                return;
            }

            // Draw the background of the item.
            e.DrawBackground();

            // Should we draw the focus rectangle?
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }

            Font f = new Font(e.Font, FontStyle.Regular);
            // Create a new background brush.
            Brush b = new SolidBrush(e.ForeColor);
            // Draw the item.
            GXAuthentication authentication = (GXAuthentication)AuthenticationCB.Items[e.Index];
            if (authentication == null)
            {
                return;
            }
            string name = authentication.Type.ToString();
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }

        private void ServerAddressTypeCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= ServerAddressTypeCB.Items.Count)
            {
                return;
            }

            // Draw the background of the item.
            e.DrawBackground();

            // Should we draw the focus rectangle?
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }

            Font f = new Font(e.Font, FontStyle.Regular);
            // Create a new background brush.
            Brush b = new SolidBrush(e.ForeColor);
            // Draw the item.
            GXServerAddress item = (GXServerAddress)ServerAddressTypeCB.Items[e.Index];
            if (item == null)
            {
                return;
            }
            string name = item.HDLCAddress.ToString();
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }

        /// <summary>
        /// Show Serial port settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdvancedBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ((GXSerial)SelectedMedia).PortName = this.SerialPortCB.Text;
                if (SelectedMedia.Properties(this))
                {
                    this.SerialPortCB.Text = ((GXSerial)SelectedMedia).PortName;
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(this, Ex.Message, Properties.Resources.CTT, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowConformance(Enums.Conformance c)
        {
            if (UseLNCB.Checked)
            {
                GeneralProtectionCB.Checked = (c & Enums.Conformance.GeneralProtection) != 0;
                GeneralBlockTransferCB.Checked = (c & Enums.Conformance.GeneralBlockTransfer) != 0;
                Attribute0SetReferencingCB.Checked = (c & Enums.Conformance.Attribute0SupportedWithSet) != 0;
                PriorityManagementCB.Checked = (c & Enums.Conformance.PriorityMgmtSupported) != 0;
                Attribute0GetReferencingCB.Checked = (c & Enums.Conformance.Attribute0SupportedWithGet) != 0;
                GetBlockTransferCB.Checked = (c & Enums.Conformance.BlockTransferWithGetOrRead) != 0;
                SetBlockTransferCB.Checked = (c & Enums.Conformance.BlockTransferWithSetOrWrite) != 0;
                ActionBlockTransferCB.Checked = (c & Enums.Conformance.BlockTransferWithAction) != 0;
                MultipleReferencesCB.Checked = (c & Enums.Conformance.MultipleReferences) != 0;
                DataNotificationCB.Checked = (c & Enums.Conformance.DataNotification) != 0;
                AccessCB.Checked = (c & Enums.Conformance.Access) != 0;
                GetCB.Checked = (c & Enums.Conformance.Get) != 0;
                SetCB.Checked = (c & Enums.Conformance.Set) != 0;
                SelectiveAccessCB.Checked = (c & Enums.Conformance.SelectiveAccess) != 0;
                EventNotificationCB.Checked = (c & Enums.Conformance.EventNotification) != 0;
                ActionCB.Checked = (c & Enums.Conformance.EventNotification) != 0;
            }
            else
            {
                SNGeneralProtectionCB.Checked = (c & Enums.Conformance.GeneralProtection) != 0;
                SNGeneralBlockTransferCB.Checked = (c & Enums.Conformance.GeneralBlockTransfer) != 0;
                ReadCB.Checked = (c & Enums.Conformance.Read) != 0;
                WriteCB.Checked = (c & Enums.Conformance.Write) != 0;
                UnconfirmedWriteCB.Checked = (c & Enums.Conformance.UnconfirmedWrite) != 0;
                ReadBlockTransferCB.Checked = (c & Enums.Conformance.BlockTransferWithGetOrRead) != 0;
                WriteBlockTransferCB.Checked = (c & Enums.Conformance.BlockTransferWithSetOrWrite) != 0;
                SNMultipleReferencesCB.Checked = (c & Enums.Conformance.MultipleReferences) != 0;
                InformationReportCB.Checked = (c & Enums.Conformance.InformationReport) != 0;
                SNDataNotificationCB.Checked = (c & Enums.Conformance.DataNotification) != 0;
                ParameterizedAccessCB.Checked = (c & Enums.Conformance.ParameterizedAccess) != 0;
            }
            LNSettings.Visible = UseLNCB.Checked;
            SNSettings.Visible = !UseLNCB.Checked;
        }
        private void UseLNCB_CheckedChanged(object sender, EventArgs e)
        {
            Gurux.DLMS.Enums.Conformance c = GXDLMSClient.GetInitialConformance(UseLNCB.Checked);
            ShowConformance(c);
        }
    }
}
