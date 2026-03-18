import type { AppState } from '../state/AppState';
import type { TrackData } from '../state/types';

// A distinct color per player slot
const PLAYER_COLORS = ['#e74c3c','#3498db','#2ecc71','#f39c12','#9b59b6','#1abc9c','#e67e22','#e91e63'];

const CAR_LEN = 30;
const CAR_WID = 18;

export class Renderer {
  private canvas: HTMLCanvasElement;
  private ctx: CanvasRenderingContext2D;
  private track: TrackData | null = null;
  private playerColorMap = new Map<string, string>();
  private colorIndex = 0;

  constructor(canvas: HTMLCanvasElement) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d')!;
  }

  setTrack(track: TrackData) {
    this.track = track;
  }

  private getPlayerColor(playerId: string): string {
    if (!this.playerColorMap.has(playerId)) {
      this.playerColorMap.set(playerId, PLAYER_COLORS[this.colorIndex % PLAYER_COLORS.length]);
      this.colorIndex++;
    }
    return this.playerColorMap.get(playerId)!;
  }

  render(state: AppState) {
    const { ctx, canvas } = this;
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Background grass gradient
    const bgGrad = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
    bgGrad.addColorStop(0, '#2e6b1a');
    bgGrad.addColorStop(1, '#1a4a10');
    ctx.fillStyle = bgGrad;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Subtle grass texture scan lines
    ctx.globalAlpha = 0.04;
    ctx.fillStyle = '#000';
    for (let gy = 0; gy < canvas.height; gy += 5) ctx.fillRect(0, gy, canvas.width, 2);
    ctx.globalAlpha = 1;

    if (this.track) this.drawTrack();

    // Draw all interpolated players
    state.interpolatedPlayers.forEach(player => {
      this.drawCar(player.x, player.y, player.angle, this.getPlayerColor(player.playerId),
        player.displayName, player.playerId === state.myPlayerId);
    });

    // HUD
    const me = state.interpolatedPlayers.get(state.myPlayerId);
    if (me && this.track) {
      this.drawHUD(state, me);
    }

    // Scoreboard panel
    if (state.scoreboard.length > 0) {
      this.drawScoreboard(state);
    }
  }

  private drawTrack() {
    const { ctx, track } = this;
    if (!track) return;
    const ts = track.tileSize;
    const CURB_W = 8; // width of kerb strip on each edge

    track.tiles.forEach(tile => {
      const x = tile.col * ts;
      const y = tile.row * ts;

      ctx.save();
      ctx.translate(x + ts / 2, y + ts / 2);
      ctx.rotate((tile.rotation * Math.PI) / 180);

      // --- Road surface with subtle gradient ---
      const roadGrad = ctx.createLinearGradient(-ts / 2, 0, ts / 2, 0);
      roadGrad.addColorStop(0,   '#4a4a4a');
      roadGrad.addColorStop(0.5, '#525252');
      roadGrad.addColorStop(1,   '#474747');
      ctx.fillStyle = roadGrad;
      ctx.fillRect(-ts / 2, -ts / 2, ts, ts);

      // Road surface noise (subtle)
      ctx.globalAlpha = 0.025;
      ctx.fillStyle = '#000';
      for (let ny = -ts / 2; ny < ts / 2; ny += 6) ctx.fillRect(-ts / 2, ny, ts, 3);
      ctx.globalAlpha = 1;

      // --- Kerb stripes on the two long edges ---
      const kerbColors = ['#cc2222', '#eeeeee'];
      const stripeW = 12;
      const numStripes = Math.ceil(ts / stripeW);
      for (let ki = 0; ki < numStripes; ki++) {
        const kx = -ts / 2 + ki * stripeW;
        const kCol = kerbColors[ki % 2];
        ctx.fillStyle = kCol;
        ctx.fillRect(kx, -ts / 2, Math.min(stripeW, ts / 2 - kx), CURB_W);
        ctx.fillRect(kx,  ts / 2 - CURB_W, Math.min(stripeW, ts / 2 - kx), CURB_W);
      }

      // --- Center dashed line ---
      ctx.strokeStyle = 'rgba(255,255,255,0.38)';
      ctx.lineWidth = 2;
      ctx.setLineDash([20, 18]);
      if (tile.type === 'straight') {
        ctx.beginPath();
        ctx.moveTo(0, -ts / 2);
        ctx.lineTo(0, ts / 2);
        ctx.stroke();
      } else if (tile.type === 'curve') {
        // Arc center line for curve tiles — radius = ts/2 from inner corner
        const r = ts / 2;
        ctx.beginPath();
        ctx.arc(-ts / 2, -ts / 2, r + ts / 2, 0, Math.PI / 2);
        ctx.stroke();
      }
      ctx.setLineDash([]);

      ctx.restore();
    });

    // --- Finish line (checkerboard) ---
    track.checkpoints.forEach(cp => {
      if (cp.isFinishLine) {
        const sq = 8;
        const cols = Math.ceil(cp.width  / sq);
        const rows = Math.ceil(cp.height / sq);
        for (let r = 0; r < rows; r++) {
          for (let c = 0; c < cols; c++) {
            ctx.fillStyle = (r + c) % 2 === 0 ? '#ffffff' : '#111111';
            ctx.fillRect(cp.x + c * sq, cp.y + r * sq,
              Math.min(sq, cp.x + cp.width  - (cp.x + c * sq)),
              Math.min(sq, cp.y + cp.height - (cp.y + r * sq)));
          }
        }
        // Red start/finish post markers
        ctx.fillStyle = 'rgba(255,50,50,0.85)';
        ctx.fillRect(cp.x, cp.y, 4, cp.height);
        ctx.fillRect(cp.x + cp.width - 4, cp.y, 4, cp.height);
      }
    });
  }

  private drawCar(x: number, y: number, angle: number, color: string, name: string, isMe: boolean) {
    const { ctx } = this;
    const hl = CAR_LEN / 2;
    const hw = CAR_WID / 2;

    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angle);

    // Drop shadow
    ctx.fillStyle = 'rgba(0,0,0,0.30)';
    ctx.beginPath();
    ctx.ellipse(4, 5, hl * 0.9, hw * 0.9, 0, 0, Math.PI * 2);
    ctx.fill();

    // Car body (rounded rect)
    ctx.fillStyle = color;
    rRect(ctx, -hl, -hw, CAR_LEN, CAR_WID, 4);

    // Windshield
    ctx.fillStyle = 'rgba(160,220,255,0.82)';
    rRect(ctx, 2, -hw + 3, hl * 0.7, CAR_WID - 6, 2);

    // Rear spoiler + nose (darker shade of body color)
    const dark = darkenHex(color, 55);
    ctx.fillStyle = dark;
    ctx.fillRect(-hl - 4, -hw - 3, 6, CAR_WID + 6);  // spoiler
    ctx.fillRect( hl - 4, -hw + 2, 5, CAR_WID - 4);  // nose

    // Wheels (4 corners)
    ctx.fillStyle = '#1a1a1a';
    const wx = hl * 0.34, wy = hw + 2;
    ctx.fillRect(-wx - 5, -wy,     10, 5);
    ctx.fillRect(-wx - 5,  wy - 5, 10, 5);
    ctx.fillRect( wx - 4, -wy,     10, 5);
    ctx.fillRect( wx - 4,  wy - 5, 10, 5);

    // Wheel highlights
    ctx.fillStyle = '#444';
    ctx.fillRect(-wx - 3, -wy + 1,   4, 3);
    ctx.fillRect(-wx - 3,  wy - 4,   4, 3);
    ctx.fillRect( wx - 2, -wy + 1,   4, 3);
    ctx.fillRect( wx - 2,  wy - 4,   4, 3);

    // White outline for local player
    if (isMe) {
      ctx.strokeStyle = 'rgba(255,255,255,0.9)';
      ctx.lineWidth = 1.5;
      ctx.beginPath();
      if ((ctx as any).roundRect) {
        (ctx as any).roundRect(-hl, -hw, CAR_LEN, CAR_WID, 4);
      } else {
        ctx.rect(-hl, -hw, CAR_LEN, CAR_WID);
      }
      ctx.stroke();
    }

    ctx.restore();

    // Name label with subtle background
    ctx.save();
    ctx.font = 'bold 10px monospace';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'bottom';
    const labelW = ctx.measureText(name).width + 8;
    ctx.fillStyle = 'rgba(0,0,0,0.55)';
    ctx.fillRect(x - labelW / 2, y - hl - 18, labelW, 13);
    ctx.fillStyle = isMe ? '#f1c40f' : '#ffffff';
    ctx.fillText(name, x, y - hl - 6);
    ctx.restore();
  }

  private drawHUD(state: AppState, me: { lap: number; speed: number; lapTimeMs: number; rank: number }) {
    const { ctx } = this;
    const track = this.track!;
    const totalLaps = state.raceResults ? state.raceResults.totalLaps : 3;

    // Panel background
    ctx.fillStyle = 'rgba(0,0,0,0.65)';
    roundedRect(ctx, 8, 8, 164, 88, 8);

    ctx.fillStyle = '#e94560';
    ctx.font = 'bold 9px monospace';
    ctx.textAlign = 'left';
    ctx.fillText('DUST RACING 2D', 16, 22);

    ctx.fillStyle = '#fff';
    ctx.font = 'bold 13px monospace';
    const speed = Math.round((me.speed / 380) * 200);
    ctx.fillText(`LAP   ${me.lap} / ${track ? totalLaps : 3}`, 16, 38);
    ctx.fillText(`POS   P${me.rank}`, 16, 54);
    ctx.fillText(`SPD   ${speed} km/h`, 16, 70);
    ctx.fillText(`TIME  ${formatMs(me.lapTimeMs)}`, 16, 86);
  }

  private drawScoreboard(state: AppState) {
    const { ctx, canvas } = this;
    const x = canvas.width - 182;
    const lineH = 20;
    const h = 20 + state.scoreboard.length * lineH;

    ctx.fillStyle = 'rgba(0,0,0,0.70)';
    roundedRect(ctx, x - 8, 8, 192, h, 8);

    ctx.fillStyle = '#e94560';
    ctx.font = 'bold 9px monospace';
    ctx.textAlign = 'left';
    ctx.fillText('STANDINGS', x, 22);

    ctx.font = 'bold 11px monospace';
    state.scoreboard.forEach((entry, i) => {
      const isMe = entry.playerId === state.myPlayerId;
      ctx.fillStyle = isMe ? '#f1c40f' : '#ddd';
      const best = entry.bestLapMs ? formatMs(entry.bestLapMs) : '--:--.---';
      const lapStr = entry.finished ? 'FIN' : `L${entry.lap}`;
      ctx.fillText(`P${entry.rank} ${entry.displayName.padEnd(8).slice(0,8)} ${lapStr} ${best}`, x, 36 + i * lineH);
    });
  }
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function rRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number) {
  const r2 = Math.min(r, w / 2, h / 2);
  ctx.beginPath();
  ctx.moveTo(x + r2, y);
  ctx.lineTo(x + w - r2, y);
  ctx.arcTo(x + w, y,     x + w, y + r2,     r2);
  ctx.lineTo(x + w, y + h - r2);
  ctx.arcTo(x + w, y + h, x + w - r2, y + h, r2);
  ctx.lineTo(x + r2, y + h);
  ctx.arcTo(x,     y + h, x,     y + h - r2, r2);
  ctx.lineTo(x, y + r2);
  ctx.arcTo(x,     y,     x + r2, y,          r2);
  ctx.closePath();
  ctx.fill();
}

