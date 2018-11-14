using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VLC
{
    public partial class VLCControl: UserControl
    {
        public VLCControl()
        {
            InitializeComponent();
            textBox1.Text = "rtsp://91.230.153.2:126/rtsp?channelId=0c09cc2a-b077-4171-bcec-772bc81133e0&login=root&password=&streamtype=alternative";
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Start play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (axVLCPlugin21.playlist.isPlaying)
                axVLCPlugin21.playlist.stop();
            axVLCPlugin21.playlist.items.clear();
            var url = textBox1.Text;
            if (checkBox1.Checked)
                url += $"&mode=archive&starttime={dateTimePicker1.Text}&speed=1";//"&mode=archive&starttime=08.11.2018+03:05:01&speed=1";
            axVLCPlugin21.playlist.add(url, null, null);
            axVLCPlugin21.playlist.play();
        }
        /// <summary>
        /// Stop play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (axVLCPlugin21.playlist.isPlaying)
                axVLCPlugin21.playlist.stop();
            axVLCPlugin21.playlist.items.clear();
        }
    }
}
