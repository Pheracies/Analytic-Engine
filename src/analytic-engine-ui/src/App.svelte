<script lang="ts">
  import { onMount } from 'svelte';
  import { startSignalR, sendViaSignalR, sendViaRest } from './layer1/connection';
  import type { ItemRequest, ConnectionState } from './layer0/types';
  import { CATEGORY_MAP } from './layer0/types';

  // 1. Reactive states (Svelte 5 Runes)
  let connectionState = $state<ConnectionState>('Disconnected');
  let requests = $state<ItemRequest[]>([]);
  let logs = $state<string[]>([]);

  // Form Inputs
  let itemType = $state('HealthPotion');
  let itemAmount = $state(5);
  let categoryWeapon = $state(false);
  let categoryAccessory = $state(true);
  let categoryWealth = $state(false);
  let protocol = $state<'SignalR'>('SignalR');

  // Helper log function
  function addLog(message: string) {
    const time = new Date().toLocaleTimeString();
    logs = [`[${time}] ${message}`, ...logs];
  }

  // Handle request submission
  async function submitRequest(e: Event) {
    e.preventDefault();

    // Map checkboxes to C# enum values (0 = Weapon, 1 = Accessory, 2 = Wealth)
    const selectedCategories: number[] = [];
    if (categoryWeapon) selectedCategories.push(0);
    if (categoryAccessory) selectedCategories.push(1);
    if (categoryWealth) selectedCategories.push(2);

    const payload: ItemRequest = {
      type: itemType,
      amount: itemAmount,
      categories: selectedCategories,
    };

    addLog(`Sending request via ${protocol}: ${payload.type} (Qty: ${payload.amount})`);

    try {
      await sendViaSignalR(payload);
    } catch (err: any) {
      addLog(`Error sending: ${err.message}`);
    }
  }

  // Initialize Connection on Mount
  onMount(async () => {
    addLog('Initializing SignalR connection...');
    try {
      await startSignalR(
        (newReq: ItemRequest) => {
          // This callback runs when the server broadcasts an event!
          requests = [newReq, ...requests];
          addLog(`Real-time update: Received ${newReq.type}`);
        },
        (newState: ConnectionState) => {
          // This callback runs when the connection state changes!
          connectionState = newState;
          addLog(`Connection state changed to: ${newState}`);
        }
      );
    } catch (err: any) {
      addLog(`Initialization failed: ${err.message}`);
    }
  });
</script>

<main class="dashboard">
  <header>
    <h1>Analytical Engine Dashboard</h1>
    <div class="status-badge {connectionState.toLowerCase()}">
      <span class="dot"></span>
      {connectionState}
    </div>
  </header>

  <div class="grid">
    <!-- Left Column: Controls -->
    <section class="card controls">
      <h2>Send Request Event</h2>
      <form onsubmit={submitRequest}>
        <div class="field">
          <label for="itemType">Item Type (Name):</label>
          <input id="itemType" type="text" bind:value={itemType} placeholder="e.g. HealthPotion" />
        </div>

        <div class="field">
          <label for="itemAmount">Amount (1 - 3000):</label>
          <input id="itemAmount" type="number" min="1" max="3000" bind:value={itemAmount} />
        </div>

        <div class="field">
          <label>Categories:</label>
          <div class="checkbox-group">
            <label><input type="checkbox" bind:checked={categoryWeapon} /> Weapon</label>
            <label><input type="checkbox" bind:checked={categoryAccessory} /> Accessory</label>
            <label><input type="checkbox" bind:checked={categoryWealth} /> Wealth</label>
          </div>
        </div>

        

        <button type="submit" disabled={connectionState !== 'Connected' && protocol === 'SignalR'}>
          Fire Event
        </button>
      </form>
    </section>

    <!-- Right Column: Real-time Feed & Logs -->
    <div class="feed-column">
      <section class="card feed">
        <h2>Live Event Feed (SignalR Broadcasts)</h2>
        {#if requests.length === 0}
          <p class="empty-state">No real-time events received yet.</p>
        {:else}
          <div class="feed-list">
            {#each requests as req}
              <div class="feed-item">
                <span class="timestamp">{req.timestamp ? new Date(req.timestamp).toLocaleTimeString() : 'N/A'}</span>
                <span class="type">{req.type}</span>
                <span class="amount">x{req.amount}</span>
                <div class="categories">
                  {#each req.categories as cat}
                    <span class="category-tag">{CATEGORY_MAP[cat] || 'Unknown'}</span>
                  {/each}
                </div>
              </div>
            {/each}
          </div>
        {/if}
      </section>

      <section class="card logs">
        <h2>Console Logs</h2>
        <div class="log-console">
          {#each logs as log}
            <div class="log-line">{log}</div>
          {/each}
        </div>
      </section>
    </div>
  </div>
</main>