using CombatServeurSocketElfe.Classes;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;
        Thread m_thCombat;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_r = new Random();

            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener)
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9999);
            m_ServerListener.Start();
            Reset();
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();

            lstReception.Items.Clear();
        }

        void AfficheStatNain()
        {



            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {



            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        private void btnReset_Click(object sender, EventArgs e)
        {

            Reset();
        }

        private void btnAttente_Click(object sender, EventArgs e)
        {
            m_thCombat = new Thread(new ThreadStart(Combat));

            m_thCombat.Start();
        }
        public void Combat()
        {
            string reponseServeur = "aucune";
            string receptionClient = "rien";
            int nbOctetReception;
            int noArme = 0, vie = 0, force = 0;
            string arme = "";
            byte[] tByteReception = new byte[50];
            ASCIIEncoding textByte = new ASCIIEncoding();
            byte[] tByteEnvoie;

            try
            {
                // en boucle jusqu'à ce que l'Elfe soit mort ou le nain soit mort
                while (true)
                {
                    lstReception.Items.Add("Attente du client!");
                    lstReception.Update();

                    // attend une connexion cliente socket
                    m_client = m_ServerListener.AcceptSocket(); //(bloquant)

                    lstReception.Items.Add("Client branché !");
                    lstReception.Update();
                    Thread.Sleep(500);

                    // reçoit les données cliente (nain)
                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);

                    lstReception.Items.Add("du client: " + receptionClient);
                    lstReception.Update();

                    // split sur le ‘;’ pour récupérer les données d’un nain
                    string[] subString = receptionClient.Split(';');
                    if (subString.Length != 3)
                    {
                        lstReception.Items.Add("Erreur de transmission");
                        lstReception.Update();
                    }

                    vie = Convert.ToInt32(subString[0]);
                    force = Convert.ToInt32(subString[1]);
                    arme = subString[2];

                    m_nain = new Nain(vie, force, noArme);
                    m_nain.Arme = arme;

                    AfficheStatNain();

                    // exécute Frapper
                    m_nain.Frapper(m_elfe);

                    // affiche les données de l'elfe membre  
                    AfficheStatElfe();

                    // exécute LancerSort
                    m_elfe.LancerSort(m_nain);

                    // affiche les données du nain membre et de l'elfe membre
                    AfficheStatNain();
                    AfficheStatElfe();
                    // envoie les données au client sous cette forme: 
                    //vieNain;forceNain;armeNain|vieElfe;forceElfe;sortElfe
                    reponseServeur = m_nain.Vie + ";" + m_nain.Force + ";" + m_nain.Arme + ";" + m_elfe.Vie + ";" + m_elfe.Force + ";" + m_elfe.Sort;

                    lstReception.Items.Add(reponseServeur);
                    lstReception.Update();

                    tByteEnvoie = textByte.GetBytes(reponseServeur);

                    lstReception.Items.Add(Encoding.ASCII.GetString(tByteEnvoie));
                    lstReception.Update();

                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);

                    // ferme le socket
                    m_client.Close();

                    // vérifie s’il y a un gagnant
                    if (m_elfe.Vie <= 0)
                    {
                        lstReception.Items.Add("Nain gagnant!");
                        lstReception.Update();
                        return;
                    }
                    if (m_nain.Vie <= 0)
                    {
                        lstReception.Items.Add("Elfe gagnant!");
                        lstReception.Update();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            m_elfe.Vie = 0;
            m_nain.Vie = 0;
            try
            {
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender, e);
            try
            {
                // il faut avoir un objet TCPListener existant
                m_client.Close();
                m_ServerListener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}

