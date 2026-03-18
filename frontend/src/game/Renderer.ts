import type { AppState } from '../state/AppState';
import type { TrackData } from '../state/types';

// A distinct color per player slot
const PLAYER_COLORS = ['#e74c3c','#3498db','#2ecc71','#f39c12','#9b59b6','#1abc9c','#e67e22','#e91e63'];

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

    // Background
    ctx.fillStyle = '#2d5a1b';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

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

    track.tiles.forEach(tile => {
      const x = tile.col * ts;
      const y = tile.row * ts;

      ctx.save();
      ctx.translate(x + ts / 2, y + ts / 2);
      ctx.rotate((tile.rotation * Math.PI) / 180);

      // Road base
      ctx.fillStyle = '#888';
      ctx.fillRect(-ts / 2, -ts / 2, ts, ts);

      // Road markings
      ctx.strokeStyle = '#fff';
      ctx.lineWidth = 2;
      ctx.setLineDash([8, 12]);
      if (tile.type === 'straight') {
        ctx.beginPath();
        ctx.moveTo(0, -ts / 2);
        ctx.lineTo(0, ts / 2);
        ctx.stroke();
      }
      ctx.setLineDash([]);
      ctx.restore();
    });

    // Draw checkpoints (debug/finish line)
    track.checkpoints.forEach(cp => {
      ctx.fillStyle = cp.isFinishLine ? 'rgba(255,255,255,0.8)' : 'rgba(255,255,0,0.3)';
      ctx.fillRect(cp.x, cp.y, cp.width, cp.height);
    });
  }

  private drawCar(x: number, y: number, angle: number, color: string, name: string, isMe: boolean) {
    const { ctx } = this;
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(angle);

    // Car body
    ctx.fillStyle = color;
    ctx.fillRect(-9, -15, 18, 30);

    // Windshield
    ctx.fillStyle = isMe ? 'rgba(255,255,255,0.9)' : 'rgba(200,220,255,0.7)';
    ctx.fillRect(-6, -12, 12, 8);

    // Outline for own car
    if (isMe) {
      ctx.strokeStyle = '#fff';
      ctx.lineWidth = 2;
      ctx.strokeRect(-9, -15, 18, 30);
    }

    ctx.restore();

    // Name label above car
    ctx.fillStyle = '#fff';
    ctx.font = 'bold 10px monospace';
    ctx.textAlign = 'center';
    ctx.fillText(name, x, y - 20);
  }

  private drawHUD(state: AppState, me: { lap: number; speed: number; lapTimeMs: number; rank: number }) {
    const { ctx } = this;
    const track = this.track!;

    ctx.fillStyle = 'rgba(0,0,0,0.55)';
    ctx.fillRect(8, 8, 160, 80);
    ctx.fillStyle = '#fff';
    ctx.font = 'bold 13px monospace';
    ctx.textAlign = 'left';

    const speed = Math.round((me.speed / 380) * 200); // normalize to ~km/h display
    ctx.fillText(`LAP   ${me.lap} / ${track ? state.raceResults ? state.raceResults.totalLaps : 3 : 3}`, 16, 28);
    ctx.fillText(`POS   P${me.rank}`, 16, 46);
    ctx.fillText(`SPD   ${speed} km/h`, 16, 64);
    ctx.fillText(`TIME  ${formatMs(me.lapTimeMs)}`, 16, 82);
  }

  private drawScoreboard(state: AppState) {
    const { ctx, canvas } = this;
    const x = canvas.width - 180;
    const lineH = 20;
    const h = 16 + state.scoreboard.length * lineH;

    ctx.fillStyle = 'rgba(0,0,0,0.65)';
    ctx.fillRect(x - 8, 8, 188, h);

    ctx.font = 'bold 11px monospace';
    ctx.textAlign = 'left';

    state.scoreboard.forEach((entry, i) => {
      const isMe = entry.playerId === state.myPlayerId;
      ctx.fillStyle = isMe ? '#f1c40f' : '#fff';
      const best = entry.bestLapMs ? formatMs(entry.bestLapMs) : '--:--.---';
      const lapStr = entry.finished ? 'FIN' : `L${entry.lap}`;
      ctx.fillText(`P${entry.rank} ${entry.displayName.padEnd(8).slice(0,8)} ${lapStr} ${best}`, x, 24 + i * lineH);
    });
  }
}

function formatMs(ms: number): string {
  const minutes = Math.floor(ms / 60000);
  const seconds = Math.floor((ms % 60000) / 1000);
  const millis  = ms % 1000;
  return `${minutes}:${String(seconds).padStart(2,'0')}.${String(millis).padStart(3,'0')}`;
}
