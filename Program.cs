using System;
using static System.Console;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.HdWallet;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using NBitcoin;
using Rijndael256;

namespace Wallets
{
    class EthereumWallet
    {
        const string network = "https://ropsten.infura.io/"; // TODO: Specify wich network you are going to use.
        const string workingDirectory = @"Wallets\"; // Path where you want to store the Wallets

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            //Initial params.
            string[] availableOperations =
            {
                "create", "load", "recover", "exit" // Allowed functionality
            };
            string input = string.Empty;
            bool isWalletReady = false;
            Wallet wallet = new Wallet(Wordlist.English, WordCount.Twelve);


            // TODO: Initialize the Web3 instance and create the Storage Directory

            Web3 web3 = new Web3(network);
            Directory.CreateDirectory(workingDirectory);

            while (!input.ToLower().Equals("exit"))
            {
                if (!isWalletReady) // User still doesn't have an wallet.
                {
                    do
                    {
                        input = ReceiveCommandCreateLoadOrRecover();

                    } while (!((IList)availableOperations).Contains(input));
                    switch (input)
                    {
                        // Create brand-new wallet. User will receive mnemonic phrase, public and private keys.
                        case "create":
                            wallet = CreateWalletDialog();
                            isWalletReady = true;
                            break;

                        // Load wallet from json file contains encrypted mnemonic phrase (words).
                        // This command will decrypt words and load wallet.
                        case "load":
                            wallet = LoadWalletDialog();
                            isWalletReady = true;
                            break;

                        /* Recover wallet from mnemonic phrase (words) which user must enter.
                         This is usefull if user has wallet, but has no json file for him
                         (for example if he uses this program for the first time).
                         Command will creates new Json file contains encrypted mnemonic phrase (words)
                         for this wallet.
                         After encrypt words program will load wallet.*/
                        case "recover":
                            wallet = RecoverWalletDialog();
                            isWalletReady = true;
                            break;

                        // Exit from the program.
                        case "exit":
                            return;
                    }
                }
                else // Already having loaded Wallet
                {
                    string[] walletAvailableOperations = {
                        "balance", "receive", "send", "exit" //Allowed functionality
                    };

                    string inputCommand = string.Empty;

                    while (!inputCommand.ToLower().Equals("exit"))
                    {
                        do
                        {
                            inputCommand = ReceiveCommandForEthersOperations();

                        } while (!((IList)walletAvailableOperations).Contains(inputCommand));
                        switch (inputCommand)
                        {
                            // Send transaction from address to address
                            case "send":
                                await SendTransactionDialog(wallet);
                                break;

                            // Shows the balances of addresses and total balance.
                            case "balance":
                                await GetWalletBallanceDialog(web3, wallet);
                                break;

                            // Shows the addresses in which you can receive coins.
                            case "receive":
                                Receive(wallet);
                                break;
                            case "exit":
                                return;
                        }
                    }
                }

            }
        }
        // Provided Dialogs.
        private static Wallet CreateWalletDialog()
        {
            try
            {
                string password;
                string passwordConfirmed;
                do
                {
                    Write("Enter password for encryption: ");
                    password = ReadLine();
                    Write("Confirm password: ");
                    passwordConfirmed = ReadLine();
                    if (password != passwordConfirmed)
                    {
                        WriteLine("Passwords did not match!");
                        WriteLine("Try again.");
                    }
                } while (password != passwordConfirmed);

                // Creating new Wallet with the provided password.
                Wallet wallet = CreateWallet(password, workingDirectory);
                return wallet;
            }
            catch (Exception)
            {
                WriteLine($"ERROR! Wallet in path {workingDirectory} can`t be created!");
                throw;
            }
        }
        private static Wallet LoadWalletDialog()
        {
            Write("Enter: Name of the file containing wallet: ");
            string nameOfWallet = ReadLine();
            Write("Enter: Password: ");
            string pass = ReadLine();
            try
            {
                // Loading the Wallet from an JSON file. Using the Password to decrypt it.
                Wallet wallet = LoadWalletFromJsonFile(nameOfWallet, workingDirectory, pass);
                return (wallet);

            }
            catch (Exception e)
            {
                WriteLine($"ERROR! Wallet {nameOfWallet} in path {workingDirectory} can`t be loaded!");
                throw e;
            }
        }
        private static Wallet RecoverWalletDialog()
        {
            try
            {
                Write("Enter: Mnemonic words with single space separator: ");
                string mnemonicPhrase = ReadLine();
                Write("Enter: password for encryption: ");
                string passForEncryptionInJsonFile = ReadLine();

                // Recovering the Wallet from Mnemonic Phrase
                Wallet wallet = RecoverFromMnemonicPhraseAndSaveToJson(
                    mnemonicPhrase, passForEncryptionInJsonFile, workingDirectory);
                return wallet;
            }
            catch (Exception e)
            {
                WriteLine("ERROR! Wallet can`t be recovered! Check your mnemonic phrase.");
                throw e;
            }
        }
        private static async Task GetWalletBallanceDialog(Web3 web3, Wallet wallet)
        {
            WriteLine("Balance:");
            try
            {
                // Getting the Balance and Displaying the Information.
                await Balance(web3, wallet);
            }
            catch (Exception)
            {
                WriteLine("Error occured! Check your wallet.");
            }
        }
        private static async Task SendTransactionDialog(Wallet wallet)
        {
            WriteLine("Enter: Address sending ethers.");
            string fromAddress = ReadLine();
            WriteLine("Enter: Address receiving ethers.");
            string toAddress = ReadLine();
            WriteLine("Enter: Amount of coins in ETH.");
            double amountOfCoins = 0d;
            try
            {
                amountOfCoins = double.Parse(ReadLine());
            }
            catch (Exception)
            {
                WriteLine("Unacceptable input for amount of coins.");
            }
            if (amountOfCoins > 0.0d)
            {
                WriteLine($"You will send {amountOfCoins} ETH from {fromAddress} to {toAddress}");
                WriteLine($"Are you sure? yes/no");
                string answer = ReadLine();
                if (answer.ToLower() == "yes")
                {
                    // Send the Transaction.
                    await Send(wallet, fromAddress, toAddress, amountOfCoins);
                }
            }
            else
            {
                WriteLine("Amount of coins for transaction must be positive number!");
            }
        }
        private static string ReceiveCommandCreateLoadOrRecover()
        {
            WriteLine("Choose working wallet.");
            WriteLine("Choose [create] to Create new Wallet.");
            WriteLine("Choose [load] to load existing Wallet from file.");
            WriteLine("Choose [recover] to recover Wallet with Mnemonic Phrase.");
            Write("Enter operation [\"Create\", \"Load\", \"Recover\", \"Exit\"]: ");
            string input = ReadLine().ToLower().Trim();
            return input;
        }
        private static string ReceiveCommandForEthersOperations()
        {
            Write("Enter operation [\"Balance\", \"Receive\", \"Send\", \"Exit\"]: ");
            string inputCommand = ReadLine().ToLower().Trim();
            return inputCommand;
        }

