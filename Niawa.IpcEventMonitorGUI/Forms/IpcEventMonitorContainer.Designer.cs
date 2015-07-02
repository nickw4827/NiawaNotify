namespace Niawa.IpcEventMonitorGUI
{
    partial class IpcEventMonitorContainer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.newIPCWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adHocNetworkAdapterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.dataGridViewWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowserWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adHocNetworkAdapterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.requestStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.favoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorUDPEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorTCPEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorSessionMgrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monitorAdHocNetworkAdapterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arrangeWindowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newIPCWindowToolStripMenuItem,
            this.commandsToolStripMenuItem,
            this.favoritesToolStripMenuItem,
            this.arrangeWindowsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1109, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // newIPCWindowToolStripMenuItem
            // 
            this.newIPCWindowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adHocNetworkAdapterToolStripMenuItem1,
            this.toolStripSeparator1,
            this.dataGridViewWindowToolStripMenuItem,
            this.webBrowserWindowToolStripMenuItem});
            this.newIPCWindowToolStripMenuItem.Name = "newIPCWindowToolStripMenuItem";
            this.newIPCWindowToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.newIPCWindowToolStripMenuItem.Text = "IPC Window";
            this.newIPCWindowToolStripMenuItem.Click += new System.EventHandler(this.newIPCWindowToolStripMenuItem_Click);
            // 
            // adHocNetworkAdapterToolStripMenuItem1
            // 
            this.adHocNetworkAdapterToolStripMenuItem1.Name = "adHocNetworkAdapterToolStripMenuItem1";
            this.adHocNetworkAdapterToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.adHocNetworkAdapterToolStripMenuItem1.Size = new System.Drawing.Size(228, 22);
            this.adHocNetworkAdapterToolStripMenuItem1.Text = "Ad-Hoc Network Adapter";
            this.adHocNetworkAdapterToolStripMenuItem1.Click += new System.EventHandler(this.adHocNetworkAdapterToolStripMenuItem1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(225, 6);
            // 
            // dataGridViewWindowToolStripMenuItem
            // 
            this.dataGridViewWindowToolStripMenuItem.Name = "dataGridViewWindowToolStripMenuItem";
            this.dataGridViewWindowToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.dataGridViewWindowToolStripMenuItem.Text = "New DataGridView Window...";
            this.dataGridViewWindowToolStripMenuItem.Click += new System.EventHandler(this.dataGridViewWindowToolStripMenuItem_Click);
            // 
            // webBrowserWindowToolStripMenuItem
            // 
            this.webBrowserWindowToolStripMenuItem.Name = "webBrowserWindowToolStripMenuItem";
            this.webBrowserWindowToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.webBrowserWindowToolStripMenuItem.Text = "New WebBrowser Window...";
            this.webBrowserWindowToolStripMenuItem.Click += new System.EventHandler(this.webBrowserWindowToolStripMenuItem_Click);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adHocNetworkAdapterToolStripMenuItem});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // adHocNetworkAdapterToolStripMenuItem
            // 
            this.adHocNetworkAdapterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.requestStatusToolStripMenuItem});
            this.adHocNetworkAdapterToolStripMenuItem.Name = "adHocNetworkAdapterToolStripMenuItem";
            this.adHocNetworkAdapterToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.adHocNetworkAdapterToolStripMenuItem.Text = "Ad-Hoc Network Adapter";
            // 
            // requestStatusToolStripMenuItem
            // 
            this.requestStatusToolStripMenuItem.Name = "requestStatusToolStripMenuItem";
            this.requestStatusToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.requestStatusToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.requestStatusToolStripMenuItem.Text = "Request Status";
            this.requestStatusToolStripMenuItem.Click += new System.EventHandler(this.requestStatusToolStripMenuItem_Click);
            // 
            // favoritesToolStripMenuItem
            // 
            this.favoritesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.monitorUDPEventsToolStripMenuItem,
            this.monitorTCPEventsToolStripMenuItem,
            this.monitorSessionMgrToolStripMenuItem,
            this.monitorAdHocNetworkAdapterToolStripMenuItem});
            this.favoritesToolStripMenuItem.Name = "favoritesToolStripMenuItem";
            this.favoritesToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.favoritesToolStripMenuItem.Text = "Favorites";
            // 
            // monitorUDPEventsToolStripMenuItem
            // 
            this.monitorUDPEventsToolStripMenuItem.Name = "monitorUDPEventsToolStripMenuItem";
            this.monitorUDPEventsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.monitorUDPEventsToolStripMenuItem.Text = "Monitor UDP Events";
            this.monitorUDPEventsToolStripMenuItem.Click += new System.EventHandler(this.monitorUDPEventsToolStripMenuItem_Click);
            // 
            // monitorTCPEventsToolStripMenuItem
            // 
            this.monitorTCPEventsToolStripMenuItem.Name = "monitorTCPEventsToolStripMenuItem";
            this.monitorTCPEventsToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.monitorTCPEventsToolStripMenuItem.Text = "Monitor TCP Events";
            this.monitorTCPEventsToolStripMenuItem.Click += new System.EventHandler(this.monitorTCPEventsToolStripMenuItem_Click);
            // 
            // monitorSessionMgrToolStripMenuItem
            // 
            this.monitorSessionMgrToolStripMenuItem.Name = "monitorSessionMgrToolStripMenuItem";
            this.monitorSessionMgrToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.monitorSessionMgrToolStripMenuItem.Text = "Monitor Session Manager";
            this.monitorSessionMgrToolStripMenuItem.Click += new System.EventHandler(this.monitorSessionMgrToolStripMenuItem_Click);
            // 
            // monitorAdHocNetworkAdapterToolStripMenuItem
            // 
            this.monitorAdHocNetworkAdapterToolStripMenuItem.Name = "monitorAdHocNetworkAdapterToolStripMenuItem";
            this.monitorAdHocNetworkAdapterToolStripMenuItem.Size = new System.Drawing.Size(255, 22);
            this.monitorAdHocNetworkAdapterToolStripMenuItem.Text = "Monitor Ad-Hoc Network Adapter";
            this.monitorAdHocNetworkAdapterToolStripMenuItem.Click += new System.EventHandler(this.monitorAdHocNetworkAdapterToolStripMenuItem_Click);
            // 
            // arrangeWindowsToolStripMenuItem
            // 
            this.arrangeWindowsToolStripMenuItem.Name = "arrangeWindowsToolStripMenuItem";
            this.arrangeWindowsToolStripMenuItem.Size = new System.Drawing.Size(113, 20);
            this.arrangeWindowsToolStripMenuItem.Text = "Arrange Windows";
            this.arrangeWindowsToolStripMenuItem.Click += new System.EventHandler(this.arrangeWindowsToolStripMenuItem_Click);
            // 
            // IpcEventMonitorContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 753);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "IpcEventMonitorContainer";
            this.Text = "Niawa IPC Event Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IpcEventMonitorContainer_FormClosing);
            this.Load += new System.EventHandler(this.IpcEventMonitorContainer_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem newIPCWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem favoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorUDPEventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorTCPEventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorSessionMgrToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem monitorAdHocNetworkAdapterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arrangeWindowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataGridViewWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webBrowserWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adHocNetworkAdapterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem requestStatusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adHocNetworkAdapterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

