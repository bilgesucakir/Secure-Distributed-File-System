using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace Server1_2
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool listening = false;
        bool masterConnected = false;

        string Server1PubPrivKey_XML;
        string MasterServerPubKey;
        string Server2PubKey;

        byte[] sessionKey;
        byte[] sessionIV;
        byte[] sessionHMac;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();
        Socket masterSocket;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
            getRSAKeys();
        }

        private void getRSAKeys() //from text files
        {
            string str1 = "", str2 = "", str3 = "";

            string workingDirectory = Environment.CurrentDirectory;              //reading "Server1_pub_prv.txt"
            string projectDirReal = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = Path.Combine(projectDirReal, @"Server1_pub_prv.txt");
            if (File.Exists(path))
            {
                string[] lines1 = System.IO.File.ReadAllLines(path);
                foreach (string line in lines1)
                {
                    str1 += line;
                }
                Server1PubPrivKey_XML = str1;
                Byte[] Server1PubPrivKey_XMLbyte = Encoding.Default.GetBytes(Server1PubPrivKey_XML);
                logs.AppendText("Server1's public-private key pair: " + generateHexStringFromByteArray(Server1PubPrivKey_XMLbyte) + "\n");

                string workingDirectory2 = Environment.CurrentDirectory;            //reading "MasterServer_pub.txt"
                string projectDirReal2 = Directory.GetParent(workingDirectory2).Parent.FullName;
                string path2 = Path.Combine(projectDirReal2, @"MasterServer_pub.txt");
                if (File.Exists(path2))
                {
                    string[] lines2 = System.IO.File.ReadAllLines(path2);
                    foreach (string line in lines2)
                    {
                        str2 += line;
                    }
                    MasterServerPubKey = str2;
                    Byte[] MasterServerPubKeybyte = Encoding.Default.GetBytes(MasterServerPubKey);
                    logs.AppendText("Master's public key: " + generateHexStringFromByteArray(MasterServerPubKeybyte) + "\n");

                    string workingDirectory3 = Environment.CurrentDirectory;              //reading "Server2_pub.txt"
                    string projectDirReal3 = Directory.GetParent(workingDirectory3).Parent.FullName;
                    string path3 = Path.Combine(projectDirReal3, @"Server2_pub.txt");
                    if (File.Exists(path3))
                    {
                        string[] lines3 = System.IO.File.ReadAllLines(path3);
                        foreach (string line in lines3)
                        {
                            str3 += line;
                        }
                        Server2PubKey = str3;
                        Byte[] Server2PubKeybyte = Encoding.Default.GetBytes(Server2PubKey);
                        logs.AppendText("Server2's public key: " + generateHexStringFromByteArray(Server2PubKeybyte) + "\n");
                    }
                    else { logs.AppendText("3 Error while reading one of the keys from the txt file.\n"); }
                }
                else { logs.AppendText("2 Error while reading one of the keys from the txt file.\n"); }
            }
            else { logs.AppendText("1 Error while reading one of the keys from the txt file.\n"); }

        }

        private void listen_button_Click(object sender, EventArgs e)
        {
            int serverPort;
            Thread acceptThread;

            if (Int32.TryParse(listenPortNum.Text, out serverPort))
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                serverSocket.Listen(3);

                listening = true;
                listen_button.Enabled = false;
                acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();

                logs.AppendText("Started listening on port: " + serverPort + "\n");
            }
            else
            {
                logs.AppendText("Please check port number \n");
            }
        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            bool keyExchangeDone = false;
            masterSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = IpAdress.Text;
            int port;
            if (Int32.TryParse(masterPortNum.Text, out port))
            {
                try
                {
                    masterSocket.Connect(IP, port);
                    masterConnected = true;
                    connect_button.Enabled = false;
                    logs.AppendText("Connected to master server\n");

                    Byte[] serverinfo = new Byte[64];
                    masterSocket.Receive(serverinfo);

                    while (!keyExchangeDone)
                    {
                        Byte[] buffer = new Byte[384];
                        buffer = Encoding.Default.GetBytes("Session Key Request, 1");   //Send session key request and ID of server
                        masterSocket.Send(buffer);
                        logs.AppendText("Session key request is sent to Master");
                        Byte[] response = new Byte[384];
                        masterSocket.Receive(response);

                        string sessionKeyMessage = Encoding.Default.GetString(response);    //contains AES key, IV, HMAC

                        // decryption with RSA 3072
                        byte[] decryptedSessionMessage = decryptWithRSA(sessionKeyMessage, 3072, Server1PubPrivKey_XML);
                        logs.AppendText("Sesion decrypted session message: " + generateHexStringFromByteArray(decryptedSessionMessage) + "\n");

                        Byte[] signiture = new Byte[384];        //get signiture(48 byte of enc. session info is signed)
                        masterSocket.Receive(signiture);

                        string sessionSigniture = Encoding.Default.GetString(signiture);
                        logs.AppendText("signiture message: " + generateHexStringFromByteArray(signiture) + "\n");

                        //// verifying with RSA 3072
                        bool verificationResult = verifyWithRSA(sessionKeyMessage, 3072, MasterServerPubKey, signiture);
                        if (verificationResult == true)   //if signiture verified, get session key(16 byte), IV(16 byte), HMAC(16 byte) from decrytedSessionMessage
                        {
                            logs.AppendText("Valid signature\n");

                            sessionKey = new Byte[16];
                            sessionIV = new Byte[16];
                            sessionHMac = new Byte[16];

                            Array.Copy(decryptedSessionMessage, 0, sessionKey, 0, 16);
                            Array.Copy(decryptedSessionMessage, 16, sessionIV, 0, 16);
                            Array.Copy(decryptedSessionMessage, 32, sessionHMac, 0, 16);

                            string sessionKeyHex = generateHexStringFromByteArray(sessionKey);
                            string sessionIVHex = generateHexStringFromByteArray(sessionIV);
                            string sessionHMacHex = generateHexStringFromByteArray(sessionHMac);

                            logs.AppendText("Sesion Key: " + sessionKeyHex + "\n");
                            logs.AppendText("Sesion IV: " + sessionIVHex + "\n");
                            logs.AppendText("HMAC value: " + sessionHMacHex + "\n");

                            keyExchangeDone = true;

                            Byte[] success = new Byte[48];
                            success = Encoding.Default.GetBytes("OK, 1");
                            masterSocket.Send(success);
                        }
                        else
                        {
                            logs.AppendText("Invalid signature");
                            Byte[] success = new Byte[48];
                            success = Encoding.Default.GetBytes("NOTOK, 1");
                            masterSocket.Send(success);
                        }
                    }

                }
                catch
                {
                    logs.AppendText("Could not connect to master server\n");
                }
            }
            else
            {
                logs.AppendText("Check the port\n");
            }
        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    clientSockets.Add(serverSocket.Accept());
                    logs.AppendText("A client is connected \n");

                    Socket s = clientSockets[clientSockets.Count - 1];
                    byte[] serverInfo = new Byte[64];
                    serverInfo = Encoding.Default.GetBytes("ServerOne");
                    s.Send(serverInfo);                        //send server info to client when connection happened 


                    Thread receiveThread;
                    receiveThread = new Thread(() => Receive(s));
                    receiveThread.Start();
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        logs.AppendText("The socket stopped working \n");
                    }
                }
            }
        }

        private void Receive(Socket s)
        {
            
            bool connected = true;

            while (connected && !terminating)
            {
                try
                {
                    //-----------Create directory to hold files-----------
                    string workingDirectory = Environment.CurrentDirectory;              
                    string destfolder = Directory.GetParent(workingDirectory).Parent.FullName;
                    string newDir = destfolder + "\\" + "IncomingFiles";
                    System.IO.Directory.CreateDirectory(newDir);

                    //------Get file name size and file size in terms of number of packages (encrypted with RSA)----------
                    byte[] fileAndNameSize = new Byte[384];
                    s.Receive(fileAndNameSize);
                    string fileNameSize = Encoding.Default.GetString(fileAndNameSize);
                    byte[] decFileAndNSize = decryptWithRSA(fileNameSize, 3072, Server1PubPrivKey_XML);
                    string decFileAndNSizeSt = Encoding.Default.GetString(decFileAndNSize);
                    
                    //-------Get file name size and file size from given string--------
                    string fileNameS = decFileAndNSizeSt.Substring(0, decFileAndNSizeSt.IndexOf("*"));
                    string fileSize = decFileAndNSizeSt.Substring(decFileAndNSizeSt.IndexOf("*")+1, (decFileAndNSizeSt.Length - fileNameS.Length)-1);
                    int fileNameSInt = Int32.Parse(fileNameS);
                    int fileSizeInt = Int32.Parse(fileSize);   //number of packages

                    //--------------Get AES key----------------------------
                    byte[] AESKey = new Byte[384];
                    s.Receive(AESKey);
                    string AESKeyString = Encoding.Default.GetString(AESKey);
                    byte[] decryptedAESKey = decryptWithRSA(AESKeyString, 3072, Server1PubPrivKey_XML);
                    string decryptedAESKeyHex = generateHexStringFromByteArray(decryptedAESKey);
                    logs.AppendText("AES key: " + decryptedAESKeyHex + "\n");

                    //--------------Get AES IV----------------------------
                    byte[] AESIV = new Byte[384];
                    s.Receive(AESIV);
                    string AESIVString = Encoding.Default.GetString(AESIV);
                    byte[] decryptedAESIV = decryptWithRSA(AESIVString, 3072, Server1PubPrivKey_XML);
                    string decryptedAESIVHex = generateHexStringFromByteArray(decryptedAESIV);
                    logs.AppendText("AES IV: " + decryptedAESIVHex + "\n");

                    //---------------Get file name---------------------
                    byte[] fileNameComing = new Byte[fileNameSInt];
                    s.Receive(fileNameComing);
                    string fileNameEnc = Encoding.Default.GetString(fileNameComing);
                    byte[] fileNameDec = decryptWithAES128(fileNameEnc, decryptedAESKey, decryptedAESIV);
                    string fileName = Encoding.Default.GetString(fileNameDec);
                    logs.AppendText(fileName + " is sent by client\n");

                    newDir = newDir + "\\" + fileName;
                    
                    if (!File.Exists(newDir))
                    {
                        // Create a new file     
                        FileStream fs = File.Create(newDir);
                        fs.Close();
                    }
                    
                    bool successfullDecrypt = true;
                    using (FileStream fsSource = new FileStream(newDir,
                    FileMode.Append, FileAccess.Write))
                    {
                        //recive file until coming packages finish
                        for (int i = 0; i < fileSizeInt; i++)
                        {
                            Byte[] receiveBuffer = new Byte[8208];
                            s.Receive(receiveBuffer);
                            int numBytesToWrite = 8192;
                            int numBytesWrite = 0;

                            string FilePartString = Encoding.Default.GetString(receiveBuffer);
                            //received 8208 bytes, it will decrpyt and becomes 8192
                            byte[] fileDecrypted = decryptWithAES128(FilePartString, decryptedAESKey, decryptedAESIV);  //decrpyt with AES key, IV
                            fsSource.Write(fileDecrypted, numBytesWrite, numBytesToWrite);   //write to file
                            Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
                            numBytesToWrite += 8192;
                            numBytesWrite += 8192;
                            
                           if (fileDecrypted == null)   //null means given input couldnt be decrpyted
                            {
                                successfullDecrypt = false;
                                byte[] negativeAck = generateNegativeAck("File decryption was failed");
                                s.Send(negativeAck);
                            }
                            else
                            {
                              
                                string fileDecryptedString = Encoding.Default.GetString(fileDecrypted);
                                fileDecrypted = null;
                                byte[] positiveAck = generatePositiveAct(fileDecryptedString);
                               
                                s.Send(positiveAck);
                            }
                            
                            //logs.AppendText(Encoding.Default.GetString(fileDecrypted));
                            
                        }

                    }

                    if (!successfullDecrypt)
                    {
                        if (File.Exists(newDir))
                        {
                            // If file found, delete it    
                            File.Delete(newDir);
                        }
                    }
     
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("A client has disconnected\n");
                    }

                    s.Close();
                    clientSockets.Remove(s);
                    connected = false;
                }
            }
        }

        public byte[] generateNegativeAck(string failuremessage)
        {
            logs.AppendText(failuremessage + "\n");
            byte[] signednegativeack = signWithRSA(failuremessage, 3072, Server1PubPrivKey_XML);
            string hexsignednegativeack = generateHexStringFromByteArray(signednegativeack);
            logs.AppendText("Signed negative acknowledgement: " + hexsignednegativeack + "\n");
            return signednegativeack;
        }

        public byte[] generatePositiveAct(string positiveMessage)
        {
            logs.AppendText("File was transfered and stored successfully \n");
            byte[] signedPositiveAck = signWithRSA(positiveMessage, 3072, Server1PubPrivKey_XML);
            string hexSignedPositiveAck = generateHexStringFromByteArray(signedPositiveAck);
            logs.AppendText("Signed positive acknowledgement: " + hexSignedPositiveAck + "\n");
            return signedPositiveAck;
        }

        //-------HELPER FUNCTIONS----------------
        static string generateHexStringFromByteArray(byte[] input)
        {
            string hexString = BitConverter.ToString(input);
            return hexString.Replace("-", "");
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

        // signing with RSA
        static byte[] signWithRSA(string input, int algoLength, string xmlString)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            byte[] result = null;

            try
            {
                result = rsaObject.SignData(byteInput, "SHA256");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // verifying with RSA
        static bool verifyWithRSA(string input, int algoLength, string xmlString, byte[] signature)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);
            // create RSA object from System.Security.Cryptography
            RSACryptoServiceProvider rsaObject = new RSACryptoServiceProvider(algoLength);
            // set RSA object with xml string
            rsaObject.FromXmlString(xmlString);
            bool result = false;

            try
            {
                result = rsaObject.VerifyData(byteInput, "SHA256", signature);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // encryption with AES-128
        static byte[] encryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

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
                result = encryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        // decryption with AES-128
        static byte[] decryptWithAES128(string input, byte[] key, byte[] IV)
        {
            // convert input string to byte array
            byte[] byteInput = Encoding.Default.GetBytes(input);

            // create AES object from System.Security.Cryptography
            RijndaelManaged aesObject = new RijndaelManaged();
            // since we want to use AES-128
            aesObject.KeySize = 128;
            // block size of AES is 128 bits
            aesObject.BlockSize = 128;
            // mode -> CipherMode.*
            aesObject.Mode = CipherMode.CFB;
            // feedback size should be equal to block size
            // aesObject.FeedbackSize = 128;
            // set the key
            aesObject.Key = key;
            // set the IV
            aesObject.IV = IV;
            // create an encryptor with the settings provided
            ICryptoTransform decryptor = aesObject.CreateDecryptor();
            byte[] result = null;

            try
            {
                result = decryptor.TransformFinalBlock(byteInput, 0, byteInput.Length);
            }
            catch (Exception e) // if encryption fails
            {
                Console.WriteLine(e.Message); // display the cause
            }

            return result;
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}
