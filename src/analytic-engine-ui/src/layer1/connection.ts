import * as signalR from '@microsoft/signalr';
import type { ItemRequest, ConnectionState } from '../layer0/types';

// Pheracies, 7/23/26
// Reads production URL from environment variable, falling back to localhost
const BACKEND_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

let connection: signalR.HubConnection | null = null;

// Starts the SignalR WebSocket connection and reports connection state changes
export async function startSignalR(
  onMessageReceived: (req: ItemRequest) => void,
  onStateChanged: (state: ConnectionState) => void
): Promise<signalR.HubConnection> {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${BACKEND_URL}/send`)
    .withAutomaticReconnect()
    .build();
    
  // Listen for connection lifecycle events to update the UI status
  connection.onclose(() => onStateChanged('Disconnected'));
  connection.onreconnecting(() => onStateChanged('Reconnecting'));
  connection.onreconnected(() => onStateChanged('Connected'));

  // Listen for broadcasted RemoteEvents from C#
  connection.on('OnRequestReceived', (req: ItemRequest) => {
    onMessageReceived(req);
  });
  
  onStateChanged('Connecting');
  await connection.start();
  onStateChanged('Connected');

  return connection;
}

// Fire a request over WebSockets (SignalR)
export async function sendViaSignalR(req: ItemRequest): Promise<void> {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
    throw new Error('SignalR is not connected');
  }
  await connection.invoke('SendRequest', req);
}

// Fire a request over REST HTTP POST
export async function sendViaRest(req: ItemRequest): Promise<any> {
  const response = await fetch(`${BACKEND_URL}/api/items`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(req),
  });

  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  return await response.json();
}