import type { AppState } from '../state/AppState';
import type { NetworkClient } from '../network/NetworkClient';

export function renderLanding(_state: AppState, net: NetworkClient): HTMLElement {
  const el = document.createElement('div');
  el.className = 'screen landing';
  el.innerHTML = `
    <div class="card">
      <h1>≡ƒÅü VibeRacing</h1>
      <p class="subtitle">Real-time multiplayer racing</p>
      <div class="form-group">
        <label>Display Name</label>
        <input id="nameInput" type="text" maxlength="20" placeholder="Enter your name" />
      </div>
      <div class="form-group">
        <label>Room Code <span class="hint">(leave empty to create new room)</span></label>
        <input id="codeInput" type="text" maxlength="6" placeholder="e.g. ABC123" style="text-transform:uppercase" />
      </div>
      <button id="joinBtn" class="btn-primary">Join / Create Room</button>
    </div>
  `;

  el.querySelector('#joinBtn')!.addEventListener('click', async () => {
    const name = (el.querySelector('#nameInput') as HTMLInputElement).value.trim();
    const code = (el.querySelector('#codeInput') as HTMLInputElement).value.trim().toUpperCase();
    if (!name) { alert('Please enter a display name'); return; }
    await net.joinLobby(code, name);
  });

  return el;
}

export function renderLobby(state: AppState, net: NetworkClient): HTMLElement {
  const el = document.createElement('div');
  el.className = 'screen lobby';

  const meReady= state.lobbyPlayers.find(p => p.playerId === state.myPlayerId)?.isReady ?? false;

  el.innerHTML = `
    <div class="card wide">
      <h2>≡ƒÜù Lobby</h2>
      <div class="room-code">Room: <strong>${state.roomCode}</strong></div>
      <p class="hint">Share this code with friends!</p>
      <ul id="playerList" class="player-list">
        ${state.lobbyPlayers.map(p => `
          <li class="${p.playerId === state.myPlayerId ? 'me' : ''}">
            ${p.displayName}
            <span class="badge ${p.isReady ? 'ready' : 'waiting'}">${p.isReady ? 'Γ£ö Ready' : 'Waiting'}</span>
          </li>`).join('')}
      </ul>
      ${!meReady ? `<button id="readyBtn" class="btn-primary">Ready Up</button>` : '<p class="ready-msg">Γ£ö Waiting for othersΓÇª</p>'}
    </div>
  `;

  el.querySelector('#readyBtn')?.addEventListener('click', () => net.readyUp());
  return el;
}

export function renderCountdown(state: AppState): HTMLElement {
  const el = document.createElement('div');
  el.className = 'screen countdown';
  el.innerHTML = `<div class="countdown-number">${state.countdownSeconds}</div>`;
  return el;
}

export function renderResults(state: AppState, onPlayAgain: () => void): HTMLElement {
  const el = document.createElement('div');
  el.className = 'screen results';
  const results = state.raceResults!;

  el.innerHTML = `
    <div class="card wide">
      <h2>≡ƒÅå Race Results ΓÇö ${results.trackName}</h2>
      <table class="results-table">
        <thead><tr><th>#</th><th>Player</th><th>Total Time</th><th>Best Lap</th></tr></thead>
        <tbody>
          ${results.results.map(r => `
            <tr class="${r.playerId === state.myPlayerId ? 'me' : ''}">
              <td>${r.rank}</td>
              <td>${r.displayName}</td>
              <td>${r.totalTimeMs != null ? formatMs(r.totalTimeMs) : 'DNF'}</td>
              <td>${r.bestLapMs != null ? formatMs(r.bestLapMs) : '--'}</td>
            </tr>`).join('')}
        </tbody>
      </table>
      <button id="playAgainBtn" class="btn-primary">Back to Lobby</button>
    </div>
  `;

  el.querySelector('#playAgainBtn')!.addEventListener('click', onPlayAgain);
  return el;
}

function formatMs(ms: number): string {
  const minutes = Math.floor(ms / 60000);
  const seconds = Math.floor((ms % 60000) / 1000);
  const millis  = ms % 1000;
  return `${minutes}:${String(seconds).padStart(2,'0')}.${String(millis).padStart(3,'0')}`;
}
