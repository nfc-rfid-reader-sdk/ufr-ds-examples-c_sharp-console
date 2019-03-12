using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uFR;
using System.Windows.Forms;

namespace uFR_AES_tester_console
{
    using DL_STATUS = System.UInt32;

    static class Functions
    {
        static DL_STATUS status;
        static byte[] aes_key_ext = new byte[16];
        static UInt32 aid = 0;
        static byte aes_key_nr = 0;
        static byte aid_key_nr_auth = 0;
        static byte file_id = 0;
        static string[] config;
        static UInt16 card_status;
        static UInt16 exec_time;
        static int auth = 0;
        static int master_auth = 0;

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static void reader_open()
        {
            status = (UInt32)uFCoder.ReaderOpen();

            if (status == 0)
            {
                Console.Write(" --------------------------------------------------\n");
                Console.Write("        uFR NFC reader successfully opened.\n");
                Console.Write(" --------------------------------------------------\n");
            }
            else
            {
                Console.WriteLine(" Error while opening device, status is: " + uFCoder.status2str((uFR.DL_STATUS)status));
            }
        }

        public static void usage()
        {
            ReadConfig();
            Console.Write(" +------------------------------------------------+\n");
            Console.Write(" |             uFR_AES_tester                     |\n");
            Console.Write(" |              DESFIRE CARDS                     |\n");
            Console.Write(" |               version 1.0                      |\n");
            Console.Write(" +------------------------------------------------+\n");
            Console.Write("                              For exit, hit escape.\n");
            Console.Write(" --------------------------------------------------\n");
            Console.Write("  (0) - Change authentication mode\n");
            Console.Write("  (1) - Master key authentication\n");
            Console.Write("  (2) - Get card UID\n");
            Console.Write("  (3) - Format card\n");
            Console.Write("  (4) - DES to AES\n");
            Console.Write("  (5) - AES to DES\n");
            Console.Write("  (6) - Get free memory\n");
            Console.Write("  (7) - Set random ID\n");
            Console.Write("  (8) - Internal key lock\n");
            Console.Write("  (9) - Internal key unlock\n");
            Console.Write("  (a) - Set baud rate\n");
            Console.Write("  (b) - Get baud rate\n");
            Console.Write("  (c) - Store AES key into reader\n");
            Console.Write("  (d) - Change AES key\n");
            Console.Write("  (e) - Change key settings\n");
            Console.Write("  (f) - Get key settings\n");
            Console.Write("  (g) - Make application\n");
            Console.Write("  (h) - Delete application\n");
            Console.Write("  (j) - Make file\n");
            Console.Write("  (k) - Delete file\n");
            Console.Write("  (l) - Write Std file\n");
            Console.Write("  (m) - Read Std file\n");
            Console.Write("  (n) - Read Value file\n");
            Console.Write("  (o) - Increase Value file\n");
            Console.Write("  (p) - Decrease Value file\n");
            Console.Write("  (r) - Change config parameters\n");
            Console.Write(" --------------------------------------------------\n");
        }

        public static void ReadConfig()
        {
            config = System.IO.File.ReadAllLines(@"..\..\config.txt");

            foreach (string line in config)
            {
                Console.WriteLine("\t" + line);
            }
        }

        public static void GetConfigParameters()
        {
            config = System.IO.File.ReadAllLines(@"..\..\config.txt");

            int doubleDot = config[0].IndexOf(':');
            config[0] = config[0].Substring(doubleDot + 2);

            doubleDot = config[1].IndexOf(':');
            config[1] = config[1].Substring(doubleDot + 2);

            doubleDot = config[2].IndexOf(':');
            config[2] = config[2].Substring(doubleDot + 2);

            doubleDot = config[3].IndexOf(':');
            config[3] = config[3].Substring(doubleDot + 2);

            doubleDot = config[4].IndexOf(':');
            config[4] = config[4].Substring(doubleDot + 2);

            aes_key_ext = StringToByteArray(config[0]);
            aid = Convert.ToUInt32(config[1], 16);
            aid_key_nr_auth = Byte.Parse(config[2]);
            file_id = Byte.Parse(config[3]);
            aes_key_nr = Byte.Parse(config[4]);
        }

        public static void GetCardUid()
        {
            byte[] card_uid = new byte[7];
            byte card_uid_len;

            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_GetDesfireUid(aes_key_nr, aid, aid_key_nr_auth, card_uid, out card_uid_len,
                         out card_status, out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_GetDesfireUid_PK(aes_key_ext, aid, aid_key_nr_auth, card_uid, out card_uid_len,
                         out card_status, out exec_time);
            }

            if (CheckStatus(status, card_status, exec_time))
            {
                Console.WriteLine(" Card UID : " + BitConverter.ToString(card_uid).Replace("-", ":"));
            }

        }

