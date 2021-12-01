using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatClientSocketNaIn.Classes;


namespace CombatClientSocketNaIn
{
    public partial class frmClienSocketNain : Form
    {
        Random m_r;
        Elfe m_elfe;
        Nain m_nain;
        public frmClienSocketNain()
        {
            InitializeComponent();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(0, 3));
            picNain.Image = m_nain.Avatar;
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString(); ;
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;

            m_elfe = new Elfe(1, 0, 0);
            picElfe.Image = m_elfe.Avatar;
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnFrappe.Enabled = true;
            Reset();
        }

        private void btnFrappe_Click(object sender, EventArgs e)
        {
            Socket client;
            int nbOctetReception;
            byte[] tByteEnvoie;
            byte[] tByteReceptionClient = new byte[64];
            string reponse;

            try
            {
                // création d’un objet socket et connexion
                client = new Socket(SocketType.Stream, ProtocolType.Tcp);

                client.Connect(IPAddress.Parse("127.0.0.1"), 9999);

                if (!client.Connected)
                    MessageBox.Show("assurez-vous que le serveur est démarré et en attente d'un client");

                // vérifier si l’objet socket est connecté
                if (client.Connected)
                {
                    // envoie les données du nain sous cette forme: 
                    // vieNain;forceNain;armeNain;

                    string str = m_nain.Vie + ";" + m_nain.Force + ";" + m_nain.Arme;

                    tByteEnvoie = Encoding.ASCII.GetBytes(str);

                    // transmission
                    client.Send(tByteEnvoie);

                    Thread.Sleep(500);

                    // réception 
                    // reçoit les données sous cette forme: 
                    // vieNain;forceNain;armeNain;vieElfe;forceElfe;sortElfe;

                    nbOctetReception = client.Receive(tByteReceptionClient);

                    reponse = Encoding.ASCII.GetString(tByteReceptionClient);

                    // split sur le string de réception pour afficher les 
                    // nouvelles stat du nain et de l’elfe

                    string[] subString = reponse.Split(';');

                    if (subString.Length != 6)
                    {
                        MessageBox.Show("Manque de donnees");
                        return;
                    }
                    
                    m_nain.Vie = Convert.ToInt32(subString[0]);
                    m_nain.Force = Convert.ToInt32(subString[1]);
                    m_nain.Arme = subString[2];
                    m_elfe.Vie = Convert.ToInt32(subString[3]);
                    m_elfe.Force = Convert.ToInt32(subString[4]);
                    m_elfe.Sort = Convert.ToInt32(subString[5]);

                    // tester et afficher le gagnant

                    lblVieNain.Text = "Vie: " + m_nain.Vie;
                    lblForceNain.Text = "Force: " + m_nain.Force;
                    lblArmeNain.Text = "Arme: " + m_nain.Arme;

                    lblVieElfe.Text = "Vie: " + m_elfe.Vie;
                    lblForceElfe.Text = "Force: " + m_elfe.Force;
                    lblSortElfe.Text = "Sort: " + m_elfe.Sort;

                    if (m_elfe.Vie <= 0)
                    {
                        MessageBox.Show("Nain gagnant!");
                        return;
                    }
                    if (m_nain.Vie <= 0)
                    {
                        MessageBox.Show("Elfe gagnant!");
                        return;
                    }
                }
                // fermeture de l’objet socket
                client.Close();

                MessageBox.Show("Deconnection, prochain tour");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

