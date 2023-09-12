using Amazon.Runtime.Internal.Util;
using MongoDB.Driver.Core.Bindings;
using QDebug.Server.Connections;
using QDebug.Server.Objects;
using S7.Net;
using Spectre.Console;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Channels;
using Workstation.ServiceModel.Ua;

namespace QDebug.Server.Commands
{
    internal class OPCCommand
    {
        private Startup _application;
        public OPCCommand(Startup application, string[] args)
        {
            _application = application;
            string prefix = "OPC";
            OPCUAConnection? connection = null;
            if (args.Length <= 1)
            {
                application.Logger.Info("Missing arguments at opc <ip> <port> args");
                return;
            }
            try
            {
                connection = application.ConnectionUtils.FindOPCUAByIPPort(args[0], Int16.Parse(args[1]));
            }
            catch (Exception exception)
            {
                application.Logger.Error($"{prefix} {args[0]}:{args[1]} error while finding {prefix}: {exception.Message}");
            }
            if (connection == null || connection.Client == null || !connection.isConnected)
            {
                application.Logger.Error($"{prefix} {args[0]}:{args[1]} connection error");
                return;
            }
            if (args.Length <= 2)
            {
                application.Logger.Warning("Missing arguments in command");
                return;
            }
            try
            {
                if (args[3] is null || args[3] == "")
                {
                    _application.Logger.Warning($"Lacking inputs at position 3 after {args[2]}");
                    return;
                }
            } catch (IndexOutOfRangeException exception)
            {
                _application.Logger.Warning($"Lacking inputs at position 3 after {args[2]}");
                return;
            }

            try
            {
                switch (args[2])
                {
                    case "sub":
                    case "subscribe":
                        Subscribe(args, connection);
                        break;
                    case "subdb":
                    case "subscribedb":
                        SubscribeDB(args, connection);
                        break;
                    case "usub":
                    case "unsubscribe":
                        Unsubscribe(args, connection);
                        break;
                    case "r":
                    case "read":
                        Read(args, connection);
                        break;
                    case "rc":
                    case "reconnect":
                        Reconnect(args, connection);
                        break;
                    case "s":
                    case "status":
                        _ = StatusAsync(args, connection);
                        break;
                    case "dc":
                    case "disconnect":
                        connection.Client.CloseAsync();
                        break;
                    case "c":
                    case "connect":
                        connection.Client.OpenAsync();
                        break;
                    default:
                        throw new Exception($"Wrongful input at index 2 of subcommand {args[2]}");

                }
            }
            catch (Exception exception)
            {
                application.Logger.Error($"error on argument at position 2 ...{args[2]}...: {exception.Message}");
            }
        }

        private async Task StatusAsync(string[] args, OPCUAConnection connection)
        {
            var readRequest = new ReadRequest
            {
                // set the NodesToRead to an array of ReadValueIds.
                NodesToRead = new[] {
                    // construct a ReadValueId from a NodeId and AttributeId.
                    new ReadValueId {
                        // you can parse the nodeId from a string.
                        // e.g. NodeId.Parse("ns=2;s=Demo.Static.Scalar.Double")
                        NodeId = NodeId.Parse(VariableIds.Server_ServerStatus),
                            // variable class nodes have a Value attribute.
                            AttributeId = AttributeIds.Value
                    }
                }
            };
            var readResult = await connection.Client.ReadAsync(readRequest);
            var serverStatus = readResult.Results[0].GetValueOrDefault<ServerStatusDataType>();
            var coloredState = "";
            switch (serverStatus.State)
            {
                case ServerState.Unknown:
                    coloredState = $"[gray on black]{serverStatus.State}[/]";
                    break;
                case ServerState.Running:
                    coloredState = $"[lime on black]{serverStatus.State}[/]";
                    break;
                case ServerState.Suspended:
                    coloredState = $"[orange on black]{serverStatus.State}[/]";
                    break;
                case ServerState.Failed:
                    coloredState = $"[red on black]{serverStatus.State}[/]";
                    break;
                case ServerState.Shutdown:
                    coloredState = $"[maroon on black]{serverStatus.State}[/]";
                    break;
                default:
                    coloredState = $"[blue on black]{serverStatus.State}[/]";
                    break;
            }

            AnsiConsole.MarkupLine($"[white on black]Status for OPC at [violet on black]{connection.Url}:[/][/]");
            AnsiConsole.MarkupLine("ProductName: [yellow on black]{0}[/]", serverStatus.BuildInfo.ProductName);
            AnsiConsole.MarkupLine("SoftwareVersion: [green on black]{0}[/]", serverStatus.BuildInfo.SoftwareVersion);
            AnsiConsole.MarkupLine("ManufacturerName: [yellow on black]{0}[/]", serverStatus.BuildInfo.ManufacturerName);
            AnsiConsole.MarkupLine("State: {0}", coloredState);
            AnsiConsole.MarkupLine("CurrentTime: [cyan on black]{0}[/]", serverStatus.CurrentTime);

        }

