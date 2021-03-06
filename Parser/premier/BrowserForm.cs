﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace premier
{
    public partial class BrowserForm : Form
    {
        public static readonly string URL = @"https://www.tipico.com/en/live-betting/";

        public BrowserForm()
        {
            InitializeComponent();

            webBrowser.ScriptErrorsSuppressed = true;

            webBrowser.Url = new Uri(URL);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            Hide();
            e.Cancel = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = webBrowser.DocumentTitle;
            webBrowser.DocumentTitleChanged += new EventHandler(webBrowser_DocumentTitleChanged);
        }

        void webBrowser_DocumentTitleChanged(object sender, EventArgs e)
        {
            Text = webBrowser.DocumentTitle;
        }
    }
}