        public static void FormatCard()
        {
            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_DesfireFormatCard(aes_key_nr, out card_status, out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireFormatCard_PK(aes_key_ext, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void ChangeAuthMode()
        {
            if (auth == 1)
            {
                auth = 0;
                Console.WriteLine(" Auth mode is set to INTERNAL KEY");
            }
            else if (auth == 0)
            {
                auth = 1;
                Console.WriteLine(" Auth mode is set to PROVIDED KEY");
            }

        }

        public static void MasterKeyAuthRequired()
        {
            if (master_auth == 1)
            {
                master_auth = 0;
                Console.WriteLine(" Master key authentication is not required");
            }
            else if (master_auth == 0)
            {
                master_auth = 1;
                Console.WriteLine(" Master key authentication is now required");
            }

        }

        public static void DesToAes()
        {
            status = (UInt32)uFCoder.DES_to_AES_key_type();

            CheckStatus(status, 1, 0);
        }

        public static void GetFreeMemory()
        {
            UInt32 free_mem;

            status = (UInt32)uFCoder.uFR_int_DesfireFreeMem(out free_mem, out card_status, out exec_time);

            if (CheckStatus(status, card_status, exec_time))
            {
                Console.WriteLine(" Free memory : " + free_mem.ToString() + " bytes");
            }

        }

        public static void SetRandomID()
        {
            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_DesfireSetConfiguration(aes_key_nr, 1, 0, out card_status, out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireSetConfiguration_PK(aes_key_ext, 1, 0, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void AesToDes()
        {
            status = (UInt32)uFCoder.AES_to_DES_key_type();

            CheckStatus(status, 1, 0);
        }

        public static void menu(char key)
        {
            GetConfigParameters();

            switch (key)
            {
                case '0':
                    ChangeAuthMode();
                    break;
                case '1':
                    MasterKeyAuthRequired();
                    break;
                case '2':
                    GetCardUid();
                    break;
                case '3':
                    FormatCard();
                    break;
                case '4':
                    DesToAes();
                    break;
                case '5':
                    AesToDes();
                    break;
                case '6':
                    GetFreeMemory();
                    break;
                case '7':
                    SetRandomID();
                    break;
                case '8':
                    InternalKeysLock();
                    break;
                case '9':
                    InternalKeysUnlock();
                    break;
                case 'a':
                    SetBaudRate();
                    break;
                case 'b':
                    GetBaudRate();
                    break;
                case 'c':
                    StoreAESkeyIntoReader();
                    break;
                case 'd':
                    ChangeAESKeyCard();
                    break;
                case 'e':
                    ChangeKeySettings();
                    break;
                case 'f':
                    GetKeySettings();
                    break;
                case 'g':
                    MakeApplication();
                    break;
                case 'h':
                    DeleteApplication();
                    break;
                case 'j':
                    MakeFile();
                    break;
                case 'k':
                    DeleteFile();
                    break;
                case 'l':
                    StdFileWrite();
                    break;
                case 'm':
                    StdReadFile();
                    break;
                case 'n':
                    ReadValueFile();
                    break;
                case 'o':
                    IncreaseValueFile();
                    break;
                case 'p':
                    DecreaseValueFile();
                    break;
                case 'r':
                    ChangeConfigParametrs();
                    break;
                default:
                    usage();
                    break;
            }
            Console.Write(" --------------------------------------------------\n");
        }

        public static bool CheckStatus(DL_STATUS status, UInt16 card_status, UInt16 exec_time)
        {

            if (card_status != 1)
            {
                if (status == 0 && card_status == (UInt16)DESFIRE_CARD_STATUS_CODES.CARD_OPERATION_OK)
                {
                    Console.WriteLine(" Operation completed");
                    Console.WriteLine(" Function status is UFR_OK");
                    Console.WriteLine(" Card status is CARD_OPERATION_OK");
                    Console.WriteLine(" Execution time : " + exec_time.ToString() + " ms");

                    return true;
                }
                else
                {
                    Console.WriteLine(" Error :");
                    Console.WriteLine(" Card status : " + Enum.GetName(typeof(DESFIRE_CARD_STATUS_CODES), card_status));
                    Console.WriteLine(" Function status : " + uFCoder.status2str((uFR.DL_STATUS)status));
                    Console.WriteLine(" Execution time : " + exec_time.ToString() + " ms");
                }
            }
            else
            {
                if (status == 0)
                {
                    Console.WriteLine(" Operation completed. Status is UFR_OK");

                    return true;
                }
                else
                {
                    Console.WriteLine(" Error :");
                    Console.WriteLine(" Function status : " + uFCoder.status2str((uFR.DL_STATUS)status));
                }
            }

            return false;

        }

        public static void StoreAESkeyIntoReader()
        {
            byte aes_key_no = 0;
            byte[] aes_key = new byte[16];

            Console.WriteLine(" Input aes key number (0 - 15) :");

            string aes_key_no_str = Console.ReadLine();

            aes_key_no = Byte.Parse(aes_key_no_str);

            if (aes_key_no > 15 || aes_key_no < 0)
            {
                Console.WriteLine(" Aes key number must be in range from 0 - 15");
            }
            else
            {
                Console.WriteLine(" Input aes key (16 bytes hex) :");
                string key = Console.ReadLine();

                if (key.Length > 32)
                {
                    Console.WriteLine(" AES key must be 16 bytes");
                }
                else
                {
                    aes_key = StringToByteArray(key);

                    status = (UInt32)uFCoder.uFR_int_DesfireWriteAesKey(aes_key_no, aes_key);

                    CheckStatus(status, 1, 0);
                }

            }

        }

        public static void InternalKeysLock()
        {
            Console.Write(" Input password (8 characters):\n");
            string pass = Console.ReadLine();

            char[] password = new char[8];
            password = pass.ToCharArray();

            if (pass.Length != 8)
            {
                Console.WriteLine(" Password must be 8 characters long");
            }
            else
            {
                status = (UInt32)uFCoder.ReaderKeysLock(password);
                CheckStatus(status, 1, 0);
            }
        }

        public static void InternalKeysUnlock()
        {
            Console.Write(" Input password (8 characters):\n");
            string pass = Console.ReadLine();

            char[] password = new char[8];
            password = pass.ToCharArray();

            if (pass.Length != 8)
            {
                Console.WriteLine(" Password must be 8 characters long");
            }
            else
            {
                status = (UInt32)uFCoder.ReaderKeysUnlock(password);
                CheckStatus(status, 1, 0);
            }

        }

        public static void SetBaudRate()
        {
            Console.WriteLine(" Chose number for transceive baud rate :");
            Console.WriteLine(" 0 - 106 kbps");
            Console.WriteLine(" 1 - 212 kbps");
            Console.WriteLine(" 2 - 424 kbps");
            Console.WriteLine(" 3 - 848 kbps");

            string tx_str = Console.ReadLine();
            byte tx = Byte.Parse(tx_str);

            Console.WriteLine(" Chose number for receive baud rate :");
            Console.WriteLine(" 0 - 106 kbps");
            Console.WriteLine(" 1 - 212 kbps");
            Console.WriteLine(" 2 - 424 kbps");
            Console.WriteLine(" 3 - 848 kbps");

            string rx_str = Console.ReadLine();
            byte rx = Byte.Parse(rx_str);

            status = (UInt32)uFCoder.SetSpeedPermanently(tx, rx);
            CheckStatus(status, 1, 0);
        }

        public static void GetBaudRate()
        {
            byte tx, rx;
            status = (UInt32)uFCoder.GetSpeedParameters(out tx, out rx);

            var map = new Dictionary<byte, string>();
            map.Add(0, "106 kbps");
            map.Add(1, "212 kbps");
            map.Add(2, "424 kbps");
            map.Add(3, "848 kbps");

            if (CheckStatus(status, 1, 0))
            {
                Console.WriteLine(" TX baud rate = " + map[tx]);
                Console.WriteLine(" RX baud rate = " + map[rx]);
            }
        }

        public static void ChangeAESKeyCard()
        {
            byte[] old_key = new byte[16];
            byte[] new_key = new byte[16];

            Console.WriteLine(" Input old aes key (16 bytes) :");
            string old_str = Console.ReadLine();

            if (old_str.Length != 32)
            {
                Console.WriteLine(" Old key must be 16 bytes long");
            }
            else
            {
                old_key = StringToByteArray(old_str);

                Console.WriteLine(" Input new aes key (16 bytes) :");
                string new_str = Console.ReadLine();

                new_key = StringToByteArray(new_str);

                if (new_str.Length != 32)
                {
                    Console.WriteLine(" New key must be 16 bytes long");
                }
                else
                {
                    Console.WriteLine(" Input key number to change (0 - 13) :");
                    string num = Console.ReadLine();

                    byte aid_key_no = Byte.Parse(num);

                    if (aid_key_no < 0 || aid_key_no > 13)
                    {
                        Console.WriteLine(" Key number for changing must be in range (0 - 13) :");
                    }
                    else
                    {
                        if (auth == 0)
                        {

                            status = (UInt32)uFCoder.uFR_int_DesfireChangeAesKey(aes_key_nr, aid, aid_key_nr_auth, new_key,
                                     aid_key_no, old_key, out card_status, out exec_time);

                        }
                        else
                        {
                            status = (UInt32)uFCoder.uFR_int_DesfireChangeAesKey_PK(aes_key_ext, aid, aid_key_nr_auth, new_key,
                                     aid_key_no, old_key, out card_status, out exec_time);
                        }

                        CheckStatus(status, card_status, exec_time);
                    }
                }
            }
        }

        public static void ChangeConfigParametrs()
        {
            char c;
            do
            {
                Console.WriteLine(" Current config:");
                ReadConfig();

                Console.WriteLine(" 1 - Change aes key");
                Console.WriteLine(" 2 - Change AID");
                Console.WriteLine(" 3 - Change AID key number for auth");
                Console.WriteLine(" 4 - Change File ID");
                Console.WriteLine(" 5 - Change internal key number");
                Console.WriteLine(" esc - Exit to main menu");

                string new_aes = "";
                string new_aid = "";
                string new_aid_key_nr_auth = "";
                string new_file_id = "";
                string new_aes_key_nr = "";

                c = Console.ReadKey().KeyChar;

                switch (c)
                {
                    case '1':
                        Console.WriteLine(" Input new aes key (16 bytes hex):");
                        new_aes = Console.ReadLine();
                        aes_key_ext = StringToByteArray(new_aes);
                        break;
                    case '2':
                        Console.WriteLine(" Input new AID (3 bytes hex):");
                        new_aid = Console.ReadLine();
                        aid = Convert.ToUInt32(new_aid, 16);
                        break;
                    case '3':
                        Console.WriteLine(" Input new AID key number for auth:");
                        new_aid_key_nr_auth = Console.ReadLine();
                        aid_key_nr_auth = Byte.Parse(new_aid_key_nr_auth);
                        break;
                    case '4':
                        Console.WriteLine(" Input new File ID:");
                        new_file_id = Console.ReadLine();
                        file_id = Byte.Parse(new_file_id);
                        break;
                    case '5':
                        Console.WriteLine(" Input new internal key number");
                        new_aes_key_nr = Console.ReadLine();
                        aes_key_nr = Byte.Parse(new_aes_key_nr);
                        break;
                    default:
                        break;

                }

                string[] lines = {
                               "AES key: " + BitConverter.ToString(aes_key_ext).Replace("-", ""),
                               "AID 3 bytes hex: " + aid.ToString("X6"),
                               "AID key number for auth: " + aid_key_nr_auth.ToString(),
                               "File ID: " + file_id.ToString(),
                               "Internal key number: " + aes_key_nr.ToString()
                             };

                System.IO.File.WriteAllLines(@"..\..\config.txt", lines);

            } while (c != '\x1b');

        }

        public static void GetKeySettings()
        {
            byte setting;
            byte max_key_no;
            byte set_temp = 0;

            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_DesfireGetKeySettings(aes_key_nr, aid, out setting, out max_key_no,
                         out card_status, out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireGetKeySettings_PK(aes_key_ext, aid, out setting, out max_key_no,
                         out card_status, out exec_time);
            }

            if (CheckStatus(status, card_status, exec_time))
            {
                setting &= 0x0F;

                switch (setting)
                {
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_CHANGE:
                        set_temp = 0;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_NOT_CHANGE:
                        set_temp = 1;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_CHANGE:
                        set_temp = 2;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_NOT_CHANGE:
                        set_temp = 3;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_CHANGE:
                        set_temp = 4;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE:
                        set_temp = 5;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_CHANGE:
                        set_temp = 6;
                        break;
                    case (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE:
                        set_temp = 7;
                        break;
                }

                int Sett = int.Parse((set_temp & 0x04).ToString());
                int Create = int.Parse((set_temp & 0x02).ToString());
                int Master = int.Parse((set_temp & 0x01).ToString());

                bool use_settings = false;
                bool use_create = false;
                bool use_master = false;

                if (Sett > 0)
                {
                    use_settings = true;
                }
                if (Create > 0)
                {
                    use_create = true;
                }
                if (Master > 0)
                {
                    use_master = true;
                }

                if (use_settings == true && use_create == true && use_master == true)
                {
                    Console.WriteLine(" 7 - Settings not changeable anymore, create or delete application with master key,");
                    Console.WriteLine("     master key is not changeable anymore");
                } else if (use_master == true && use_create == true && use_settings == false)
                {
                    Console.WriteLine(" 6 - Create and delete application with master key and master key is not changeable anymore");
                } else if (use_master == true && use_settings == true && use_create == false)
                {
                    Console.WriteLine(" 5 - Settings and master key not changeable anymore");
                } else if (use_settings == true && use_create == true && use_master == false)
                {
                    Console.WriteLine(" 4 - Settings not changeable anymore and create or delete application with master key");
                } else if (use_master == true && use_create == false && use_settings == false)
                {
                    Console.WriteLine(" 3 - Master key not changeable anymore");
                } else if (use_create == true && use_settings == false && use_master == false)
                {
                    Console.WriteLine(" 2 - Create or delete application with master key authentication");
                } else if (use_settings == true && use_create == false && use_master == false)
                {
                    Console.WriteLine(" 1 - Settings not changeable anymore");
                }
                else
                {
                    Console.WriteLine("0 - No settings");
                }
            }
        }

        public static void ChangeKeySettings()
        {
            byte setting = 0;
            byte set_temp = 0;

            Console.WriteLine("Choose key setting :");
            Console.WriteLine(" 0 - No settings");
            Console.WriteLine(" 1 - Settings not changeable anymore");
            Console.WriteLine(" 2 - Create or delete application with master key authentication");
            Console.WriteLine(" 3 - Master key not changeable anymore");
            Console.WriteLine(" 4 - Settings not changeable anymore and create or delete application with master key");
            Console.WriteLine(" 5 - Settings and master key not changeable anymore");
            Console.WriteLine(" 6 - Create and delete application with master key and master key is not changeable anymore");
            Console.WriteLine(" 7 - Settings not changeable anymore, create or delete application with master key,");
            Console.WriteLine("     master key is not changeable anymore");


            string choice_str = Console.ReadLine();

            int choice = int.Parse(choice_str);

            if (choice == 1)
            {
                set_temp |= 0x04;
            } else if (choice == 2)
            {
                set_temp |= 0x02;
            }
            else if (choice == 3)
            {
                set_temp |= 0x01;
            } else if (choice == 4)
            {
                set_temp |= 0x04;
                set_temp |= 0x02;
            } else if (choice == 5)
            {
                set_temp |= 0x04;
                set_temp |= 0x01;
            } else if (choice == 6)
            {
                set_temp |= 0x02;
                set_temp |= 0x01;
            } else if (choice == 7)
            {
                set_temp |= 0x04;
                set_temp |= 0x02;
                set_temp |= 0x01;
            }

            switch (set_temp)
            {
                case 0:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_CHANGE;
                    break;
                case 1:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 2:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_CHANGE;
                    break;
                case 3:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 4:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_CHANGE;
                    break;
                case 5:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 6:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_CHANGE;
                    break;
                case 7:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE;
                    break;
            }

            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_DesfireChangeKeySettings(aes_key_nr, aid, setting, out card_status,
                         out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireChangeKeySettings_PK(aes_key_ext, aid, setting, out card_status,
                         out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void MakeApplication() {

            byte setting = 0;
            byte set_temp = 0;
            string aid_str;
            string max_str;
            byte max_key_no;
            UInt32 aid_to_create;

            Console.WriteLine(" Input aid number (3 bytes hex) :");
            aid_str = Console.ReadLine();
            aid_to_create = Convert.ToUInt32(aid_str, 16);

            Console.WriteLine(" Input maximal key number :");
            max_str = Console.ReadLine();
            max_key_no = Byte.Parse(max_str);

            Console.WriteLine("Choose applications key setting :");
            Console.WriteLine(" 0 - No settings");
            Console.WriteLine(" 1 - Settings not changeable anymore");
            Console.WriteLine(" 2 - Create or delete application with master key authentication");
            Console.WriteLine(" 3 - Master key not changeable anymore");
            Console.WriteLine(" 4 - Settings not changeable anymore and create or delete application with master key");
            Console.WriteLine(" 5 - Settings and master key not changeable anymore");
            Console.WriteLine(" 6 - Create and delete application with master key and master key is not changeable anymore");
            Console.WriteLine(" 7 - Settings not changeable anymore, create or delete application with master key,");
            Console.WriteLine("     master key is not changeable anymore");

            string choice_str = Console.ReadLine();

            int choice = int.Parse(choice_str);

            if (choice == 1)
            {
                set_temp |= 0x04;
            }
            else if (choice == 2)
            {
                set_temp |= 0x02;
            }
            else if (choice == 3)
            {
                set_temp |= 0x01;
            }
            else if (choice == 4)
            {
                set_temp |= 0x04;
                set_temp |= 0x02;
            }
            else if (choice == 5)
            {
                set_temp |= 0x04;
                set_temp |= 0x01;
            }
            else if (choice == 6)
            {
                set_temp |= 0x02;
                set_temp |= 0x01;
            }
            else if (choice == 7)
            {
                set_temp |= 0x04;
                set_temp |= 0x02;
                set_temp |= 0x01;
            }

            switch (set_temp)
            {
                case 0:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_CHANGE;
                    break;
                case 1:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 2:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_CHANGE;
                    break;
                case 3:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 4:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_CHANGE;
                    break;
                case 5:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITHOUT_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE;
                    break;
                case 6:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_CHANGE;
                    break;
                case 7:
                    setting = (byte)DESFIRE_KEY_SETTINGS.DESFIRE_KEY_SET_CREATE_WITH_AUTH_SET_NOT_CHANGE_KEY_NOT_CHANGE;
                    break;
            }

            if (master_auth == 1)
            {
                if (auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireCreateAesApplication(aes_key_nr, aid_to_create,
                             setting, max_key_no, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireCreateAesApplication_PK(aes_key_ext, aid_to_create,
                             setting, max_key_no, out card_status, out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireCreateAesApplication_no_auth(aid_to_create,
                             setting, max_key_no, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void DeleteApplication()
        {
            UInt32 aid_to_delete;
            string aid_to_delete_str;

            Console.WriteLine(" Input aid to delete (3 bytes hex) :");
            aid_to_delete_str = Console.ReadLine();

            aid_to_delete = Convert.ToUInt32(aid_to_delete_str, 16);

            if (auth == 0)
            {
                status = (UInt32)uFCoder.uFR_int_DesfireDeleteApplication(aes_key_nr, aid_to_delete,
                         out card_status, out exec_time);
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireDeleteApplication_PK(aes_key_ext, aid_to_delete,
                         out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void FileTypeInternal(byte file_type, byte file_id_create, byte rk_num, byte wk_num, byte rwk_num, byte ck_num,
                                    byte communication_mode)
        {
            if (file_type == 1)
            {
                Console.WriteLine(" Input size of file (bytes) :");
                string bytes_num_str = Console.ReadLine();
                byte bytes_num = Byte.Parse(bytes_num_str);

                status = (UInt32)uFCoder.uFR_int_DesfireCreateStdDataFile(aes_key_nr, aid, file_id_create,
                         bytes_num, rk_num, wk_num, rwk_num, ck_num, communication_mode, out card_status, out exec_time);
            }
            else if (file_type == 2)
            {
                Console.WriteLine(" Input lower limit :");
                string low_str = Console.ReadLine();
                Int32 lower_limit = Int32.Parse(low_str);

                Console.WriteLine(" Input upper limit :");
                string upp_str = Console.ReadLine();
                Int32 upper_limit = Int32.Parse(upp_str);

                Console.WriteLine(" Input value : ");
                string value_str = Console.ReadLine();
                Int32 value = Int32.Parse(value_str);

                Console.WriteLine(" Do you want to enable limited credit?");
                Console.WriteLine(" 1. Enabled");
                Console.WriteLine(" 2. Disabled");
                string limit_str = Console.ReadLine();
                byte limited_credit_enabled = 0;

                if (Byte.Parse(limit_str) == 1)
                {
                    limited_credit_enabled = 1;
                }
                else if (Byte.Parse(limit_str) == 2)
                {
                    limited_credit_enabled = 0;
                }

                Console.WriteLine(" Do you want to use free get value?");
                Console.WriteLine(" 1. Use free get value");
                Console.WriteLine(" 2. Don't use free get value");
                string free_get_str = Console.ReadLine();

                if (Byte.Parse(free_get_str) == 1)
                {
                    limited_credit_enabled |= 0x02;
                }

                status = (UInt32)uFCoder.uFR_int_DesfireCreateValueFile(aes_key_nr, aid, file_id_create, lower_limit, upper_limit, value,
                         limited_credit_enabled, rk_num, wk_num, rwk_num, ck_num, communication_mode,
                         out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void FileTypePK(byte file_type, byte file_id_create, byte rk_num, byte wk_num, byte rwk_num, byte ck_num,
                            byte communication_mode)
        {
            if (file_type == 1)
            {
                Console.WriteLine(" Input size of file (bytes) :");
                string bytes_num_str = Console.ReadLine();
                byte bytes_num = Byte.Parse(bytes_num_str);

                status = (UInt32)uFCoder.uFR_int_DesfireCreateStdDataFile_PK(aes_key_ext, aid, file_id_create,
                         bytes_num, rk_num, wk_num, rwk_num, ck_num, communication_mode, out card_status, out exec_time);
            }
            else if (file_type == 2)
            {
                Console.WriteLine(" Input lower limit :");
                string low_str = Console.ReadLine();
                Int32 lower_limit = Int32.Parse(low_str);

                Console.WriteLine(" Input upper limit :");
                string upp_str = Console.ReadLine();
                Int32 upper_limit = Int32.Parse(upp_str);

                Console.WriteLine(" Input value : ");
                string value_str = Console.ReadLine();
                Int32 value = Int32.Parse(value_str);

                Console.WriteLine(" Do you want to enable limited credit?");
                Console.WriteLine(" 1. Enabled");
                Console.WriteLine(" 2. Disabled");
                string limit_str = Console.ReadLine();
                byte limited_credit_enabled = 0;

                if (Byte.Parse(limit_str) == 1)
                {
                    limited_credit_enabled = 1;
                }
                else if (Byte.Parse(limit_str) == 2)
                {
                    limited_credit_enabled = 0;
                }

                Console.WriteLine(" Do you want to use free get value?");
                Console.WriteLine(" 1. Use free get value");
                Console.WriteLine(" 2. Don't use free get value");
                string free_get_str = Console.ReadLine();

                if (Byte.Parse(free_get_str) == 1)
                {
                    limited_credit_enabled |= 0x02;
                }

                status = (UInt32)uFCoder.uFR_int_DesfireCreateValueFile_PK(aes_key_ext, aid, file_id_create, lower_limit, upper_limit, value,
                         limited_credit_enabled, rk_num, wk_num, rwk_num, ck_num, communication_mode,
                         out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void FileTypeNoAuth(byte file_type, byte file_id_create, byte rk_num, byte wk_num, byte rwk_num, byte ck_num,
                    byte communication_mode)
        {
            if (file_type == 1)
            {
                Console.WriteLine(" Input size of file (bytes) :");
                string bytes_num_str = Console.ReadLine();
                byte bytes_num = Byte.Parse(bytes_num_str);

                status = (UInt32)uFCoder.uFR_int_DesfireCreateStdDataFile_no_auth(aid, file_id_create,
                         bytes_num, rk_num, wk_num, rwk_num, ck_num, communication_mode, out card_status, out exec_time);
            }
            else if (file_type == 2)
            {
                Console.WriteLine(" Input lower limit :");
                string low_str = Console.ReadLine();
                Int32 lower_limit = Int32.Parse(low_str);

                Console.WriteLine(" Input upper limit :");
                string upp_str = Console.ReadLine();
                Int32 upper_limit = Int32.Parse(upp_str);

                Console.WriteLine(" Input value : ");
                string value_str = Console.ReadLine();
                Int32 value = Int32.Parse(value_str);

                Console.WriteLine(" Do you want to enable limited credit?");
                Console.WriteLine(" 1. Enabled");
                Console.WriteLine(" 2. Disabled");
                string limit_str = Console.ReadLine();
                byte limited_credit_enabled = 0;

                if (Byte.Parse(limit_str) == 1)
                {
                    limited_credit_enabled = 1;
                }
                else if (Byte.Parse(limit_str) == 2)
                {
                    limited_credit_enabled = 0;
                }

                Console.WriteLine(" Do you want to use free get value?");
                Console.WriteLine(" 1. Use free get value");
                Console.WriteLine(" 2. Don't use free get value");
                string free_get_str = Console.ReadLine();

                if (Byte.Parse(free_get_str) == 1)
                {
                    limited_credit_enabled |= 0x02;
                }

                status = (UInt32)uFCoder.uFR_int_DesfireCreateValueFile_no_auth(aid, file_id_create, lower_limit, upper_limit, value,
                         limited_credit_enabled, rk_num, wk_num, rwk_num, ck_num, communication_mode,
                         out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void MakeFile()
        {
            byte file_id_create;
            byte communication_mode = 0;
            byte file_type;

            Console.WriteLine(" Input file ID:");
            string id_str = Console.ReadLine();
            file_id_create = Byte.Parse(id_str);

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_mode = 0;

            } else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_mode = 1;

            } else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_mode = 3;
            }

            Console.WriteLine(" Chose file type:");
            Console.WriteLine(" 1. Standard data file");
            Console.WriteLine(" 2. Value file");
            string type_str = Console.ReadLine();
            file_type = Byte.Parse(type_str);

            Console.WriteLine(" Input read key number (0 - 15) :");
            string rk_num_str = Console.ReadLine();
            byte rk_num = Byte.Parse(rk_num_str);

            Console.WriteLine(" Input write key number (0 - 15) :");
            string wk_num_str = Console.ReadLine();
            byte wk_num = Byte.Parse(wk_num_str);

            Console.WriteLine(" Input read/write key number (0 - 15) :");
            string rwk_num_str = Console.ReadLine();
            byte rwk_num = Byte.Parse(rwk_num_str);

            Console.WriteLine(" Input change key number (0 - 15) :");
            string ck_num_str = Console.ReadLine();
            byte ck_num = Byte.Parse(ck_num_str);

            if (master_auth == 1)
            {
                if (auth == 0)
                {
                    FileTypeInternal(file_type, file_id_create, rk_num, wk_num, rwk_num, ck_num, communication_mode);
                }
                else
                {
                    FileTypePK(file_type, file_id_create, rk_num, wk_num, rwk_num, ck_num, communication_mode);
                }
            }
            else
            {
                FileTypeNoAuth(file_type, file_id_create, rk_num, wk_num, rwk_num, ck_num, communication_mode);
            }

        }

        public static void DeleteFile()
        {
            Console.WriteLine(" Input file ID to delete:");
            string id_str = Console.ReadLine();
            byte file_id_to_delete = Byte.Parse(id_str);

            if(master_auth == 1)
            {
                if(auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireDeleteFile(aes_key_nr, aid, file_id_to_delete, out card_status,
                             out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireDeleteFile_PK(aes_key_ext, aid, file_id_to_delete, out card_status,
                             out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireDeleteFile_no_auth(aid, file_id_to_delete, out card_status,
                             out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void StdFileWrite()
        {
            byte[] file_data = new byte[10000];
            file_data = File.ReadAllBytes(@"..\..\write.txt");
            byte file_length = (byte)file_data.Length;
            byte communication_settings = 0;

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_settings = 0;

            }
            else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_settings = 1;

            }
            else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_settings = 3;
            }

            if (master_auth == 1)
            {
                if(auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireWriteStdDataFile(aes_key_nr, aid, aid_key_nr_auth, file_id,
                              0, file_length, communication_settings, file_data, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireWriteStdDataFile_PK(aes_key_ext, aid, aid_key_nr_auth, file_id, 0, file_length,
                             communication_settings, file_data, out card_status, out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireWriteStdDataFile_no_auth(aid, aid_key_nr_auth, file_id, 0, file_length,
                             communication_settings, file_data, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void StdReadFile()
        {

            byte communication_settings = 0;
            byte file_length = 0;

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_settings = 0;

            }
            else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_settings = 1;

            }
            else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_settings = 3;
            }

            Console.WriteLine(" Input file length to read (bytes):");
            string length_str = Console.ReadLine();
            file_length = Byte.Parse(length_str);

            byte[] data = new byte[file_length]; 

            if(master_auth == 1)
            {
                if(auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireReadStdDataFile(aes_key_nr, aid, aid_key_nr_auth, file_id,
                              0, file_length, communication_settings, data, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireReadStdDataFile_PK(aes_key_ext, aid, aid_key_nr_auth, file_id,
                              0, file_length, communication_settings, data, out card_status, out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireReadStdDataFile_no_auth(aid, aid_key_nr_auth, file_id,
                              0, file_length, communication_settings, data, out card_status, out exec_time);
            }

            if(CheckStatus(status, card_status, exec_time))
            {
                System.IO.File.WriteAllBytes(@"..\..\read.txt", data);
            }
        }

        public static void ReadValueFile()
        {
            byte communication_settings = 0;
            Int32 value = 0;

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_settings = 0;

            }
            else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_settings = 1;

            }
            else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_settings = 3;
            }

            if (master_auth == 1)
            {
                if(auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireReadValueFile(aes_key_nr, aid, aid_key_nr_auth, file_id,
                             communication_settings, out value, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireReadValueFile_PK(aes_key_ext, aid, aid_key_nr_auth, file_id,
                             communication_settings, out value, out card_status, out exec_time);
                }
            
            }
            else
            {
                    status = (UInt32)uFCoder.uFR_int_DesfireReadValueFile_no_auth(aid, aid_key_nr_auth, file_id,
                             communication_settings, out value, out card_status, out exec_time);
            }

            if(CheckStatus(status, card_status, exec_time))
            {
                Console.WriteLine(" Value : " + value.ToString());
            }
        }

        public static void IncreaseValueFile()
        {
            byte communication_settings = 0;
            Int32 value = 0;

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_settings = 0;

            }
            else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_settings = 1;

            }
            else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_settings = 3;
            }

            Console.WriteLine(" Enter value to increase : ");
            string inc_str = Console.ReadLine();
            value = Int32.Parse(inc_str);

            if(master_auth == 1)
            {
                if(auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireIncreaseValueFile(aes_key_nr, aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireIncreaseValueFile_PK(aes_key_ext, aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireIncreaseValueFile_no_auth(aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }

        public static void DecreaseValueFile()
        {
            byte communication_settings = 0;
            Int32 value = 0;

            Console.WriteLine(" Chose communication mode:");
            Console.WriteLine(" 1. PLAIN");
            Console.WriteLine(" 2. MACKED");
            Console.WriteLine(" 3. ENCHIPERED");
            string comm_mode_str = Console.ReadLine();

            if (Byte.Parse(comm_mode_str) == 1)
            {
                communication_settings = 0;
            }
            else if (Byte.Parse(comm_mode_str) == 2)
            {
                communication_settings = 1;
            }
            else if (Byte.Parse(comm_mode_str) == 3)
            {
                communication_settings = 3;
            }

            Console.WriteLine(" Enter value to decrease : ");
            string inc_str = Console.ReadLine();
            value = Int32.Parse(inc_str);

            if (master_auth == 1)
            {
                if (auth == 0)
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireDecreaseValueFile(aes_key_nr, aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
                }
                else
                {
                    status = (UInt32)uFCoder.uFR_int_DesfireDecreaseValueFile_PK(aes_key_ext, aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
                }
            }
            else
            {
                status = (UInt32)uFCoder.uFR_int_DesfireDecreaseValueFile_no_auth(aid, aid_key_nr_auth, file_id,
                             communication_settings, value, out card_status, out exec_time);
            }

            CheckStatus(status, card_status, exec_time);
        }
    }
}
