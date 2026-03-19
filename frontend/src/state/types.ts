// Shared types mirroring the server DTOs

export interface InputDto {
  accelerate: boolean;
  brake: boolean;
  turnLeft: boolean;
  turnRight: boolean;
}

export interface PlayerSnapshot {
  playerId: string;
  displayName: string;
  x: number;
  y: number;
  angle: number;
  speed: number;
  lap: number;
  checkpointIndex: number;
  bestLapMs: number | null;
  finished: boolean;
  rank: number;
  lapTimeMs: number;
  nextCheckpointIndex: number | null;
}

export interface GameSnapshot {
  tick: number;
  timestamp: number;
  players: PlayerSnapshot[];
}

export interface ScoreboardEntry {
  rank: number;
  playerId: string;
  displayName: string;
  lap: number;
  bestLapMs: number | null;
  finished: boolean;
}

export interface ScoreboardUpdate {
  rankings: ScoreboardEntry[];
}

export interface RaceResult {
  rank: number;
  playerId: string;
  displayName: string;
  totalTimeMs: number | null;
  bestLapMs: number | null;
}

export interface RaceFinished {
  trackName: string;
  totalLaps: number;
  results: RaceResult[];
}

export interface LobbyPlayer {
  playerId: string;
  displayName: string;
  isReady: boolean;
}

export interface TrackData {
  name: string;
  cols: number;
  rows: number;
  tileSize: number;
  tiles: TileData[];
  checkpoints: CheckpointData[];
  startPositions: StartPosition[];
}

export interface TileData {
  col: number;
  row: number;
  type: string;
  rotation: number;
}

export interface CheckpointData {
  index: number;
  x: number;
  y: number;
  width: number;
  height: number;
  isFinishLine: boolean;
}

export interface StartPosition {
  slot: number;
  x: number;
  y: number;
  angle: number;
}
