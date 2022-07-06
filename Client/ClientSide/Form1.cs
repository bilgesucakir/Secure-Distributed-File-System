using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace ClientSide
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        private string shorterFileName = "";
        private string FileName = "";
        private string MasterServerPublicKey = "";
        private string ServerOnePublicKey = "";
        private string ServerTwoPublicKey = "";
        private string serverName = "";
        private string SendingData = "";
        bool verification = false;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBox_IP.Text;
            int portNumber;


            getRSAKeys();


            if ((Int32.TryParse(textBox_Port.Text, out portNumber)) && (textBox_IP.Text != ""))
            {
                
                try
                {
                    //Connect the client Socket to the given server with IP and Port Number
                    clientSocket.Connect(IP, portNumber);

                    Byte[] buffer = new Byte[64];
                    clientSocket.Receive(buffer);
                    richTextBox.Enabled = true;
                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));
                    if(message == "MasterServer")
                    {
                        serverName = "MasterServer";
                        //Make visual arrangements in the GUI
                        button_Disconnect.BackColor = Color.IndianRed;
                        button_Connect.BackColor = Color.DimGray;
                        textBox_Port.BackColor = Color.MediumSeaGreen;
                        textBox_IP.BackColor = Color.MediumSeaGreen;
                        button_Connect.Enabled = false;
                        button_Disconnect.Enabled = true;
                        button_Browse.Enabled = true;
                        button_Send.Enabled = true;
                        connected = true;
                        textBox_Port.Enabled = false;
                        textBox_IP.Enabled = false;
                        textBox_fileName.Enabled = true;

                   
                        richTextBox.AppendText("Connected to the server!\n");
                    }
                    else if(message == "ServerOne")
                    {
                        serverName = "ServerOne";
                        //Make visual arrangements in the GUI
                        button_Disconnect.BackColor = Color.IndianRed;
                        button_Connect.BackColor = Color.DimGray;
                        textBox_Port.BackColor = Color.MediumSeaGreen;
                        textBox_IP.BackColor = Color.MediumSeaGreen;
                        button_Connect.Enabled = false;
                        button_Disconnect.Enabled = true;
                        button_Browse.Enabled = true;
                        button_Send.Enabled = true;
                        connected = true;
                        textBox_Port.Enabled = false;
                        textBox_IP.Enabled = false;
                        textBox_fileName.Enabled = true;

                   
                        richTextBox.AppendText("Connected to the server!\n");
                    }
                    else 
                    {
                        serverName = "ServerTwo";
                        //Make visual arrangements in the GUI
                        button_Disconnect.BackColor = Color.IndianRed;
                        button_Connect.BackColor = Color.DimGray;
                        textBox_Port.BackColor = Color.MediumSeaGreen;
                        textBox_IP.BackColor = Color.MediumSeaGreen;
                        button_Connect.Enabled = false;
                        button_Disconnect.Enabled = true;
                        button_Browse.Enabled = true;
                        button_Send.Enabled = true;
                        connected = true;
                        textBox_Port.Enabled = false;
                        textBox_IP.Enabled = false;
                        textBox_fileName.Enabled = true;

                   
                        richTextBox.AppendText("Connected to the server!\n");
                    }

                }
                catch
                {

                    richTextBox.AppendText("Could not connect to the server!\n");
                }
            }
            else //When the taken credentials are not appropiate for the connection.
            {
                textBox_Port.BackColor = Color.IndianRed;
                textBox_IP.BackColor = Color.IndianRed;
                richTextBox.AppendText("Check the IP address and port number!\n");
            }

        }
        
        //Form closing form the up red X button
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            MasterServerPublicKey = "";
            ServerOnePublicKey = "";
            ServerTwoPublicKey = "";
            Environment.Exit(0);
        }

        //Browse operation initiated
        //FileDialog Search for the fileparth, filename and information in the file.
        private void button_Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog browseBox = new OpenFileDialog();
            browseBox.Title = "Client browsing file from her/his directory...";
            browseBox.ShowDialog();
            textBox_fileName.Text = browseBox.FileName;
            FileName = browseBox.FileName;
            shorterFileName = browseBox.SafeFileName;
        }

        private void button_Disconnect_Click(object sender, EventArgs e)
        {
            connected = false;
            clientSocket.Close();
            button_Disconnect.Enabled = false;
            button_Connect.Enabled = true;
            button_Browse.Enabled = false;
            textBox_IP.Enabled = true;
            textBox_Port.Enabled = true;
            button_Send.Enabled = false;
            button_Disconnect.BackColor = Color.DimGray;
            button_Connect.BackColor = Color.MediumSeaGreen;
            textBox_IP.BackColor = Color.White;
            textBox_Port.BackColor = Color.White;
            MasterServerPublicKey = "";
            ServerOnePublicKey = "";
            ServerTwoPublicKey = "";
            textBox_fileName.Clear();
            richTextBox.Clear();
            SendingData = "";
            textBox_IP.Clear();
            textBox_Port.Clear();
            richTextBox.AppendText("Disconnected from the server!\n");
        }

        private void button_Send_Click(object sender, EventArgs e)
        {

            if (serverName == "MasterServer")
            {

                byte[] fileUploadInfo = Encoding.Default.GetBytes("File Upload");
                clientSocket.Send(fileUploadInfo);
                if (textBox_fileName.Text != "")
                {
                    // ------------------- AES Key and IV Generation -----------------------------


                    string key = randomNumberGenerator(16);
                    byte[] key_byte = Encoding.Default.GetBytes(key);
                    richTextBox.AppendText("AES Key for file encryption process: " + generateHexStringFromByteArray(key_byte) + "\n");
                    string IV = randomNumberGenerator(16);
                    byte[] IV_byte = Encoding.Default.GetBytes(IV);
                    richTextBox.AppendText("AES IV for file encryption process: " + generateHexStringFromByteArray(IV_byte) + "\n");

                    // ----------------------------------------------------------------------------

                    // ---------------------------- Key and IV ENCRYPTION ----------------------------
                    //Server 1 Public Key Reading
                    printKeysAsHex(MasterServerPublicKey);

                    //richTextBox.AppendText(MasterServerPublicKey);

                    byte[] MasterServerPKByte = Encoding.Default.GetBytes(MasterServerPublicKey);
                    richTextBox.AppendText("Master Server Public Key in HexaDecimal Format: " + generateHexStringFromByteArray(MasterServerPKByte) + "\n");

                    // PKC
                    // encryption with RSA 3072
                    byte[] encryptedRSA_AES_Key = encryptWithRSA(key, 3072, MasterServerPublicKey);
                    //richTextBox.AppendText("AES KEY: " + key + "\n");
                    //richTextBox.AppendText("Master Server Public KEY: " + MasterServerPublicKey + "\n");
                    //richTextBox.AppendText("RSA 3072 Encryption of the AES Key: " + generateHexStringFromByteArray(encryptedRSA_AES_Key) + "\n");
                    byte[] encryptedRSA_AES_IV = encryptWithRSA(IV, 3072, MasterServerPublicKey);
                    string encryptedRSA_AES_Key_String = Encoding.Default.GetString(encryptedRSA_AES_Key);
                    string encryptedRSA_AES_IV_String = Encoding.Default.GetString(encryptedRSA_AES_IV);



                    // --------------------- FILENAME ENCRYPTION -----------------------------------------

                    byte[] shrt = Encoding.Default.GetBytes(shorterFileName);
                    byte[] encryptedFileName = encryptWithAES128(shrt, key_byte, IV_byte);

                    //-----------------------------------------------------------------------------------
                    FileStream fs = File.Open(textBox_fileName.Text, FileMode.Open, FileAccess.Read);
                    long length = new System.IO.FileInfo(textBox_fileName.Text).Length;
                    int number_of_blocks = ((int)length / 8192) + 1;
                    string numBlocks = number_of_blocks.ToString();
                    fs.Close();

                    int fileNameSizeEnc = encryptedFileName.Length;
                    string sizes = fileNameSizeEnc.ToString() + "*" + number_of_blocks.ToString();
                    byte[] encryptedFileLength = encryptWithRSA(sizes, 3072, MasterServerPublicKey);
                    clientSocket.Send(encryptedFileLength);


                    clientSocket.Send(encryptedRSA_AES_Key);
                    clientSocket.Send(encryptedRSA_AES_IV);
                    clientSocket.Send(encryptedFileName);

                    using (FileStream fsSource = new FileStream(textBox_fileName.Text,
                    FileMode.Open, FileAccess.Read))
                    {

                        for (int i = 1; i <= number_of_blocks; i++)
                        {
                            Byte[] data_packet = new Byte[8192];
                            int numBytesToRead = 8192;
                            int numBytesRead = 0;
                            fsSource.Read(data_packet, numBytesRead, numBytesToRead);
                            string data = Encoding.Default.GetString(data_packet);

                            Byte[] sendPacket = new byte[8208];
                            sendPacket = encryptWithAES128(data_packet, key_byte, IV_byte);
                            clientSocket.Send(sendPacket);



                            Byte[] DigitalSignature = new byte[384];
                            clientSocket.Receive(DigitalSignature);
                            if (DigitalSignature != null)
                            {
                                verification = verifyWithRSA(data_packet, 3072, MasterServerPublicKey, DigitalSignature);
                                richTextBox.AppendText("Digital signature for packet is confirmed.\n");
                                richTextBox.AppendText("Digital signature in hex format: \n");
                                richTextBox.AppendText(generateHexStringFromByteArray(DigitalSignature) + "\n");
                            }
                            else
                            {
                                richTextBox.AppendText("File packet has sent but digital signature could not received!!!");
                            }
                            Array.Clear(data_packet, 0, data_packet.Length);
                            numBytesRead += 8192;
                            numBytesToRead += 8192;
                        }
                        if (verification)
                        {
                            richTextBox.AppendText("Digital Signature is confirmed!!!");

                        }
                        else
                        {
                            richTextBox.AppendText("Digital Signature is not confirmed!!!");
                        }
                        fsSource.Close();
                    }
                }
                else
                {
                    richTextBox.AppendText("Intended file to send is not selected!!!");
                }
            }

            else if (serverName == "ServerOne")
            {

                if (textBox_fileName.Text != "")
                {
                    // ------------------- AES Key and IV Generation -----------------------------


                    string key = randomNumberGenerator(16);
                    byte[] key_byte = Encoding.Default.GetBytes(key);
                    richTextBox.AppendText("AES Key for file encryption process: " + generateHexStringFromByteArray(key_byte) + "\n");
                    string IV = randomNumberGenerator(16);
                    byte[] IV_byte = Encoding.Default.GetBytes(IV);
                    richTextBox.AppendText("AES IV for file encryption process: " + generateHexStringFromByteArray(IV_byte) + "\n");

                    // ----------------------------------------------------------------------------

                    // ---------------------------- Key and IV ENCRYPTION ----------------------------
                    //Server 1 Public Key Reading
                    printKeysAsHex(ServerOnePublicKey);

                    //richTextBox.AppendText(ServerOnePublicKey);
                    //ServerOnePublicKey = ServerOnePublicKey.Substring(0, ServerOnePublicKey.IndexOf("\0"));
                    byte[] ServerOnePKByte = Encoding.Default.GetBytes(ServerOnePublicKey);
                    richTextBox.AppendText("Server One Public Key in HexaDecimal Format: " + generateHexStringFromByteArray(ServerOnePKByte) + "\n");

                    // PKC
                    // encryption with RSA 3072
                    byte[] encryptedRSA_AES_Key = encryptWithRSA(key, 3072, ServerOnePublicKey);
                    //richTextBox.AppendText("AES KEY: " + key + "\n");

                    //richTextBox.AppendText("ServerOne KEY: " + ServerOnePublicKey + "\n");
                    //richTextBox.AppendText("RSA 3072 Encryption of the AES Key: " + generateHexStringFromByteArray(encryptedRSA_AES_Key) + "\n");
                    byte[] encryptedRSA_AES_IV = encryptWithRSA(IV, 3072, ServerOnePublicKey);
                    string encryptedRSA_AES_Key_String = Encoding.Default.GetString(encryptedRSA_AES_Key);
                    string encryptedRSA_AES_IV_String = Encoding.Default.GetString(encryptedRSA_AES_IV);


                    byte[] shrt = Encoding.Default.GetBytes(shorterFileName);
                    // --------------------- FILENAME ENCRYPTION -----------------------------------------
                    byte[] encryptedFileName = encryptWithAES128(shrt, key_byte, IV_byte);
                    //richTextBox.AppendText("burdaaaa "+ generateHexStringFromByteArray(encryptedFileName) + "    " + generateHexStringFromByteArray(key_byte)  + "    " + generateHexStringFromByteArray(IV_byte));
                    //-----------------------------------------------------------------------------------
                    FileStream fs = File.Open(textBox_fileName.Text, FileMode.Open, FileAccess.Read);
                    long length = new System.IO.FileInfo(textBox_fileName.Text).Length;
                    int number_of_blocks = ((int)length / 8192) + 1;
                    string numBlocks = number_of_blocks.ToString();
                    fs.Close();

                    int fileNameSizeEnc = encryptedFileName.Length;
                    //int fileSizeEnc = encryptedFileAES128.Length;

                    string sizes = fileNameSizeEnc.ToString() + "*" + number_of_blocks.ToString();
                    byte[] encryptedFileLength = encryptWithRSA(sizes, 3072, ServerOnePublicKey);
                    clientSocket.Send(encryptedFileLength);


                    clientSocket.Send(encryptedRSA_AES_Key);
                    clientSocket.Send(encryptedRSA_AES_IV);
                    clientSocket.Send(encryptedFileName);

                    using (FileStream fsSource = new FileStream(textBox_fileName.Text,
                    FileMode.Open, FileAccess.Read))
                    {

                        for (int i = 1; i <= number_of_blocks; i++)
                        {
                            Byte[] data_packet = new Byte[8192];
                            int numBytesToRead = 8192;
                            int numBytesRead = 0;
                            fsSource.Read(data_packet, numBytesRead, numBytesToRead);
                            string data = Encoding.Default.GetString(data_packet);

                            Byte[] sendPacket = new byte[8208];
                            sendPacket = encryptWithAES128(data_packet, key_byte, IV_byte);
                            clientSocket.Send(sendPacket);



                            Byte[] DigitalSignature = new byte[384];
                            clientSocket.Receive(DigitalSignature);
                            if (DigitalSignature != null)
                            {
                                verification = verifyWithRSA(data_packet, 3072, ServerOnePublicKey, DigitalSignature);
                                richTextBox.AppendText("Digital signature for packet is confirmed.\n");
                                richTextBox.AppendText("Digital signature in hex format: \n");
                                richTextBox.AppendText(generateHexStringFromByteArray(DigitalSignature) + "\n");
                            }
                            else
                            {
                                richTextBox.AppendText("File packet has sent but digital signature could not received!!!");
                            }
                            Array.Clear(data_packet, 0, data_packet.Length);
                            numBytesRead += 8192;
                            numBytesToRead += 8192;
                        }
                        if (verification)
                        {
                            richTextBox.AppendText("Digital Signature is confirmed!!!");

                        }
                        else
                        {
                            richTextBox.AppendText("Digital Signature is not confirmed!!!");
                        }
                        fsSource.Close();
                    }
                }
                else
                {
                    richTextBox.AppendText("Intended file to send is not selected!!!");
                }


            }

            else
            {
                if (textBox_fileName.Text != "")
                {
                    // ------------------- AES Key and IV Generation -----------------------------


                    string key = randomNumberGenerator(16);
                    byte[] key_byte = Encoding.Default.GetBytes(key);
                    richTextBox.AppendText("AES Key for file encryption process: " + generateHexStringFromByteArray(key_byte) + "\n");
                    string IV = randomNumberGenerator(16);
                    byte[] IV_byte = Encoding.Default.GetBytes(IV);
                    richTextBox.AppendText("AES IV for file encryption process: " + generateHexStringFromByteArray(IV_byte) + "\n");

                    // ----------------------------------------------------------------------------

                    // ---------------------------- Key and IV ENCRYPTION ----------------------------
                    //Server 1 Public Key Reading
                    printKeysAsHex(ServerTwoPublicKey);



                    //richTextBox.AppendText(ServerTwoPublicKey);

                    byte[] ServerTwoPKByte = Encoding.Default.GetBytes(ServerTwoPublicKey);
                    richTextBox.AppendText("Server Two Public Key in HexaDecimal Format: " + generateHexStringFromByteArray(ServerTwoPKByte) + "\n");

                    // PKC
                    // encryption with RSA 3072
                    byte[] encryptedRSA_AES_Key = encryptWithRSA(key, 3072, ServerTwoPublicKey);
                    //richTextBox.AppendText("AES KEY: " + key + "\n");
                    //richTextBox.AppendText("ServerTwo KEY: " + ServerTwoPublicKey + "\n");
                    //richTextBox.AppendText("RSA 3072 Encryption of the AES Key: " + generateHexStringFromByteArray(encryptedRSA_AES_Key) + "\n");
                    byte[] encryptedRSA_AES_IV = encryptWithRSA(IV, 3072, ServerTwoPublicKey);
                    string encryptedRSA_AES_Key_String = Encoding.Default.GetString(encryptedRSA_AES_Key);
                    string encryptedRSA_AES_IV_String = Encoding.Default.GetString(encryptedRSA_AES_IV);



                    // --------------------- FILENAME ENCRYPTION -----------------------------------------

                    byte[] shrt = Encoding.Default.GetBytes(shorterFileName);
                    byte[] encryptedFileName = encryptWithAES128(shrt, key_byte, IV_byte);

                    //-----------------------------------------------------------------------------------
                    FileStream fs = File.Open(textBox_fileName.Text, FileMode.Open, FileAccess.Read);
                    long length = new System.IO.FileInfo(textBox_fileName.Text).Length;
                    int number_of_blocks = ((int)length / 8192) + 1;
                    string numBlocks = number_of_blocks.ToString();
                    fs.Close();

                    int fileNameSizeEnc = encryptedFileName.Length;
                    string sizes = fileNameSizeEnc.ToString() + "*" + number_of_blocks.ToString();
                    byte[] encryptedFileLength = encryptWithRSA(sizes, 3072, ServerTwoPublicKey);
                    clientSocket.Send(encryptedFileLength);


                    clientSocket.Send(encryptedRSA_AES_Key);
                    clientSocket.Send(encryptedRSA_AES_IV);
                    clientSocket.Send(encryptedFileName);

                    using (FileStream fsSource = new FileStream(textBox_fileName.Text,
                    FileMode.Open, FileAccess.Read))
                    {

                        for (int i = 1; i <= number_of_blocks; i++)
                        {
                            Byte[] data_packet = new Byte[8192];
                            int numBytesToRead = 8192;
                            int numBytesRead = 0;
                            fsSource.Read(data_packet, numBytesRead, numBytesToRead);
                            string data = Encoding.Default.GetString(data_packet);

                            Byte[] sendPacket = new byte[8208];
                            sendPacket = encryptWithAES128(data_packet, key_byte, IV_byte);
                            clientSocket.Send(sendPacket);



                            Byte[] DigitalSignature = new byte[384];
                            clientSocket.Receive(DigitalSignature);
                            if (DigitalSignature != null)
                            {
                                verification = verifyWithRSA(data_packet, 3072, ServerTwoPublicKey, DigitalSignature);
                                richTextBox.AppendText("Digital signature for packet is confirmed.\n");
                                richTextBox.AppendText("Digital signature in hex format: \n");
                                richTextBox.AppendText(generateHexStringFromByteArray(DigitalSignature) + "\n");
                            }
                            else
                            {
                                richTextBox.AppendText("File packet has sent but digital signature could not received!!!");
                            }
                            Array.Clear(data_packet, 0, data_packet.Length);
                            numBytesRead += 8192;
                            numBytesToRead += 8192;
                        }
                        if (verification)
                        {
                            richTextBox.AppendText("Digital Signature is confirmed!!!");

                        }
                        else
                        {
                            richTextBox.AppendText("Digital Signature is not confirmed!!!");
                        }
                        fsSource.Close();
                    }
                }
                else
                {
                    richTextBox.AppendText("Intended file to send is not selected!!!");
                }



            }

        }

        private void printKeysAsHex(string xmlStr)
        {
            byte[] byt = Encoding.Default.GetBytes(xmlStr);
            string hexVersion = generateHexStringFromByteArray(byt);
            richTextBox.AppendText(hexVersion + "\n");
        }

        private void getRSAKeys() //from text files
        {
            string str1 = "", str2 = "", str3 = "";

            string workingDirectory = Environment.CurrentDirectory;              //reading "MasterServer_pub_prv.txt"
            string projectDirReal = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = Path.Combine(projectDirReal, @"MasterServer_pub.txt");
            if (File.Exists(path))
            {
                string[] lines1 = System.IO.File.ReadAllLines(path);
                foreach (string line in lines1)
                {
                    str1 += line;
                }
                MasterServerPublicKey = str1;
                richTextBox.AppendText("This server has Master Server's public key.\n");
                

                string workingDirectory2 = Environment.CurrentDirectory;            //reading "Server1_pub.txt"
                string projectDirReal2 = Directory.GetParent(workingDirectory2).Parent.FullName;
                string path2 = Path.Combine(projectDirReal2, @"Server1_pub.txt");
                if (File.Exists(path2))
                {
                    string[] lines2 = System.IO.File.ReadAllLines(path2);
                    foreach (string line1 in lines2)
                    {
                        str2 += line1;
                    }
                    ServerOnePublicKey = str2;
                    richTextBox.AppendText("This server has Server1's public key.\n");
                    



                    string workingDirectory3 = Environment.CurrentDirectory;              //reading "Server2_pub.txt"
                    string projectDirReal3 = Directory.GetParent(workingDirectory3).Parent.FullName;
                    string path3 = Path.Combine(projectDirReal3, @"Server2_pub.txt");
                    if (File.Exists(path3))
                    {
                        string[] lines3 = System.IO.File.ReadAllLines(path3);
                        foreach (string line2 in lines3)
                        {
                            str3 += line2;
                        }
                        ServerTwoPublicKey = str3;
                        richTextBox.AppendText("This server has Server2's public key.\n");
                        


                    }
                    else { richTextBox.AppendText("Error while reading one of the keys from the txt file.\n"); }
                }
                else { richTextBox.AppendText("Error while reading one of the keys from the txt file.\n"); }
            }
            else { richTextBox.AppendText("Error while reading one of the keys from the txt file.\n"); }
        }

        static byte[] encryptWithAES128(byte [] input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            //byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform encryptor = aesObject.CreateEncryptor();
            byte[] result = null;

            try
            {
                result = encryptor.TransformFinalBlock(input, 0, input.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        // RSA decryption with varying bit length
        static byte[] decryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                result = rsaObject.Decrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // RSA encryption with varying bit length
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlStringKey);
            byte[] result = null;

            try
            {
                //true flag is set to perform direct RSA encryption using OAEP padding
                result = rsaObject.Encrypt(byteInput, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // helper functions
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

        public string randomNumberGenerator(int length)
        {
            Byte[] bytesRandom = new Byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytesRandom);
            }
            string randomNumber = Encoding.Default.GetString(bytesRandom).Trim('\0');
            richTextBox.AppendText((length*8).ToString() + "-bit Random Number: " + generateHexStringFromByteArray(bytesRandom) + "\n"); // For debugging purposes
            return randomNumber;
        }

        static bool verifyWithRSA(byte[] input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            //byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(input, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        public byte[] FileToByteArray(string fileName)  
        {  
            byte[] fileContent = null;  
            System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);  
            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(fs);  
            long byteLength = new System.IO.FileInfo(fileName).Length;  
            fileContent = binaryReader.ReadBytes((Int32)byteLength);  
            fs.Close();  
            fs.Dispose();  
            binaryReader.Close();  
            
            return fileContent;  
        }  
        

    }



    
}
        
