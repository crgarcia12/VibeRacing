import type { AppState } from '../state/AppState';
import type { TileData, TrackData } from '../state/types';

// A distinct color per player slot
const PLAYER_COLORS = ['#e74c3c','#3498db','#2ecc71','#f39c12','#9b59b6','#1abc9c','#e67e22','#e91e63'];
const KERB_COLORS = ['#cc2222', '#eeeeee'];

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
    bgGrad.addColorStop(0, '#4a8b2d');
    bgGrad.addColorStop(0.55, '#2f6e1b');
    bgGrad.addColorStop(1, '#173a0d');
    ctx.fillStyle = bgGrad;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Subtle grass texture scan lines
    ctx.globalAlpha = 0.04;
    ctx.fillStyle = '#000';
    for (let gy = 0; gy < canvas.height; gy += 5) ctx.fillRect(0, gy, canvas.width, 2);
    ctx.globalAlpha = 1;

    const vignette = ctx.createRadialGradient(
      canvas.width / 2,
      canvas.height * 0.44,
      canvas.height * 0.16,
      canvas.width / 2,
      canvas.height / 2,
      canvas.width * 0.7,
    );
    vignette.addColorStop(0, 'rgba(255,225,170,0)');
    vignette.addColorStop(1, 'rgba(0,0,0,0.24)');
    ctx.fillStyle = vignette;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    if (this.track) {
      this.drawGroundVariation();
      this.drawTrack();
      this.drawTracksideProps();
    }

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
    const curbWidth = Math.max(6, Math.round(ts * 0.083));
    const stripeLength = Math.max(10, Math.round(ts * 0.125));
    const curveOuterRadius = ts;
    const curveInnerRadius = Math.max(curbWidth + 4, Math.round(ts * 0.12));
    const curveRoadOuterRadius = curveOuterRadius - curbWidth;
    const curveRoadInnerRadius = curveInnerRadius + curbWidth;

    track.tiles.forEach(tile => {
      const x = tile.col * ts;
      const y = tile.row * ts;

      ctx.save();
      ctx.translate(x + ts / 2, y + ts / 2);
      ctx.rotate((tile.rotation * Math.PI) / 180);

      ctx.fillStyle = 'rgba(0,0,0,0.16)';
      ctx.fillRect(-ts / 2 + 5, -ts / 2 + 7, ts, ts);

      if (tile.type === 'curve') {
        this.drawCurveTile(ts, curveInnerRadius, curveRoadInnerRadius, curveRoadOuterRadius, curveOuterRadius, stripeLength);
      } else {
        this.drawStraightTile(ts, curbWidth, stripeLength);
      }

      this.drawLaneMarking(tile, ts, curveRoadInnerRadius, curveRoadOuterRadius);

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

  private drawGroundVariation() {
    const { ctx, canvas, track } = this;
    if (!track) return;

    const { minX, maxX, minY, maxY } = this.getTrackBounds();
    const ts = track.tileSize;
    const centerX = (minX + maxX) / 2;
    const centerY = (minY + maxY) / 2;

    const dustGlow = ctx.createRadialGradient(
      centerX,
      centerY - ts * 0.7,
      ts * 0.35,
      centerX,
      centerY,
      ts * 4.8,
    );
    dustGlow.addColorStop(0, 'rgba(214,182,103,0.18)');
    dustGlow.addColorStop(0.45, 'rgba(120,98,46,0.1)');
    dustGlow.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = dustGlow;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    [
      { x: centerX, y: centerY, rx: ts * 2.45, ry: ts * 1.55, rotation: 0.16, color: 'rgba(66,110,40,0.22)' },
      { x: centerX - ts * 2.2, y: centerY - ts * 1.45, rx: ts * 1.2, ry: ts * 0.8, rotation: -0.42, color: 'rgba(52,88,31,0.19)' },
      { x: centerX + ts * 2.1, y: centerY - ts * 1.55, rx: ts * 1.15, ry: ts * 0.72, rotation: 0.34, color: 'rgba(137,112,58,0.16)' },
      { x: centerX + ts * 2.15, y: centerY + ts * 1.2, rx: ts * 1.2, ry: ts * 0.78, rotation: -0.28, color: 'rgba(54,82,31,0.17)' },
      { x: centerX - ts * 2.15, y: centerY + ts * 1.35, rx: ts * 1.28, ry: ts * 0.82, rotation: 0.38, color: 'rgba(132,101,55,0.16)' },
      { x: minX - ts * 0.35, y: centerY, rx: ts * 0.9, ry: ts * 1.72, rotation: -0.12, color: 'rgba(40,75,26,0.18)' },
      { x: maxX + ts * 0.35, y: centerY, rx: ts * 0.9, ry: ts * 1.72, rotation: 0.12, color: 'rgba(40,75,26,0.18)' },
      { x: minX + ts * 0.35, y: minY - ts * 0.32, rx: ts * 0.9, ry: ts * 0.52, rotation: -0.22, color: 'rgba(148,123,67,0.15)' },
      { x: maxX - ts * 0.35, y: minY - ts * 0.28, rx: ts * 0.95, ry: ts * 0.56, rotation: 0.18, color: 'rgba(148,123,67,0.15)' },
      { x: minX + ts * 0.38, y: maxY + ts * 0.28, rx: ts * 0.98, ry: ts * 0.58, rotation: 0.18, color: 'rgba(143,115,63,0.15)' },
      { x: maxX - ts * 0.38, y: maxY + ts * 0.3, rx: ts * 0.92, ry: ts * 0.56, rotation: -0.18, color: 'rgba(143,115,63,0.15)' },
    ].forEach(patch => this.drawGroundPatch(patch.x, patch.y, patch.rx, patch.ry, patch.rotation, patch.color));

    ctx.save();
    ctx.strokeStyle = 'rgba(126,101,58,0.18)';
    ctx.lineWidth = ts * 0.28;
    ctx.lineCap = 'round';
    ctx.beginPath();
    ctx.moveTo(minX + ts * 1.3, centerY + ts * 1.08);
    ctx.bezierCurveTo(
      centerX - ts * 1.35,
      centerY + ts * 0.7,
      centerX + ts * 0.9,
      centerY - ts * 1.12,
      maxX - ts * 1.12,
      centerY - ts * 0.96,
    );
    ctx.stroke();
    ctx.strokeStyle = 'rgba(255,231,180,0.06)';
    ctx.lineWidth = ts * 0.1;
    ctx.beginPath();
    ctx.moveTo(minX + ts * 1.26, centerY + ts * 1.12);
    ctx.bezierCurveTo(
      centerX - ts * 1.28,
      centerY + ts * 0.78,
      centerX + ts * 0.84,
      centerY - ts * 1.02,
      maxX - ts * 1.04,
      centerY - ts * 0.9,
    );
    ctx.stroke();
    ctx.restore();

    [
      { x: centerX - ts * 1.1, y: centerY - ts * 0.2, scale: 0.85, color: '#6ba74c' },
      { x: centerX + ts * 0.45, y: centerY - ts * 0.62, scale: 0.78, color: '#74b652' },
      { x: centerX + ts * 0.95, y: centerY + ts * 0.55, scale: 0.88, color: '#5f9d41' },
      { x: centerX - ts * 1.35, y: centerY + ts * 0.82, scale: 0.82, color: '#6fad46' },
      { x: minX - ts * 0.16, y: centerY - ts * 0.85, scale: 0.76, color: '#5b8f3a' },
      { x: maxX + ts * 0.12, y: centerY + ts * 0.92, scale: 0.74, color: '#5b8f3a' },
      { x: minX + ts * 0.6, y: minY - ts * 0.08, scale: 0.68, color: '#78b95a' },
      { x: maxX - ts * 0.58, y: maxY + ts * 0.12, scale: 0.72, color: '#6aa548' },
    ].forEach(tuft => this.drawGrassTuft(tuft.x, tuft.y, tuft.scale, tuft.color));
  }

  private drawTracksideProps() {
    const { track } = this;
    if (!track) return;

    const { minX, maxX, minY, maxY } = this.getTrackBounds();
    const ts = track.tileSize;
    const centerX = (minX + maxX) / 2;
    const centerY = (minY + maxY) / 2;

    this.drawFence(minX + ts * 0.72, minY - ts * 0.32, maxX - ts * 0.72, minY - ts * 0.32, 28);
    this.drawFence(minX + ts * 0.82, maxY + ts * 0.32, maxX - ts * 0.82, maxY + ts * 0.32, 28);
    this.drawFence(minX - ts * 0.32, minY + ts * 1.05, minX - ts * 0.32, maxY - ts * 1.05, 26);
    this.drawFence(maxX + ts * 0.32, minY + ts * 1.05, maxX + ts * 0.32, maxY - ts * 1.05, 26);

    [
      { x: minX - ts * 0.46, y: minY + ts * 1.25, scale: 1.04, foliage: '#3a7436' },
      { x: maxX + ts * 0.46, y: minY + ts * 1.12, scale: 1.08, foliage: '#356f34' },
      { x: maxX + ts * 0.38, y: maxY + ts * 0.42, scale: 1.02, foliage: '#2f612f' },
      { x: minX - ts * 0.42, y: maxY + ts * 0.48, scale: 0.98, foliage: '#376d34' },
      { x: centerX - ts * 1.65, y: centerY - ts * 0.95, scale: 0.92, foliage: '#417d3c' },
      { x: centerX + ts * 1.58, y: centerY - ts * 1.04, scale: 0.96, foliage: '#396f36' },
      { x: centerX + ts * 1.15, y: centerY + ts * 1.02, scale: 0.9, foliage: '#437e3d' },
      { x: centerX - ts * 1.72, y: centerY + ts * 1.18, scale: 0.88, foliage: '#386f35' },
    ].forEach(tree => this.drawTree(tree.x, tree.y, tree.scale, tree.foliage));

    [
      { x: minX + ts * 0.55, y: minY - ts * 0.2, scale: 0.86, color: '#538f40' },
      { x: maxX - ts * 0.55, y: minY - ts * 0.18, scale: 0.84, color: '#5b9646' },
      { x: minX + ts * 0.48, y: maxY + ts * 0.12, scale: 0.9, color: '#4f8a3d' },
      { x: maxX - ts * 0.44, y: maxY + ts * 0.18, scale: 0.88, color: '#538f40' },
      { x: centerX, y: centerY - ts * 1.55, scale: 0.8, color: '#629b4c' },
      { x: centerX + ts * 0.55, y: centerY + ts * 1.44, scale: 0.76, color: '#5b9646' },
      { x: centerX - ts * 0.82, y: centerY + ts * 1.32, scale: 0.72, color: '#5a9547' },
      { x: centerX - ts * 0.38, y: centerY - ts * 1.25, scale: 0.68, color: '#6ba74f' },
    ].forEach(shrub => this.drawShrubCluster(shrub.x, shrub.y, shrub.scale, shrub.color));

    [
      { x: centerX - ts * 0.22, y: centerY + ts * 0.18, scale: 1 },
      { x: centerX + ts * 0.62, y: centerY - ts * 0.42, scale: 0.86 },
      { x: minX - ts * 0.1, y: centerY - ts * 1.05, scale: 0.78 },
      { x: maxX + ts * 0.06, y: centerY + ts * 1.15, scale: 0.82 },
    ].forEach(rock => this.drawRockCluster(rock.x, rock.y, rock.scale));
  }

  private drawGroundPatch(x: number, y: number, rx: number, ry: number, rotation: number, color: string) {
    const { ctx } = this;
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(rotation);
    ctx.fillStyle = color;
    ctx.beginPath();
    ctx.ellipse(0, 0, rx, ry, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.restore();
  }

  private drawGrassTuft(x: number, y: number, scale: number, color: string) {
    const { ctx } = this;
    ctx.save();
    ctx.translate(x, y);
    ctx.strokeStyle = color;
    ctx.lineWidth = Math.max(1, scale * 1.25);
    ctx.lineCap = 'round';

    [
      { cpX: -4, cpY: -10, endX: -7, endY: -16 },
      { cpX: -1, cpY: -16, endX: 0, endY: -24 },
      { cpX: 3, cpY: -12, endX: 7, endY: -19 },
      { cpX: 7, cpY: -9, endX: 11, endY: -15 },
    ].forEach(blade => {
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.quadraticCurveTo(blade.cpX * scale, blade.cpY * scale, blade.endX * scale, blade.endY * scale);
      ctx.stroke();
    });
    ctx.restore();
  }

  private drawTree(x: number, y: number, scale: number, foliage: string) {
    const { ctx } = this;
    const trunkW = 8 * scale;
    const trunkH = 24 * scale;

    ctx.save();
    ctx.translate(x, y);

    ctx.fillStyle = 'rgba(0,0,0,0.18)';
    ctx.beginPath();
    ctx.ellipse(8 * scale, 20 * scale, 23 * scale, 9 * scale, -0.28, 0, Math.PI * 2);
    ctx.fill();

    ctx.fillStyle = '#6f4921';
    roundedRect(ctx, -trunkW / 2, 3 * scale, trunkW, trunkH, 2 * scale);
    ctx.fillStyle = '#8c5a29';
    ctx.fillRect(-scale, 4 * scale, 2 * scale, trunkH - 4 * scale);

    ctx.fillStyle = darkenHex(foliage, 40);
    fillCircle(ctx, -12 * scale, -2 * scale, 13 * scale);
    fillCircle(ctx, 12 * scale, -4 * scale, 14 * scale);
    fillCircle(ctx, 0, -13 * scale, 18 * scale);

    ctx.fillStyle = foliage;
    fillCircle(ctx, -14 * scale, -8 * scale, 12 * scale);
    fillCircle(ctx, 10 * scale, -11 * scale, 13 * scale);
    fillCircle(ctx, 0, -18 * scale, 15 * scale);

    ctx.fillStyle = 'rgba(255,255,255,0.1)';
    fillCircle(ctx, -5 * scale, -18 * scale, 6 * scale);
    ctx.restore();
  }

  private drawShrubCluster(x: number, y: number, scale: number, color: string) {
    const { ctx } = this;
    ctx.save();
    ctx.translate(x, y);

    ctx.fillStyle = 'rgba(0,0,0,0.12)';
    ctx.beginPath();
    ctx.ellipse(4 * scale, 8 * scale, 18 * scale, 7 * scale, -0.2, 0, Math.PI * 2);
    ctx.fill();

    ctx.fillStyle = darkenHex(color, 32);
    fillCircle(ctx, -12 * scale, 1 * scale, 8 * scale);
    fillCircle(ctx, 0, -3 * scale, 9 * scale);
    fillCircle(ctx, 11 * scale, 2 * scale, 8 * scale);

    ctx.fillStyle = color;
    fillCircle(ctx, -10 * scale, -2 * scale, 7 * scale);
    fillCircle(ctx, 1 * scale, -5 * scale, 8 * scale);
    fillCircle(ctx, 10 * scale, 0, 7 * scale);

    ctx.fillStyle = 'rgba(255,255,255,0.08)';
    fillCircle(ctx, -2 * scale, -5 * scale, 4 * scale);
    ctx.restore();
  }

  private drawRockCluster(x: number, y: number, scale: number) {
    const { ctx } = this;
    ctx.save();
    ctx.translate(x, y);

    ctx.fillStyle = 'rgba(0,0,0,0.14)';
    ctx.beginPath();
    ctx.ellipse(4 * scale, 8 * scale, 20 * scale, 8 * scale, -0.12, 0, Math.PI * 2);
    ctx.fill();

    [
      { x: -12, y: 1, rx: 9, ry: 7, color: '#8e8c79' },
      { x: 0, y: -4, rx: 11, ry: 8, color: '#9a967f' },
      { x: 13, y: 2, rx: 8, ry: 6, color: '#7f7d6b' },
    ].forEach(rock => {
      ctx.fillStyle = rock.color;
      ctx.beginPath();
      ctx.ellipse(rock.x * scale, rock.y * scale, rock.rx * scale, rock.ry * scale, -0.18, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = 'rgba(255,255,255,0.12)';
      ctx.beginPath();
      ctx.ellipse((rock.x - 2) * scale, (rock.y - 2) * scale, rock.rx * scale * 0.38, rock.ry * scale * 0.28, -0.18, 0, Math.PI * 2);
      ctx.fill();
    });

    ctx.restore();
  }

  private drawFence(startX: number, startY: number, endX: number, endY: number, postSpacing: number) {
    const { ctx } = this;
    const dx = endX - startX;
    const dy = endY - startY;
    const length = Math.hypot(dx, dy);
    if (!length) return;

    ctx.save();
    ctx.translate(startX, startY);
    ctx.rotate(Math.atan2(dy, dx));

    ctx.fillStyle = 'rgba(0,0,0,0.14)';
    roundedRect(ctx, 0, -2, length, 6, 3);

    ctx.fillStyle = '#7d5a36';
    roundedRect(ctx, 0, -11, length, 4, 2);
    roundedRect(ctx, 0, -1, length, 4, 2);

    for (let post = 0; post <= length; post += postSpacing) {
      ctx.fillStyle = '#9f7548';
      roundedRect(ctx, post - 2, -16, 4, 24, 2);
      ctx.fillStyle = '#c79a63';
      ctx.fillRect(post - 1, -16, 1, 18);
    }

    ctx.restore();
  }

  private getTrackBounds() {
    const track = this.track!;
    const cols = track.tiles.map(tile => tile.col);
    const rows = track.tiles.map(tile => tile.row);
    const ts = track.tileSize;

    return {
      minX: Math.min(...cols) * ts,
      maxX: (Math.max(...cols) + 1) * ts,
      minY: Math.min(...rows) * ts,
      maxY: (Math.max(...rows) + 1) * ts,
    };
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

function fillCircle(ctx: CanvasRenderingContext2D, x: number, y: number, radius: number) {
  ctx.beginPath();
  ctx.arc(x, y, radius, 0, Math.PI * 2);
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
