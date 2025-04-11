using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliente_Texto
{
    public partial class Form1 : Form
    {
        private TcpClient cliente;
        private StreamWriter escritor;
        private StreamReader leitor;
        private String usuario ="";
        public Form1()
        {
            InitializeComponent();
        }


        private async Task EscutarServidor()
        {
            // loop infinito pra sempre escutar o servidor
            while (true)
            {
                try
                {
                    //pega o que servidor mandou e armazena numa variável do tipo string ( Readlineasync)
                    string mensagem = await leitor.ReadLineAsync();

                    //se a mensagem não for nula
                    if (mensagem != null)
                    {
                        //adiciona a mensagem no listbox
                        lstMensagens.Invoke(new Action(() => lstMensagens.Items.Add($"{mensagem}")));
                    }
                }
                catch
                {
                    //Caso não consiga conversar com servidor , ele eh descontado
                    lstMensagens.Invoke(new Action(() => lstMensagens.Items.Add("Desconectado do servidor.")));
                    break;
                }
            }
        }





        private void btnEnviar_Click_1(object sender, EventArgs e)
        {
            //caso a mensagem não seja vazia ou com espaços branco
            if (!string.IsNullOrWhiteSpace(txtMensagem.Text))
            {
                //verifica se o usuário é válido, caso não seja , pede pra se conectar no servidor
                if(usuario == "")
                {
                    MessageBox.Show("Por Favor conecte-se e coloque um nome de usuário válido");
                    txtUsuario.Focus();
                }
                else {
                    //usa o Writeline ,armazena em memória ele não envia de uma vez;
                    escritor.WriteLine(txtMensagem.Text);
                    //o Flush ele envia pra o servidor, e apaga os arquivos da memória
                    escritor.Flush();
                    //escreve no listbox a mensagem que você acabou enviar ( só com Você) na frente da mensagem
                    lstMensagens.Items.Add($"Você: {txtMensagem.Text}");
                    txtMensagem.Clear();
                }
            }
        }



        private void btnConectar_Click_1(object sender, EventArgs e)
        {
            //verifica se tudo foi digitado corretamente
            if (!(string.IsNullOrEmpty(txtUsuario.Text)) && !(string.IsNullOrEmpty(txtIP.Text)) && !(string.IsNullOrEmpty(txtPorta.Text)))
            {
                cliente = new TcpClient();
                string ip = txtIP.Text;
                int porta = int.Parse(txtPorta.Text);

                //tenta conectar com servidor
                try
                {
                    cliente.Connect(ip, porta);
                    lstMensagens.Items.Add("Conectado ao servidor!");
                    //cria a variável do Stream que vai receber um Network stream como dito no Servidor 2 serve pra estabelecer um canal de comunicação entre o cliente e o servidor
                    var stream = cliente.GetStream(); 
                    //escritor permite que o cliente possa escrever pra o servidor
                    escritor = new StreamWriter(stream, Encoding.UTF8, 999, leaveOpen: true);
                    // leitor permite que cliente possa receber dados do servidor
                    leitor = new StreamReader(stream, Encoding.UTF8);

                    //quando acaba de conectar com o servidor , ele vai enviar uma mensagem , essa primeira mensagem , o servidor espera que seja o nome do cliente ( que será a variável usuário , que é referente o que o usuário digitou no txtUsuário)
                    usuario = txtUsuario.Text;
                    escritor.WriteLine(usuario);
                    escritor.Flush();

                   
                    txtUsuario.Enabled = false;
                    btnConectar.Enabled = false;

                   //executa o método que eu expliquei acima de escutar servidor , como disse anteriormente task serve para executar o método de forma asyncrona , numa thread separada
                    Task.Run(() => EscutarServidor());
                }
                catch (Exception ex)
                {
                    lstMensagens.Items.Add($"Erro ao conectar: {ex.Message}");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtIP.Text))
                {
                    MessageBox.Show("Por favor digite o IP");
                    txtIP.Focus();
                }
                else if (string.IsNullOrEmpty(txtPorta.Text))
                {
                    MessageBox.Show("Por favor digite a porta");
                    txtPorta.Focus();
                }
                else
                {
                    MessageBox.Show("Por favor digite o nome do usuário");
                    txtUsuario.Focus();
                }
            }
        }
        //serve pra que o usuário só digite números no campo IP e Porta
        private void txtIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            Program.IntNumber(e);
        }

        private void txtPorta_KeyPress(object sender, KeyPressEventArgs e)
        {
            Program.IntNumber(e);
        }
    }
}

