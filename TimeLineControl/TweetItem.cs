﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TogiApi;
using System.Threading;
using System.Net;
using System.IO;
using System.ComponentModel.Design;

namespace TimeLineControl
{
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))] 
    public partial class TweetItem : UserControl
    {
        delegate void SetPicture(Bitmap Resim);
        delegate void SetFavoriteIcon(bool isFavorite);


        private Tweet ItemTweet;
        private Bitmap Resim;
        private WebClient ResimIstegi;

        public TweetItem()
        {
            InitializeComponent();                      
        }

        public TweetItem(Tweet t)
        {
            InitializeComponent();
            ItemTweet = t;

            //ShowFavoriteIcon(t.isFavorite);
        }

        void ResimIstegi_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                SetProgfileImage(new Bitmap(e.Result));
            }
            catch
            {

            }
        }

        private void SetControlValues()
        {
            TweetText.Links.Clear();
            TweetText.Text = System.Web.HttpUtility.HtmlDecode(ItemTweet.Text.Trim());

            FullName.Text = String.Format("{0} ({1})",
                ItemTweet.UserName,
                ItemTweet.UserScreenName);

            lTime.Text = TweetUtils.ToRelativeDate(ItemTweet.CreateAt);
            SetBackColorDefault(ItemTweet.TweetType);
            Resim = Properties.Resources.default_profile_normal;
            TweetUtils.LinkEkle(TweetText);

            // Resim Aliniyor.
            //if (Resim == null)
            //{
                Thread img = new Thread(new ParameterizedThreadStart(ShowProfileImage));
                img.Start(ItemTweet.ProfilImageUrl);
            //}
        }

        private void ShowProfileImage(object ImageUrl)
        {
            // Profil Resmi Çağırılıyor.
            //SetProgfileImage(Utils.GetImage((string)ImageUrl));

            ResimIstegi = new WebClient();
            ResimIstegi.OpenReadAsync(new Uri((string)ImageUrl));
            ResimIstegi.OpenReadCompleted += new OpenReadCompletedEventHandler(ResimIstegi_OpenReadCompleted);
        }

        private void SetProgfileImage(Bitmap Resim)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetPicture(SetProgfileImage),new object[]{Resim});
                return;
            }

            ProfileImage.Image = Resim;
        }

        private void TweetText_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
            }
            catch
            {                
                // Lay Lay Lom...
            }
        }

        private void TweetItem_MouseHover(object sender, EventArgs e)
        {
            //this.BackColor = Color.FromArgb(115, 229, 229);
            SetBackColorIsRead(ItemTweet.TweetType);
        }

        private void TweetItem_MouseLeave(object sender, EventArgs e)
        {
            //this.BackColor = Color.PaleTurquoise;
            SetBackColorDefault(ItemTweet.TweetType);
        }

        private void SetBackColorIsRead(Tweet.TweetTypes tp)
        {
            this.TweetText.ForeColor = Color.Black;
            this.TweetText.LinkColor = Color.RoyalBlue;
            this.FullName.ForeColor = Color.Black;
            this.lTime.ForeColor = Color.Black;

            switch (tp)
            {
                case TogiApi.Tweet.TweetTypes.Normal:
                    this.BackColor = Color.FromArgb(115, 229, 229);
                    break;

                case TogiApi.Tweet.TweetTypes.Message:
                    this.BackColor = Color.Orange;
                    break;

                case TogiApi.Tweet.TweetTypes.Reply:
                    this.BackColor = Color.FromArgb(171, 229, 113);                     
                    break;

                default:
                    break;
            }
        }

        private void SetBackColorDefault(Tweet.TweetTypes tp)
        {
            this.TweetText.ForeColor = Color.Gray;
            this.TweetText.LinkColor = Color.RoyalBlue;
            this.FullName.ForeColor = Color.Gray;
            this.lTime.ForeColor = Color.Gray;

            switch (tp)
            {
                case TogiApi.Tweet.TweetTypes.Normal:
                    this.BackColor = Color.PaleTurquoise;
                    break;

                case TogiApi.Tweet.TweetTypes.Message:
                    this.BackColor = Color.Wheat;
                    break;

                case TogiApi.Tweet.TweetTypes.Reply:              
                    this.BackColor = Color.FromArgb(195, 238, 190);
                    break;

                default:
                    break;
            }
        }

        private void TweetItem_Load(object sender, EventArgs e)
        {
            SetControlValues();
        }

        private void replyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tsFavorite_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(CreateFavorite));
            th.SetApartmentState(ApartmentState.STA);
            th.IsBackground = true;
            th.Start();
        }

        private void CreateFavorite()
        {
            using (Twitter t = new Twitter())
            {
                t.Favorite(ItemTweet.Id);
                ShowFavoriteIcon(true);
            }
        }

        private void ShowFavoriteIcon(bool isFavorite)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SetFavoriteIcon(ShowFavoriteIcon), new object[] { isFavorite});
                return;
            }

            pFavoriIcon.Visible = isFavorite;
        }
    }
}
