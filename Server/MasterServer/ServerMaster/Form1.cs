using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerMaster
{
    public partial class Form1 : Form
    {

        bool terminating = false;
        bool listening = false;
        
        
        Socket masterServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> socketList = new List<Socket>(); // list of client socket connected to server

        string MasterServerPrivPucKeyPair_XML = "";
        string Server1PubKey = "";
        string Server2PubKey = "";
        Byte[] randomlyGenerated = new Byte[48];

        string portNumber;

        private Byte[] generateKey_IV_HMAC()  //using RNGCryptoServiceProvider Class
        {
            Byte[] randomByteArray = new Byte[48];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(randomByteArray);
            }
            return randomByteArray;
        }
        static byte[] encryptWithRSA(string input, int algoLength, string xmlStringKey, ref bool success)
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
                success = true;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);

            }

            return result;
        }

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

        public byte[] generateNegativeAck(string failuremessage)
        {
            richTextBox1.AppendText(failuremessage + "\n");
            byte[] signednegativeack = signWithRSA(failuremessage, 3072, MasterServerPrivPucKeyPair_XML);
            string hexsignednegativeack = generateHexStringFromByteArray(signednegativeack);
            richTextBox1.AppendText("Signed negative acknowledgement: " + hexsignednegativeack + "\n");
            return signednegativeack;
        }

        public byte[] generatePositiveAct(string positiveMessage)
        {
            richTextBox1.AppendText("File was transfered and stored successfully \n");
            byte[] signedPositiveAck = signWithRSA(positiveMessage, 3072, MasterServerPrivPucKeyPair_XML);
            string hexSignedPositiveAck = generateHexStringFromByteArray(signedPositiveAck);
            richTextBox1.AppendText("Signed positive acknowledgement: " + hexSignedPositiveAck + "\n");
            return signedPositiveAck;
        }

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

        private void printKeysAsHex(string xmlStr)
        {
            byte[] byt = Encoding.Default.GetBytes(xmlStr);
            string hexVersion = generateHexStringFromByteArray(byt);
            richTextBox1.AppendText(hexVersion + "\n");
        }

        private Byte[] encryptByte48WithRSA_3072(Byte[] randomlyGen, string xml3072BitsKey, ref bool isEncrypted)
        {
            string randomlyGenStr = Encoding.Default.GetString(randomlyGen); ;
            Byte[] encryptedRSA = encryptWithRSA(randomlyGenStr, 3072, xml3072BitsKey, ref isEncrypted);
            richTextBox1.AppendText("RSA 3072 Encryption: "); 
            richTextBox1.AppendText(generateHexStringFromByteArray(encryptedRSA));

            return encryptedRSA;
        }
        private void getRSAKeys() //from text files
        {
            string str1 = "", str2 = "", str3 = "";

            string workingDirectory = Environment.CurrentDirectory;              //reading "MasterServer_pub_prv.txt"
            string projectDirReal = Directory.GetParent(workingDirectory).Parent.FullName;
            string path = Path.Combine(projectDirReal, @"MasterServer_pub_prv.txt");
            if (File.Exists(path))
            {
                string[] lines1 = System.IO.File.ReadAllLines(path);
                foreach (string line in lines1)
                {
                    str1 += line;
                }
                MasterServerPrivPucKeyPair_XML = str1;
                richTextBox1.AppendText("This server has its own public-private key pair.\n");
                printKeysAsHex(MasterServerPrivPucKeyPair_XML);

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
                    Server1PubKey = str2;
                    richTextBox1.AppendText("This server has Server1's public key.\n");
                    printKeysAsHex(Server1PubKey);

                    

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
                        Server2PubKey = str3;
                        richTextBox1.AppendText("This server has Server2's public key.\n");
                        printKeysAsHex(Server2PubKey);

                        
                    }
                    else { richTextBox1.AppendText("Error while reading one of the keys from the txt file.\n"); }
                }
                else { richTextBox1.AppendText("Error while reading one of the keys from the txt file.\n"); }
            }
            else { richTextBox1.AppendText("Error while reading one of the keys from the txt file.\n"); }
        }    
        
        private void displayKeyIVHMAC(byte[] bytes48)
        {
            string bytes48str = generateHexStringFromByteArray(bytes48);
            string key16 = bytes48str.Substring(0, 32);
            string IV16 = bytes48str.Substring(32, 32);
            string HMAC16 = bytes48str.Substring(64, 32);

            richTextBox1.AppendText("AES key: " + key16 + "\n");
            richTextBox1.AppendText("IV: " + IV16 + "\n");
            richTextBox1.AppendText("HMAC: " + HMAC16 + "\n");
        }   

        private void sendServerInfo(ref Socket s)
        {
            byte[] serverInfo = new Byte[64];
            serverInfo = Encoding.Default.GetBytes("MasterServer");
            s.Send(serverInfo);
            richTextBox1.AppendText("Server info is sent to the recipient: \"ServerMaster\"\n");
        }

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = masterServerSocket.Accept();
                    socketList.Add(newClient);
                    
                    richTextBox1.AppendText("A server or a client is connected.\n");


                    Socket s = socketList[socketList.Count - 1];
                    sendServerInfo(ref s); //indicating which server the client is connected now, sending server name/type






                    Thread receiveThread = new Thread(new ThreadStart(Receive));
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
                        richTextBox1.AppendText("Socket has stopped working. \n");
                    }
                }
            }
        }

        private void Receive()
        {
            //will be implemented
            int index = socketList.Count - 1;
            Socket s = socketList[index]; //client that is newly added
            bool connected = true;

            while (!terminating && connected)
            {
                try
                {
                    
                    Byte[] buffer = new Byte[384];
                    s.Receive(buffer);
                    string message = Encoding.Default.GetString(buffer);
                    message = message.Substring(0, message.IndexOf("\0"));

                    

                    if (message == "Session Key Request, 1")//if server1 requests a session key
                    {
                        richTextBox1.AppendText("Server1 requests a session key.\n");
                        randomlyGenerated = generateKey_IV_HMAC(); //generating 48 bytes randomly
                        bool isEncrypted = false;
                        Byte[] encrypted48 = new Byte[48];
                        richTextBox1.AppendText("Generated key:  \n" + generateHexStringFromByteArray(randomlyGenerated) + "\n");

                        displayKeyIVHMAC(randomlyGenerated);

                        richTextBox1.AppendText("Encrypting the session key for Server1...\n");
                        encrypted48 = encryptByte48WithRSA_3072(randomlyGenerated, Server1PubKey, ref isEncrypted);//encrypting the 48 bytes with RSA3072

                       
                        if (isEncrypted)//if encryption is successfull
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 is completed succesfully.\n");
                            richTextBox1.AppendText("Now signing with MasterServer's priavte key...\n");

                            string toBeSend1 = Encoding.Default.GetString(encrypted48); 

                            Byte[] signatureFromEnc48 = signWithRSA(toBeSend1, 3072, MasterServerPrivPucKeyPair_XML); //signing the encrypted version of geenrated 48bytes

                            string toBeSend2 = Encoding.Default.GetString(signatureFromEnc48);
                            richTextBox1.AppendText("Signature : \n" + generateHexStringFromByteArray(signatureFromEnc48) + "\n");
                            Byte[] sendByte1 = new Byte[384];
                            sendByte1 = encrypted48;                      
                            Byte[] sendByte2 = new Byte[384];
                            sendByte2 = signatureFromEnc48;                                

                            s.Send(sendByte1); //encrypted48
                            s.Send(sendByte2); //signature of encrypted48

                        }
                        else
                        {   
                            richTextBox1.AppendText("Encryption with RSA-3072 failed.\n");
                        }

                    }
                    else if (message == "Session Key Request, 2")//if server2 requests a session key
                    {
                        richTextBox1.AppendText("Server2 requests a session key.\n");
                        randomlyGenerated = generateKey_IV_HMAC(); //generating 48 bytes randomly
                        bool isEncrypted = false;
                        Byte[] encrypted48 = new Byte[48];
                        richTextBox1.AppendText("Generated key:  \n" + generateHexStringFromByteArray(randomlyGenerated) + "\n");

                        displayKeyIVHMAC(randomlyGenerated);

                        richTextBox1.AppendText("Encrypting the session key for Server2...\n");
                        encrypted48 = encryptByte48WithRSA_3072(randomlyGenerated, Server2PubKey, ref isEncrypted);//encrypting the 48 bytes with RSA3072


                        if (isEncrypted)//if encryption is succesfull
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 is completed succesfully.\n");
                            richTextBox1.AppendText("Now signing with MasterServer's priavte key...\n");

                            string toBeSend1 = Encoding.Default.GetString(encrypted48);

                            Byte[] signatureFromEnc48 = signWithRSA(toBeSend1, 3072, MasterServerPrivPucKeyPair_XML); //signing the encrypted version of geenrated 48bytes

                            string toBeSend2 = Encoding.Default.GetString(signatureFromEnc48);
                            richTextBox1.AppendText("Signature : \n" + generateHexStringFromByteArray(signatureFromEnc48) + "\n");
                            Byte[] sendByte1 = new Byte[384];
                            sendByte1 = encrypted48;
                            Byte[] sendByte2 = new Byte[384];
                            sendByte2 = signatureFromEnc48;

                            s.Send(sendByte1); //encrypted48
                            s.Send(sendByte2); //signature of encrypted48

                        }
                        else
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 failed.\n");
                        }
                    }
                    else if (message == "OK, 1") //server1, session key verified //OK
                    {
                        //complete session key transfer
                        richTextBox1.AppendText("Sesion key transfer to server 1 is completed succesfully.\n");
                    }
                    else if (message == "OK, 2") //server2, session key verified //OK
                    {
                        //complete session key transfer
                        richTextBox1.AppendText("Sesion key transfer to server 2 is completed succesfully.\n");
                    }

                    else if (message == "NOTOK, 1") //server1 sends it, not verified session key //NOTOK
                    {
                        richTextBox1.AppendText("Error while sesion key transfer to Server1. Protocol for sending the session key starts over now.\n");

                        randomlyGenerated = generateKey_IV_HMAC(); //generating 48 bytes randomly
                        bool isEncrypted = false;
                        Byte[] encrypted48 = new Byte[48];
                        richTextBox1.AppendText("Generated key:  \n" + generateHexStringFromByteArray(randomlyGenerated) + "\n");

                        displayKeyIVHMAC(randomlyGenerated);

                        richTextBox1.AppendText("Encrypting the session key for Server1...\n");
                        encrypted48 = encryptByte48WithRSA_3072(randomlyGenerated, Server1PubKey, ref isEncrypted);//encrypting the 48 bytes with RSA3072


                        if (isEncrypted)//if encryption is succesfull
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 is completed succesfully.\n");
                            richTextBox1.AppendText("Now signing with MasterServer's priavte key...\n");

                            string toBeSend1 = Encoding.Default.GetString(encrypted48);

                            Byte[] signatureFromEnc48 = signWithRSA(toBeSend1, 3072, MasterServerPrivPucKeyPair_XML); //signing the encrypted version of geenrated 48bytes

                            string toBeSend2 = Encoding.Default.GetString(signatureFromEnc48);
                            richTextBox1.AppendText("Signature : \n" + generateHexStringFromByteArray(signatureFromEnc48) + "\n");
                            Byte[] sendByte1 = new Byte[384];
                            sendByte1 = encrypted48;
                            Byte[] sendByte2 = new Byte[384];
                            sendByte2 = signatureFromEnc48;

                            s.Send(sendByte1); //encrypted48
                            s.Send(sendByte2); //signature of encrypted48

                        }
                        else
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 failed.\n");
                        }

                    }
                    else if (message == "NOTOK, 2") //server2 sends it, not verified session key //NOTOK
                    {
                        richTextBox1.AppendText("Error while sesion key transfer to server 2. Protocol for sending the session key starts over now.\n");

                        randomlyGenerated = generateKey_IV_HMAC(); //generating 48 bytes randomly
                        bool isEncrypted = false;
                        Byte[] encrypted48 = new Byte[48];
                        richTextBox1.AppendText("Generated key:  \n" + generateHexStringFromByteArray(randomlyGenerated) + "\n");

                        displayKeyIVHMAC(randomlyGenerated);

                        richTextBox1.AppendText("Encrypting the session key for Server2...\n");
                        encrypted48 = encryptByte48WithRSA_3072(randomlyGenerated, Server2PubKey, ref isEncrypted);//encrypting the 48 bytes with RSA3072


                        if (isEncrypted)//if encryption is successfull
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 is completed succesfully.\n");
                            richTextBox1.AppendText("Now signing with MasterServer's priavte key...\n");

                            string toBeSend1 = Encoding.Default.GetString(encrypted48);

                            Byte[] signatureFromEnc48 = signWithRSA(toBeSend1, 3072, MasterServerPrivPucKeyPair_XML); //signing the encrypted version of geenrated 48bytes

                            string toBeSend2 = Encoding.Default.GetString(signatureFromEnc48);
                            richTextBox1.AppendText("Signature : \n" + generateHexStringFromByteArray(signatureFromEnc48) + "\n");
                            Byte[] sendByte1 = new Byte[384];
                            sendByte1 = encrypted48;
                            Byte[] sendByte2 = new Byte[384];
                            sendByte2 = signatureFromEnc48;

                            s.Send(sendByte1); //encrypted48
                            s.Send(sendByte2); //signature of encrypted48

                        }
                        else
                        {
                            richTextBox1.AppendText("Encryption with RSA-3072 failed.\n");
                        }
                    }
                    //if client wants to upload a file ...
                    else //anything else, file upload
                    {

                        //richTextBox1.AppendText(message);

                        //-----------Create directory to hold files-----------
                        string workingDirectory = Environment.CurrentDirectory;
                        string destfolder = Directory.GetParent(workingDirectory).Parent.FullName;
                        string newDir = destfolder + "\\" + "IncomingFiles";
                        System.IO.Directory.CreateDirectory(newDir);

                        //------Get file name size and file size in terms of number of packages (encrypted with RSA)----------
                        byte[] fileAndNameSize = new Byte[384];
                        s.Receive(fileAndNameSize);
                        string fileNameSize = Encoding.Default.GetString(fileAndNameSize);
                        byte[] decFileAndNSize = decryptWithRSA(fileNameSize, 3072, MasterServerPrivPucKeyPair_XML);
                        string decFileAndNSizeSt = Encoding.Default.GetString(decFileAndNSize);

                        //-------Get file name size and file size from given string--------
                        string fileNameS = decFileAndNSizeSt.Substring(0, decFileAndNSizeSt.IndexOf("*"));
                        string fileSize = decFileAndNSizeSt.Substring(decFileAndNSizeSt.IndexOf("*") + 1, (decFileAndNSizeSt.Length - fileNameS.Length) - 1);
                        int fileNameSInt = Int32.Parse(fileNameS);
                        int fileSizeInt = Int32.Parse(fileSize);   //number of packages

                        //--------------Get AES key----------------------------
                        byte[] AESKey = new Byte[384];
                        s.Receive(AESKey);
                        string AESKeyString = Encoding.Default.GetString(AESKey);
                        byte[] decryptedAESKey = decryptWithRSA(AESKeyString, 3072, MasterServerPrivPucKeyPair_XML);
                        string decryptedAESKeyHex = generateHexStringFromByteArray(decryptedAESKey);
                        richTextBox1.AppendText("AES key: " + decryptedAESKeyHex + "\n");

                        //--------------Get AES IV----------------------------
                        byte[] AESIV = new Byte[384];
                        s.Receive(AESIV);
                        string AESIVString = Encoding.Default.GetString(AESIV);
                        byte[] decryptedAESIV = decryptWithRSA(AESIVString, 3072, MasterServerPrivPucKeyPair_XML);
                        string decryptedAESIVHex = generateHexStringFromByteArray(decryptedAESIV);
                        richTextBox1.AppendText("AES IV: " + decryptedAESIVHex + "\n");

                        //---------------Get file name---------------------
                        byte[] fileNameComing = new Byte[fileNameSInt];
                        s.Receive(fileNameComing);
                        string fileNameEnc = Encoding.Default.GetString(fileNameComing);
                        byte[] fileNameDec = decryptWithAES128(fileNameEnc, decryptedAESKey, decryptedAESIV);
                        string fileName = Encoding.Default.GetString(fileNameDec);
                        richTextBox1.AppendText(fileName + " is sent by client\n");

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
                                    byte[] negativeAck = generateNegativeAck("File decryption was failed\n");
                                    s.Send(negativeAck);
                                }
                                else
                                {

                                    string fileDecryptedString = Encoding.Default.GetString(fileDecrypted);
                                    fileDecrypted = null;
                                    byte[] positiveAck = generatePositiveAct(fileDecryptedString);

                                    s.Send(positiveAck);
                                }

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
                }
                catch
                {
                    if (!terminating)
                    {
                        richTextBox1.AppendText("A client is disconnected. \n");
                    }

                    s.Close();
                    socketList.Remove(s);
                    connected = false;
                }
            }


        }

        private void button1_Click(object sender, EventArgs e) //listen button
        {
            portNumber = textBox1.Text;
            int portInt;

            if (Int32.TryParse(textBox1.Text, out portInt))
            {
                System.Net.IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portInt);
                masterServerSocket.Bind(endPoint);
                masterServerSocket.Listen(5);

                listening = true;
                button1.Enabled = false;

                Thread acceptThread = new Thread(new ThreadStart(Accept));
                acceptThread.Start();
                richTextBox1.AppendText("Started listening on port: " + portInt + ".\n");
                Byte[] b = generateKey_IV_HMAC();

                getRSAKeys();
            }
            else
            {
                richTextBox1.AppendText("Please check the port number.\n");
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void textBoxGetPort_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
