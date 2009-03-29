﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PockeTwit
{
    public partial class userListControl : UserControl
    {

        
        private List<LinkLabel> VisibleItems = new List<LinkLabel>();
        public delegate void delItemChose(string itemText);
        public event delItemChose ItemChosen = delegate { };
        private DeviceType dt = DetectDevice.DeviceType;
        
        public userListControl()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            txtInput.LostFocus += new EventHandler(userListControl_LostFocus);
        }

        public string inputText
        {
            set
            {
                txtInput.Text = value;
                txtInput.SelectionStart = txtInput.Text.Length;
            }
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            int top = txtInput.Bottom;
            
            foreach (LinkLabel existing in VisibleItems)
            {
                existing.Click -= new EventHandler(lblItem_Click);
                existing.LostFocus -= new EventHandler(userListControl_LostFocus);
                this.Controls.Remove(existing);
            }

            VisibleItems.Clear();
            if (string.IsNullOrEmpty(txtInput.Text)) 
            { 
                this.Height = top * 2;
                return; 
            }
            string[] Names = AddressBook.GetList(txtInput.Text.ToLower());
            if (Names.Length > 0)
            {
                foreach (string Name in Names)
                {
                    LinkLabel lblItem = new LinkLabel();
                    lblItem.Text = Name;
                    lblItem.Top = top;
                    lblItem.Height = ClientSettings.TextSize + 10;
                    lblItem.Width = this.Width;
                    lblItem.Click += new EventHandler(lblItem_Click);
                    lblItem.LostFocus += new EventHandler(userListControl_LostFocus);
                    lblItem.Font = ClientSettings.TextFont;
                    lblItem.ForeColor = ClientSettings.ForeColor;

                    this.Controls.Add(lblItem);
                    top = top + ClientSettings.TextSize + 10;
                    VisibleItems.Add(lblItem);
                    
                }
                this.Height = top;
            }
            txtInput.Focus();
        }
        

        void lblItem_Click(object sender, EventArgs e)
        {
            LinkLabel lbl = (LinkLabel)sender;
            ItemChosen(lbl.Text);
            this.Visible = false;
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\b')
            {
                if (string.IsNullOrEmpty(txtInput.Text))
                {
                    this.Visible = false;
                    ItemChosen("");
                    e.Handled = true;
                }
            }
            if (e.KeyChar == '\r' || e.KeyChar == ' ')
            {
                this.Visible = false;
                ItemChosen(txtInput.Text + " ");
                e.Handled = true;
            }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            DisableCompletion.SIPHelper.DisableCompletion(txtInput);
            if (e.KeyCode == Keys.Back)
            {
                if (string.IsNullOrEmpty(txtInput.Text))
                {
                    this.Visible = false;
                    ItemChosen("");
                    e.Handled = true;
                }
            }
            if (e.KeyCode == (Keys.Down))
            {
                if (VisibleItems.Count > 0)
                {
                    VisibleItems[0].Focus();
                    e.Handled = true;
                }
            }
            if ((int)e.KeyCode == 229)
            {
                if (VisibleItems.Count > 0)
                {
                    VisibleItems[0].Focus();
                    e.Handled = true;
                }
            }
        }

        private void txtInput_GotFocus(object sender, EventArgs e)
        {
            DisableCompletion.SIPHelper.DisableCompletion(txtInput);
        }

        void userListControl_LostFocus(object sender, EventArgs e)
        {
            if (!this.Visible) { return; }
            if (txtInput.Focused) { return; }
            foreach (LinkLabel l in VisibleItems)
            {
                if (l.Focused) { return; }
            }
            this.Visible = false;
            ItemChosen(" ");
        }

        private void userListControl_GotFocus(object sender, EventArgs e)
        {
            txtInput.Focus();
            DisableCompletion.SIPHelper.DisableCompletion(txtInput);
        }
    }
}