        // TODO: Implement this methods.

        public static Wallet CreateWallet(string password, string pathfile)
        {
            // TODO: Create brand-new wallet and get all the Words that were used.

            Wallet wallet = new Wallet(Wordlist.English, WordCount.Twelve);
            string words = string.Join(" ", wallet.Words);
            string fileName = string.Empty;

            try
            {
                // TODO: Save the Wallet in the desired Directory.
                fileName = SaveWalletToJsonFile(wallet, password, pathfile);
            }
            catch (Exception e)
            {
                WriteLine($"ERROR! The file can`t be saved! {e}");
                throw e;
            }

            WriteLine("New Wallet was created successfully!");
            WriteLine("Write down the following mnemonic words and keep them in the save place.");
            // TODO: Display the Words here.
            WriteLine(words);
            WriteLine("Seed: ");
            // TODO: Display the Seed here.
            WriteLine(wallet.Seed);
            WriteLine();
            // TODO: Implement and use PrintAddressesAndKeys to print all the Addresses and Keys.
            PrintAddressesAndKeys(wallet);

            return wallet;
        }
        private static void PrintAddressesAndKeys(Wallet wallet)
        {
            // TODO: Print all the Addresses and the coresponding Private Keys.
            WriteLine("Addresses: ");
            for (int i = 0; i < 20; i++)
            {
                WriteLine(wallet.GetAccount(i).Address);
            }

            WriteLine();
            WriteLine("Private Keys: ");
            for (int i = 0; i < 20; i++)
            {
                WriteLine(wallet.GetAccount(i).PrivateKey);
            }

            WriteLine();
        }
        private static string SaveWalletToJsonFile(Wallet wallet, string password, string pathfile)
        {
            //TODO: Encrypt and Save the Wallet to JSON.
            string words = string.Join(" ", wallet.Words);
            var encryptedWords = Rijndael.Encrypt(words, password, KeySize.Aes256);
            string date = DateTime.Now.ToString();
            var walletJsonData = new { encryptedWords = encryptedWords, date = date };
            string json = JsonConvert.SerializeObject(walletJsonData);
            Random random = new Random();
            var fileName =
                "EthereumWallet_"
                + DateTime.Now.Year + "-"
                + DateTime.Now.Month + "-"
                + DateTime.Now.Day + "-"
                + DateTime.Now.Hour + "-"
                + DateTime.Now.Minute + "-"
                + DateTime.Now.Second + "-"
                + random.Next(0, 1000) + ".json";
            File.WriteAllText(Path.Combine(pathfile, fileName), json);
            WriteLine($"Wallet saved in file: {fileName}");
            return fileName;
        }

