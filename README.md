# Ethereum Wallet (C#)
Create Ethereum wallet with C#, .NET Core, and Nethereum libraries. Install several NuGet packages, write the source code for different functionalities and in the end, send and receive ether coins with your wallet.   

## NuGetPackages
* Nethereum.KeyStore
* Nethereum.HdWallet
* Nethereum.Web3
* Rijndael256.Core

## Network
Ropsten TestNet with [Infura](https://infura.io/)

## Create the Repo

1. Created the empty repo in github.
2. Clone the empty repo to my local box.
3. Run `dotnet new console`
4. Run `dotnet run` – This should run the code and print out "Hello World!"
5. Copy the Program.cs code from the exercise template directory to the Program.cs in the project.
6. Run dotnet add package for the following imports: using System; using static System.Console; using System.Collections; using System.IO; using System.Linq; using System.Threading.Tasks; using Nethereum.HdWallet; using Nethereum.Web3; using Nethereum.Web3.Accounts; using Newtonsoft.Json; using NBitcoin; using Rijndael256

## Program Options
### Choose a Wallet
* `Create` new wallet and save it to json file 
* `Load` existing wallet from file
* `Recover` existing wallet from mnemonic phrases and save it to new json file 
* `Exit` from the program 

### Interact with a Wallet
* `Receive` addresses for receiving coins 
* `Check` balances
* `Send` coins
* `Exit` from the program


## Module 
MI2: Module 5: E2
