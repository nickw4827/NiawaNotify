namespace Niawa.IpcEventMonitorGUI
{
    partial class IpcWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IpcWindow));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbMonitor = new System.Windows.Forms.ToolStripButton();
            this.tsbResetView = new System.Windows.Forms.ToolStripButton();
            this.dgvMonitor = new System.Windows.Forms.DataGridView();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 456);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(678, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(39, 17);
            this.tsslStatus.Text = "Ready";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbMonitor,
            this.tsbResetView});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(678, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbMonitor
            // 
            this.tsbMonitor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbMonitor.Image = ((System.Drawing.Image)(resources.GetObject("tsbMonitor.Image")));
            this.tsbMonitor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMonitor.Name = "tsbMonitor";
            this.tsbMonitor.Size = new System.Drawing.Size(98, 22);
            this.tsbMonitor.Text = "Start Monitoring";
            this.tsbMonitor.Click += new System.EventHandler(this.tsbMonitor_Click);
            // 
            // tsbResetView
            // 
            this.tsbResetView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbResetView.Image = ((System.Drawing.Image)(resources.GetObject("tsbResetView.Image")));
            this.tsbResetView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbResetView.Name = "tsbResetView";
            this.tsbResetView.Size = new System.Drawing.Size(38, 22);
            this.tsbResetView.Text = "Clear";
            this.tsbResetView.Click += new System.EventHandler(this.tsbResetView_Click);
            // 
            // dgvMonitor
            // 
            this.dgvMonitor.AllowUserToAddRows = false;
            this.dgvMonitor.AllowUserToDeleteRows = false;
            this.dgvMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMonitor.Location = new System.Drawing.Point(0, 25);
            this.dgvMonitor.Name = "dgvMonitor";
            this.dgvMonitor.ReadOnly = true;
            this.dgvMonitor.Size = new System.Drawing.Size(678, 431);
            this.dgvMonitor.TabIndex = 2;
            this.dgvMonitor.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvMonitor_CellContentClick);
            // 
            // IpcWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 478);
            this.Controls.Add(this.dgvMonitor);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "IpcWindow";
            this.Text = "IPC []";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IpcWindow_FormClosed);
            this.Load += new System.EventHandler(this.IpcWindow_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMonitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.DataGridView dgvMonitor;
        private System.Windows.Forms.ToolStripButton tsbMonitor;
        private System.Windows.Forms.ToolStripButton tsbResetView;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
    }
}