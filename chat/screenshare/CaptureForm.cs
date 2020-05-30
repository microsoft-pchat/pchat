using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class CaptureForm : Form
    {
        [DllImport("Shcore.dll")]
        static extern int SetProcessDpiAwareness(int val);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        Thread sendFramesThread;
        object rectangleUpdateLock = new object();
        Rectangle captureRectangle = Rectangle.Empty;
        ManualResetEvent stopSignal = new ManualResetEvent(false);

        public string Host { get; set; }

        public int Port { get; set; }

        public Guid Id { get; set; }

        public CaptureForm()
        {
            SetProcessDpiAwareness(1);
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (null == sendFramesThread)
            {
                stopSignal.Reset();

                sendFramesThread = new Thread(SendFramesThreadProc) { IsBackground = true };
                sendFramesThread.Start();

                button1.Text = "Stop Sharing";
            }
            else
            {
                stopSignal.Set();
                sendFramesThread = null;

                button1.Text = "Start Sharing";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string url = string.Format("https://{0}/feed?id={1}", Host, Id);

            // the joys of inline c# :(
            Thread thread = new Thread(() => Clipboard.SetText(url));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void SendFramesThreadProc()
        {
            string id = Id.ToString().Trim(new char[] { '{', '}' });
            while (!stopSignal.WaitOne(0)) // re-connect loop
            {
                try
                {
                    UpdateLabel("Trying to connect");
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(Host, Port);
                        SslStream stream = new SslStream(client.GetStream());
                        stream.AuthenticateAsClient(Host);

                        // create client here
                        UpdateLabel("Connected");

                        var lastImageSent = new byte[] { };
                        DateTime lastFrameSendTime = DateTime.Now;

                        // frame sending loop
                        while (!stopSignal.WaitOne(0))
                        {
                            var image = GetImage();

                            if (ByteArrayEquals(lastImageSent, image) && (DateTime.Now - lastFrameSendTime).TotalSeconds < 30)
                            {
                                Trace.TraceInformation("Skipping image.  No change recently");
                                continue;
                            }

                            // protocol: [header][jpeg image]

                            // header is intentionally not-optimized for hackathon.  using ascii encoding for data 
                            // [guid, without surrounding brackets]:[length, ascii encoded in 10 bytes]:
                            // e.g. 00000000-0000-0000-0000-000000000000:      2291:
                            //

                            var headerBytes = UTF8Encoding.Default.GetBytes(string.Format("{0}:{1}:", id, image.Length.ToString().PadLeft(10)));

                            stream.Write(headerBytes, 0, headerBytes.Length);
                            stream.Write(image, 0, image.Length);

                            lastImageSent = image;
                            lastFrameSendTime = DateTime.Now;
                        }

                        stream.Close();
                    }
                } 
                catch (Exception ex)
                {
                    UpdateLabel("Error... re-connecting");
                    stopSignal.WaitOne(5000);

                    Trace.TraceError("Unhandled exception in frame transfer loop: {0}", ex);
                }
            }

            UpdateLabel(string.Empty);

            Trace.TraceInformation("Clean thread exit");
        }

        private void UpdateLabel(string str)
        {
            if (InvokeRequired)
            {
                this.label1.BeginInvoke((Action)(() => { label1.Text = str; }));
            }
        }

        private byte[] GetImage()
        {
            Rectangle area;
            lock (rectangleUpdateLock)
            {
                area = new Rectangle(captureRectangle.X, captureRectangle.Y, captureRectangle.Width, captureRectangle.Height);
            }

            Bitmap captureBitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
            using (Graphics captureGraphics = Graphics.FromImage(captureBitmap))
            {
                captureGraphics.CopyFromScreen(area.Left, area.Top, 0, 0, new Size((int)(area.Size.Width), (int)(area.Size.Height)));
                using (MemoryStream ms = new MemoryStream())
                {
                    captureBitmap.Save(ms, ImageFormat.Jpeg);
                    ms.Flush();

                    return ms.ToArray();
                }
            }
        }

        static bool ByteArrayEquals(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        private void pictureBox1_MoveOrResize(object sender, EventArgs e)
        {
            lock (rectangleUpdateLock)
            {
                this.captureRectangle = new Rectangle(this.PointToScreen(this.pictureBox1.Location), this.pictureBox1.Size);
            }
        }

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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(490, 394);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start Sharing";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(342, 394);
            this.button2.Name = "button1";
            this.button2.Size = new System.Drawing.Size(138, 37);
            this.button2.TabIndex = 0;
            this.button2.Text = "Copy URL";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.Color.Maroon;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(616, 350);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 402);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 20);
            this.label1.TabIndex = 2;
            // 
            // CaptureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 443);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CaptureForm";
            this.ShowIcon = false;
            this.Text = "Screencast";
            this.TopMost = true;
            this.TopLevel = true;
            this.TransparencyKey = System.Drawing.Color.Maroon;
            this.ResizeEnd += new System.EventHandler(this.pictureBox1_MoveOrResize);
            this.Move += new System.EventHandler(this.pictureBox1_MoveOrResize);
            this.Resize += new System.EventHandler(this.pictureBox1_MoveOrResize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
    }
}