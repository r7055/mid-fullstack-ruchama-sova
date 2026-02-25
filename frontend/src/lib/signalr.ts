import {
  HubConnection,
  HubConnectionBuilder,
  HttpTransportType,
  LogLevel
} from "@microsoft/signalr";
import { TRANSACTION_HUB_ENDPOINT } from "./config";

export const RECEIVE_TRANSACTION_EVENT = "ReceiveTransaction";

export function createTransactionHubConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(TRANSACTION_HUB_ENDPOINT, {
      transport:
        HttpTransportType.WebSockets |
        HttpTransportType.ServerSentEvents |
        HttpTransportType.LongPolling
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
