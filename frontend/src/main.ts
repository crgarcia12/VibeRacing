import { createInitialState } from './state/AppState';
import { NetworkClient } from './network/NetworkClient';
import { Renderer } from './game/Renderer';
import { InputHandler } from './input/InputHandler';
import { renderLanding, renderLobby, renderCountdown, renderResults } from './ui/Screens';
import dustyFields from '../../shared/tracks/dusty-fields.json';
import type { TrackData } from './state/types';

const track = dustyFields as unknown as TrackData;

const state = createInitialState();
const appDiv = document.getElementById('app')!;

// Canvas for the race screen
const canvas = document.createElement('canvas');
canvas.width = track.cols * track.tileSize;
canvas.height = track.rows * track.tileSize;
canvas.id = 'raceCanvas';

const renderer = new Renderer(canvas);
renderer.setTrack(track);

let inputHandler: InputHandler | null = null;
let rafId: number | null = null;

function renderLoop() {
  renderer.render(state);
  rafId = requestAnimationFrame(renderLoop);
}

function stopRenderLoop() {
  if (rafId !== null) { cancelAnimationFrame(rafId); rafId = null; }
}

function update() {
  appDiv.innerHTML = '';

  switch (state.screen) {
    case 'landing':
      stopRenderLoop();
      inputHandler?.destroy(); inputHandler = null;
      appDiv.appendChild(renderLanding(state, net));
      break;

    case 'lobby':
      stopRenderLoop();
      appDiv.appendChild(renderLobby(state, net));
      break;

    case 'countdown':
      stopRenderLoop();
      appDiv.appendChild(renderCountdown(state));
      break;

    case 'race':
      appDiv.appendChild(canvas);
      if (!inputHandler) {
        inputHandler = new InputHandler((a, b, l, r) => net.sendInput(a, b, l, r));
        inputHandler.startSending();
      }
      if (rafId === null) renderLoop();
      break;

    case 'results':
      stopRenderLoop();
      inputHandler?.stopSending();
      appDiv.appendChild(renderResults(state, () => {
        state.screen = 'lobby';
        state.raceResults = null;
        state.scoreboard = [];
        state.scoreboardRevision = 0;
        state.latestSnapshot = null;
        state.interpolatedPlayers.clear();
        update();
      }));
      break;
  }
}

const net = new NetworkClient(state, update);

(async () => {
  await net.connect();
  update();
})();
