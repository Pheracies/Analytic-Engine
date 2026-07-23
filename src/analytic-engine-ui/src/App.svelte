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
 const categoryOptions = [
    { id: 0, name: 'Weapon' },
    { id: 1, name: 'Accessory' },
    { id: 2, name: 'Wealth' }
  ];
  // Pheracies, 7/23/26
  // A single array holding the checked category IDs (1 = Accessory checked by default)
  let selectedCategories = $state<number[]>([1]);
  // Helper log function
  function addLog(message: string) {
    const time = new Date().toLocaleTimeString();
    logs = [`[${time}] ${message}`, ...logs];
  }

  async function submitRequest(e: Event) {
    e.preventDefault();

    const payload: ItemRequest = {
      type: itemType,
      amount: itemAmount,
      categories: selectedCategories, // Already a clean number[] array!
    };
    
   

    addLog(`Sending request via ${protocol}: ${payload.type} (Qty: ${payload.amount})`);

    try {
      await sendViaSignalR(payload);
    } catch (err: any) {
      addLog(`Error sending: ${err.message}`);
    }
  }
  async function removeRequest(req: ItemRequest | undefined) {
    if (!req) {
      return;
    }

    if (req.timestamp) {
      try {
        await sendViaSignalR(req); // wait, we need a new method or invoke it!
        // We will call connection.invoke directly:
        // (Wait, we can import or access the connection. Let's look at connection.ts)
      } catch (err: any) {
        addLog(`Delete error: ${err.message}`);
      }
    }
  }
  // Pheracies, 7/23/26
  // Removes a request from the local UI array by its index position
  function remove(indexToRemove: number) {
    const req = requests.at(indexToRemove);
    if (!req) {
      addLog(`Attempted delete at invalid index ${indexToRemove}`);
      return;
    }

    requests = requests.filter((_, index) => index !== indexToRemove);
    removeRequest(req);
    addLog(`Deleted item globally at index ${indexToRemove}`);
  }

  // Initialize Connection on Mount
  onMount(async () => {
    addLog('Initializing SignalR connection...');
    try {
            const conn = await startSignalR(
        (newReq: ItemRequest) => {
          // Pheracies, 7/23/26
          // Check if we already have this item type on the screen
          const existingIndex = requests.findIndex(r => r.type === newReq.type);

          if (existingIndex !== -1) {
            // Update the existing card on the screen!
            requests[existingIndex] = newReq;
          } else {
            // Prepend a new card if it's a new item type
            requests = [newReq, ...requests];
          }

          addLog(`Real-time update: Received ${newReq.type} (Qty: ${newReq.amount})`);
        },
        (newState: ConnectionState) => {
          connectionState = newState;
          addLog(`Connection state changed to: ${newState}`);
        }
      );

      // Pheracies, 7/23/26
      // Listen for system welcome messages from C# and log them to the console feed
      conn.on('OnSystemMessage', (msg: string) => {
        addLog(`System: ${msg}`);
      });
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
          <!-- svelte-ignore a11y_label_has_associated_control -->
          <label>Categories:</label>
          <div class="checkbox-group">
            {#each categoryOptions as cat}
              <label>
                <input 
                  type="checkbox" 
                  value={cat.id} 
                  bind:group={selectedCategories} 
                /> 
                {cat.name}
              </label>
            {/each}
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
            {#each requests as req,index}
              <div class="feed-item">
                <span class="timestamp">{req.timestamp ? new Date(req.timestamp).toLocaleTimeString() : 'N/A'}</span>
                <span class="type">{req.type}</span>
                <div class="amount-group">
                  <span class="amount">x{req.amount}</span>
                  <button class="delete-btn" onclick={() => remove(index)} aria-label="Delete">
                    &times;
                  </button>
                </div>
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