namespace EasyCraft.CrashHandler
{
    partial class CrashReport
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
            this.label1 = new System.Windows.Forms.Label();
            this.emailInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.descInput = new System.Windows.Forms.RichTextBox();
            this.fileListView = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn3 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.sendBtn = new System.Windows.Forms.Button();
            this.status1 = new System.Windows.Forms.Panel();
            this.status2 = new System.Windows.Forms.Panel();
            this.status3 = new System.Windows.Forms.Panel();
            this.status4 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.fileListView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Email:";
            // 
            // emailInput
            // 
            this.emailInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.emailInput.Location = new System.Drawing.Point(107, 12);
            this.emailInput.Name = "emailInput";
            this.emailInput.Size = new System.Drawing.Size(533, 45);
            this.emailInput.TabIndex = 1;
            this.emailInput.TextChanged += new System.EventHandler(this.emailInput_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(12, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(628, 88);
            this.label2.TabIndex = 2;
            this.label2.Text = "Description (what were you doing, when did it happen, did it happen before):";
            // 
            // descInput
            // 
            this.descInput.AcceptsTab = true;
            this.descInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descInput.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descInput.Location = new System.Drawing.Point(19, 168);
            this.descInput.Name = "descInput";
            this.descInput.Size = new System.Drawing.Size(621, 328);
            this.descInput.TabIndex = 3;
            this.descInput.Text = "";
            this.descInput.TextChanged += new System.EventHandler(this.descInput_TextChanged);
            // 
            // fileListView
            // 
            this.fileListView.AllColumns.Add(this.olvColumn1);
            this.fileListView.AllColumns.Add(this.olvColumn2);
            this.fileListView.AllColumns.Add(this.olvColumn3);
            this.fileListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileListView.CellEditUseWholeCell = false;
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2,
            this.olvColumn3});
            this.fileListView.Cursor = System.Windows.Forms.Cursors.Default;
            this.fileListView.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileListView.FullRowSelect = true;
            this.fileListView.GridLines = true;
            this.fileListView.HideSelection = false;
            this.fileListView.Location = new System.Drawing.Point(19, 502);
            this.fileListView.Name = "fileListView";
            this.fileListView.OverlayImage.Alignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.fileListView.ShowGroups = false;
            this.fileListView.Size = new System.Drawing.Size(621, 282);
            this.fileListView.TabIndex = 4;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            this.fileListView.DoubleClick += new System.EventHandler(this.fileListView_DoubleClick);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "fileType";
            this.olvColumn1.AspectToStringFormat = "";
            this.olvColumn1.Text = "Type";
            this.olvColumn1.Width = 100;
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "name";
            this.olvColumn2.AspectToStringFormat = "";
            this.olvColumn2.Text = "Name";
            this.olvColumn2.Width = 200;
            // 
            // olvColumn3
            // 
            this.olvColumn3.AspectName = "path";
            this.olvColumn3.AspectToStringFormat = "";
            this.olvColumn3.FillsFreeSpace = true;
            this.olvColumn3.Text = "Path";
            this.olvColumn3.Width = 38;
            // 
            // sendBtn
            // 
            this.sendBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sendBtn.Location = new System.Drawing.Point(509, 790);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(131, 52);
            this.sendBtn.TabIndex = 5;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = true;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // status1
            // 
            this.status1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.status1.Location = new System.Drawing.Point(19, 810);
            this.status1.Name = "status1";
            this.status1.Size = new System.Drawing.Size(114, 18);
            this.status1.TabIndex = 6;
            // 
            // status2
            // 
            this.status2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.status2.Location = new System.Drawing.Point(139, 810);
            this.status2.Name = "status2";
            this.status2.Size = new System.Drawing.Size(114, 18);
            this.status2.TabIndex = 7;
            // 
            // status3
            // 
            this.status3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.status3.Location = new System.Drawing.Point(259, 810);
            this.status3.Name = "status3";
            this.status3.Size = new System.Drawing.Size(114, 18);
            this.status3.TabIndex = 8;
            // 
            // status4
            // 
            this.status4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.status4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(150)))), ((int)(((byte)(150)))));
            this.status4.Location = new System.Drawing.Point(379, 810);
            this.status4.Name = "status4";
            this.status4.Size = new System.Drawing.Size(114, 18);
            this.status4.TabIndex = 9;
            // 
            // CrashReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(652, 854);
            this.Controls.Add(this.status4);
            this.Controls.Add(this.status3);
            this.Controls.Add(this.status2);
            this.Controls.Add(this.status1);
            this.Controls.Add(this.sendBtn);
            this.Controls.Add(this.fileListView);
            this.Controls.Add(this.descInput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.emailInput);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(567, 56);
            this.Name = "CrashReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Send Crash Report";
            ((System.ComponentModel.ISupportInitialize)(this.fileListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox emailInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox descInput;
        private BrightIdeasSoftware.ObjectListView fileListView;
        private BrightIdeasSoftware.OLVColumn olvColumn1;
        private BrightIdeasSoftware.OLVColumn olvColumn2;
        private BrightIdeasSoftware.OLVColumn olvColumn3;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.Panel status1;
        private System.Windows.Forms.Panel status2;
        private System.Windows.Forms.Panel status3;
        private System.Windows.Forms.Panel status4;
    }
}

