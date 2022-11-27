using Client.Console;
using Client.Services;
using Server.DataSource;
using Server.Services;
using Server.Services.Registration;

var dataSource = new InMemoryDataSource();
var dataProviderService = new DataProviderService(dataSource);

var regCenter = new RegistrationCenterService(dataProviderService);

var votingCenter = new VotingCenterService(dataProviderService, regCenter);

var clientVotingService = new ClientVotingService(dataProviderService, votingCenter, regCenter);

var terminal = new Terminal(clientVotingService, dataProviderService);
terminal.Loop();