        static Wallet LoadWalletFromJsonFile(string nameOfWalletFile, string path, string pass)
        {
            //TODO: Implement the logic that is needed to Load and Wallet from JSON.
            string pathToFile = Path.Combine(path, nameOfWalletFile);
            string words = string.Empty;
            WriteLine($"Read from {pathToFile}");
            try
            {
                string line = File.ReadAllText(pathToFile);
                dynamic results = JsonConvert.DeserializeObject<dynamic>(line);
                string encryptedWords = results.encryptedWords;
                words = Rijndael.Decrypt(encryptedWords, pass, KeySize.Aes256);
                string dataAndTime = results.date;
            }
            catch (Exception e)
            {
                WriteLine("ERROR!" + e);
            }

            return Recover(words);
        }
        public static Wallet Recover(string words)
        {
            // TODO: Recover a Wallet from existing mnemonic phrase (words).
            Wallet wallet = new Wallet(words, null);
            WriteLine("Wallet was successfully recovered.");
            WriteLine("Words: " + string.Join(" ", wallet.Words));
            WriteLine("Seed: " + string.Join(" ", wallet.Seed));
            WriteLine();
            PrintAddressesAndKeys(wallet);
            return wallet;
        }

        public static Wallet RecoverFromMnemonicPhraseAndSaveToJson(string words, string password, string pathfile)
        {
           // TODO: Recover from Mnemonic phrases and Save to JSON.
           Wallet wallet = Recover(words);
           string fileName = string.Empty;
           try
           {
               fileName = SaveWalletToJsonFile(wallet, password, pathfile);
           }
           catch (Exception)
           {
               WriteLine($"ERROR! The file {fileName} with recovered wallet can't be saved!");
               throw;
           }

           return wallet;
        }

        public static void Receive(Wallet wallet)
        {
            // TODO: Print all avaiable addresses in Wallet.
            if (wallet.GetAddresses().Count() > 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    WriteLine(wallet.GetAccount(i).Address);
                }
                WriteLine();
            }
            else
            {
                WriteLine("No addresses found!");
            }
        }
        static async Task Send(Wallet wallet, string fromAddress, string toAddress, double amountOfCoins)
        {
            // TODO: Generate and Send a transaction.
            Account accountFrom = wallet.GetAccount(fromAddress);
            string privateKeyFrom = accountFrom.PrivateKey;
            if (privateKeyFrom == string.Empty)
            {
                WriteLine("Address sending coins is not from current wallet!");
                throw new Exception("Address sending coins is not from current wallet!");
            }

            var web3 = new Web3(accountFrom, network);
            var wei = Web3.Convert.ToWei(amountOfCoins);
            try
            {
                var transaction = await web3.TransactionManager.SendTransactionAsync(
                    accountFrom.Address,
                    toAddress,
                    new Nethereum.Hex.HexTypes.HexBigInteger(wei)
                );
                WriteLine("Transaction has been sent successfully!");
            }
            catch (Exception e)
            {
                WriteLine($"ERROR! The transaction can't be completed! {e}");
                throw e;
            }
        }
        static async Task Balance(Web3 web3, Wallet wallet)
        {
            // TODO: Print all addresses and their balance. Print the Total Balance of the Wallet as well.
            decimal totalBalance = 0.0m;
            for (int i = 0; i < 20; i++)
            {
                var balance = await web3.Eth.GetBalance.SendRequestAsync(wallet.GetAccount(i).Address);
                var etherAmount = Web3.Convert.FromWei(balance.Value);
                totalBalance += etherAmount;
                WriteLine(wallet.GetAccount(i).Address + " " + etherAmount + " ETH");
            }

            WriteLine($"Total balance: {totalBalance} ETH \n");
        }
      }
    }
  