        private async void Unsubscribe(string[] args, OPCUAConnection connection)
        {
            _application.Logger.Debug($"Attempting to Unsubscribe at {connection.Url} Id: {args[3]}");
            try
            {
                var request = new DeleteSubscriptionsRequest
                {
                    SubscriptionIds = new uint[] {
                        (uint) Int32.Parse(args[3])
                    }
                };
                await connection.Client.DeleteSubscriptionsAsync(request);
                _application.Logger.Info($"Unsubscribed from Id {args[3]} at {connection.Url}");
            }
            catch (FormatException exception)
            {
                _application.Logger.Error($"Error on unsubscribe {connection.Url} Id: {args[3]}, id format exception {exception.Message}");
            }
            catch (OverflowException exception)
            {
                _application.Logger.Error($"Error on unsubscribe {connection.Url} Id: {args[3]}, id overflow exception {exception.Message}");
            }
        }

        private async void SubscribeDB(string[] args, OPCUAConnection connection)
        {
            _application.Logger.Info($"Attempting to subscribe entire DB at {connection.Url} - {args[3]}");

            BrowseResponse? browseResponse = null;
            try // db must be browsed to find sub element nodes
            {
                BrowseRequest browseRequest = new BrowseRequest
                {
                    NodesToBrowse = new BrowseDescription[] {
                    new BrowseDescription {
                        NodeId = NodeId.Parse(args[3]),
                            BrowseDirection = BrowseDirection.Forward,
                            ReferenceTypeId = NodeId.Parse(ReferenceTypeIds.HierarchicalReferences),
                            NodeClassMask = (uint) NodeClass.Variable | (uint) NodeClass.Object | (uint) NodeClass.Method, IncludeSubtypes = true,
                            ResultMask = (uint) BrowseResultMask.All
                    }
                }};
                browseResponse = await connection.Client.BrowseAsync(browseRequest);
            } catch(ServiceResultException exception)
            {
                _application.Logger.Error($"Browse fail on {connection.Url}: {exception.Message}");
                return;
            }

            List<MonitoredItemCreateRequest> monitoredItems = new List<MonitoredItemCreateRequest>();

            uint i = 0;
            Dictionary<uint, string> indexedNodeIds = new Dictionary<uint, string>();
            foreach (var subNode in browseResponse.Results[0].References ?? new ReferenceDescription[0])
            {
                if (subNode.NodeId != null)
                {
                    monitoredItems.Add(new MonitoredItemCreateRequest
                    {
                        ItemToMonitor = new ReadValueId
                        {
                            NodeId = subNode.NodeId.NodeId,
                            AttributeId = AttributeIds.Value
                        },
                        MonitoringMode = MonitoringMode.Reporting,
                        RequestedParameters = new MonitoringParameters
                        {
                            ClientHandle = i,
                            SamplingInterval = 60,
                            QueueSize = 0,
                            DiscardOldest = true
                        }
                    });
                    indexedNodeIds.Add(i, subNode.NodeId.NodeId.ToString());
                }
                i++;
            }
            Sub(monitoredItems.ToArray(), connection, indexedNodeIds);
        }

