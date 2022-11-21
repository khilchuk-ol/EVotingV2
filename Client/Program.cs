
using System.Security.Cryptography;
using Client.Console;
using Client.Services;
using Server.DataSource;
using Server.Encoding;
using Server.Services;

var dataSource = new InMemoryDataSource();
var dataProviderService = new DataProviderService(dataSource);

var votingCenter = new VotingCenterService(dataProviderService);

var clientVotingService = new ClientVotingService(dataProviderService, votingCenter);

var i = 1111111;

var rsa = new RSACryptoServiceProvider();
rsa.ImportParameters(votingCenter.PublicKey);

var encrypted = rsa.Encrypt(BitConverter.GetBytes(i), RSAEncryptionPadding.Pkcs1);

var decrypted = votingCenter.ApplyPrivateKey(encrypted);

var res = BitConverter.ToInt32(decrypted);

var terminal = new Terminal(clientVotingService, dataProviderService);
terminal.Loop();