function roundedRect(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number) {
  ctx.beginPath();
  const r2 = Math.min(r, w / 2, h / 2);
  ctx.moveTo(x + r2, y);
  ctx.lineTo(x + w - r2, y);
  ctx.arcTo(x + w, y,     x + w, y + r2,     r2);
  ctx.lineTo(x + w, y + h - r2);
  ctx.arcTo(x + w, y + h, x + w - r2, y + h, r2);
  ctx.lineTo(x + r2, y + h);
  ctx.arcTo(x,     y + h, x,     y + h - r2, r2);
  ctx.lineTo(x, y + r2);
  ctx.arcTo(x,     y,     x + r2, y,          r2);
  ctx.closePath();
  ctx.fill();
}

function darkenHex(hex: string, amount: number): string {
  const r = Math.max(0, parseInt(hex.slice(1, 3), 16) - amount);
  const g = Math.max(0, parseInt(hex.slice(3, 5), 16) - amount);
  const b = Math.max(0, parseInt(hex.slice(5, 7), 16) - amount);
  return `rgb(${r},${g},${b})`;
}

function formatMs(ms: number): string {
  const minutes = Math.floor(ms / 60000);
  const seconds = Math.floor((ms % 60000) / 1000);
  const millis  = ms % 1000;
  return `${minutes}:${String(seconds).padStart(2,'0')}.${String(millis).padStart(3,'0')}`;
}