        private async void Subscribe(string[] args, OPCUAConnection connection)
        {
            _application.Logger.Info($"Attempting to Subscribe to opcua at {connection.Url} - {args[3]}");
            Sub(new[] {
                    new MonitoredItemCreateRequest {
                        ItemToMonitor = new ReadValueId {
                                NodeId = NodeId.Parse(args[3]),
                                    AttributeId = AttributeIds.Value
                            },
                            MonitoringMode = MonitoringMode.Reporting,
                            RequestedParameters = new MonitoringParameters {
                                ClientHandle = 0,
                                    SamplingInterval = 20,
                                    QueueSize = 0,
                                    DiscardOldest = true
                            }
                    }
                }, connection, new Dictionary<uint, string>() { { 0, args[3] } });
        }

        private async void Sub(MonitoredItemCreateRequest[] monitoredItems, OPCUAConnection connection, Dictionary<uint, string> indexedNodeIDs)
        {

            CreateSubscriptionRequest subRequest = new()
            {
                RequestedPublishingInterval = 2000,
                RequestedMaxKeepAliveCount = 10,
                PublishingEnabled = true
            };

            if (monitoredItems.Length >= 100) // split up monitored items groups if more than 100 at once since >100 changes cant be listened to per group
            {

            }

            try
            {
                var subscriptionResponse = await connection.Client.CreateSubscriptionAsync(subRequest);
                var id = subscriptionResponse.SubscriptionId;

                MonitoredItemCreateRequest[] itemsToCreate = monitoredItems;

                var itemsRequest = new CreateMonitoredItemsRequest
                {
                    SubscriptionId = id,
                    ItemsToCreate = itemsToCreate,
                };

                var itemsResponse = await connection.Client.CreateMonitoredItemsAsync(itemsRequest);

                var token = connection.Client.Where(pr => pr.SubscriptionId == id).Subscribe(pr =>
                {
                    _application.Logger.Debug($"new Value for connection {connection.Url}:sub:{pr.SubscriptionId};");
                    // loop thru all the data change notifications
                    var dcns = pr.NotificationMessage.NotificationData.OfType<DataChangeNotification>();
                    foreach (var dcn in dcns)
                    {
                        foreach (var min in dcn.MonitoredItems)
                        {
                            //_application.Logger.Info($"NodeId: {indexedNodeIDs[min.ClientHandle]}, Value: {min.Value.Value}");
                            _application.DBConnections[0].IDBConnection.Cache(
                                connection.Ip+":"+connection.Port, 
                                new BasicObject(indexedNodeIDs[min.ClientHandle], min.Value.Value+""
                                ));
                            _application.SServer.Emit("vc", (indexedNodeIDs[min.ClientHandle], min.Value.Value)); // value change
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                _application.Logger.Error($"Subscribe failure at {connection.Url}: {exception.Message}");
            }
        }

        private async void Read(string[] args, OPCUAConnection connection)
        {
            try
            {
                ReadRequest readRequest = new()
                {
                    NodesToRead = new[] {
                        new ReadValueId {
                            NodeId = NodeId.Parse(args[3]),
                                AttributeId = AttributeIds.Value
                        }
                    }
                };

                var read = await connection.Client.ReadAsync(readRequest);
                _application.Logger.Info($"Readout for {args[3]}: {read.Results[0].Value}");
            }
            catch (Exception exception)
            {
                _application.Logger.Error($"Read failure at {connection.Url} - {args[3]}: {exception.Message}");
            }
        }

        private async void Reconnect(string[] args, OPCUAConnection connection)
        {
            await connection.Client.CloseAsync();
            await connection.Client.OpenAsync();
        }
    }
}