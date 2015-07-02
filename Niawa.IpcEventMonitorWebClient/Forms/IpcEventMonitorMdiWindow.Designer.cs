namespace Niawa.IpcEventMonitorWebClient.Forms
{
    partial class IpcEventMonitorMdiWindow
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
            this.iPCEventToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adHocNetworkAdapterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customEventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adHocNetworkAdapterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.requestStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iPCEventToolStripMenuItem,
            this.commandsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(931, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // iPCEventToolStripMenuItem
            // 
            this.iPCEventToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adHocNetworkAdapterToolStripMenuItem,
            this.customEventsToolStripMenuItem});
            this.iPCEventToolStripMenuItem.Name = "iPCEventToolStripMenuItem";
            this.iPCEventToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.iPCEventToolStripMenuItem.Text = "IPC Event";
            // 
            // adHocNetworkAdapterToolStripMenuItem
            // 
            this.adHocNetworkAdapterToolStripMenuItem.Name = "adHocNetworkAdapterToolStripMenuItem";
            this.adHocNetworkAdapterToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.adHocNetworkAdapterToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.adHocNetworkAdapterToolStripMenuItem.Text = "Ad-Hoc Network Adapter";
            this.adHocNetworkAdapterToolStripMenuItem.Click += new System.EventHandler(this.adHocNetworkAdapterToolStripMenuItem_Click);
            // 
            // customEventsToolStripMenuItem
            // 
            this.customEventsToolStripMenuItem.Name = "customEventsToolStripMenuItem";
            this.customEventsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.customEventsToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.customEventsToolStripMenuItem.Text = "Custom Event...";
            this.customEventsToolStripMenuItem.Click += new System.EventHandler(this.customEventsToolStripMenuItem_Click);
            // 
            // commandsToolStripMenuItem
            // 
            this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adHocNetworkAdapterToolStripMenuItem1});
            this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
            this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
            this.commandsToolStripMenuItem.Text = "Commands";
            // 
            // adHocNetworkAdapterToolStripMenuItem1
            // 
            this.adHocNetworkAdapterToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.requestStatusToolStripMenuItem});
            this.adHocNetworkAdapterToolStripMenuItem1.Name = "adHocNetworkAdapterToolStripMenuItem1";
            this.adHocNetworkAdapterToolStripMenuItem1.Size = new System.Drawing.Size(209, 22);
            this.adHocNetworkAdapterToolStripMenuItem1.Text = "Ad-Hoc Network Adapter";
            this.adHocNetworkAdapterToolStripMenuItem1.Click += new System.EventHandler(this.adHocNetworkAdapterToolStripMenuItem1_Click);
            // 
            // requestStatusToolStripMenuItem
            // 
            this.requestStatusToolStripMenuItem.Name = "requestStatusToolStripMenuItem";
            this.requestStatusToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.requestStatusToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.requestStatusToolStripMenuItem.Text = "Request Status";
            // 
            // IpcEventMonitorMdiWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 577);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "IpcEventMonitorMdiWindow";
            this.Text = "IpcEventMonitorMdiWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IpcEventMonitorMdiWindow_FormClosing);
            this.Load += new System.EventHandler(this.IpcEventMonitorMdiWindow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem iPCEventToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adHocNetworkAdapterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customEventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adHocNetworkAdapterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem requestStatusToolStripMenuItem;
    }
}