using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        int count_threads = 0;
        int global_num_form = 1; //Число вложенных форм
        Thread pbthread; //ProgressBar Thread
        Thread child_pbthread; //Child ProgressBar Thread
        Thread mainthread = null;
        bool run_child_thread = false;
        bool close_button_click = false;
        Form1 newform;
        //private static ManualResetEvent resetEvent = new ManualResetEvent(false);

        public Form1()
        {
            InitializeComponent();
        }

        private void RunChildThreadInNewForm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() =>
                {
                   this.button1.PerformClick();  
                }));
            }

            mainthread.Join();
        }

        private void FillProgressBar()
        {
            for (int i = 0; i < 100; i++)
            {
                if (run_child_thread)
                {
                    run_child_thread = false;
                    child_pbthread = new Thread(newform.RunChildThreadInNewForm);
                    child_pbthread.Priority = ThreadPriority.Lowest;
                    //child_pbthread.IsBackground = true;
                    child_pbthread.Start();
                    child_pbthread.Join();
                }

                    Thread.Sleep(100);
                    if (this.InvokeRequired)
                    {
                        this.Invoke((Action)(() =>
                        {
                            this.progressBar1.Value = i;
                        }));
                    }

            }
            count_threads--;
            //resetEvent.Set();
        }

        private void GeneralStuff()
        {
            while (count_threads > 0)
            {
                //ThreadPool.QueueUserWorkItem(arg => FillProgressBar());
                pbthread = new Thread(FillProgressBar);
                //pbthread.Priority = ThreadPriority.Lowest;
                //pbthread.IsBackground = true;
                pbthread.Start();
                pbthread.Join();
                if (this.InvokeRequired)
                {
                    this.Invoke((Action)(() =>
                    {
                        label2.Text = count_threads.ToString();
                    }));
                 }
            }


            if (global_num_form > 1)
            {
                this.BeginInvoke((MethodInvoker)delegate { 
                this.Owner.Enabled = true;
                this.Close(); });
            }
            else
            {
                if (close_button_click)
                {
                    this.BeginInvoke((MethodInvoker)delegate { this.Close();});
                }
                //Thread.Sleep(5000);
            }
        }

        
        private void test()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => this.Close()));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            count_threads++;
            label2.Text = count_threads.ToString();

            if (mainthread == null)
            {
                mainthread = new Thread(GeneralStuff);
                mainthread.Start();
            }

            if (!mainthread.IsAlive)
            {
                mainthread = new Thread(GeneralStuff);
                mainthread.Start();
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pbthread != null)
            {
                pbthread.Abort();
                if (count_threads > 0)
                {
                    count_threads--;
                }
                label2.Text = count_threads.ToString();
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (mainthread != null && mainthread.IsAlive)
            {
                newform = new Form1();
                newform.Show(this);
                this.Enabled = false;
                newform.global_num_form = global_num_form;
                newform.global_num_form++;
                if (newform.global_num_form > 4)
                {
                    newform.button3.Enabled = false;
                }
                run_child_thread = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            close_button_click = true;
            if (mainthread != null)
            {
                if (mainthread.IsAlive)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

    }
}
        