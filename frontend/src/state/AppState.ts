import type { GameSnapshot, PlayerSnapshot, ScoreboardEntry, LobbyPlayer, RaceFinished, TrackData } from './types';

export type AppScreen = 'landing' | 'lobby' | 'countdown' | 'race' | 'results';

export interface AppState {
  screen: AppScreen;
  myPlayerId: string;
  myDisplayName: string;
  roomCode: string;

  // Lobby
  lobbyPlayers: LobbyPlayer[];

  // Countdown
  countdownSeconds: number;

  // Race
  track: TrackData | null;
  latestSnapshot: GameSnapshot | null;
  interpolatedPlayers: Map<string, PlayerSnapshot>; // Rendering cache only; lap/rank authority stays in backend snapshots.
  scoreboard: ScoreboardEntry[]; // Displayed standings; rebuilt from authoritative snapshots so lap/finish data stays current.
  scoreboardRevision: number; // Incremented whenever the displayed scoreboard ordering changes.

  // Results
  raceResults: RaceFinished | null;
}

export function createInitialState(): AppState {
  return {
    screen: 'landing',
    myPlayerId: '',
    myDisplayName: '',
    roomCode: '',
    lobbyPlayers: [],
    countdownSeconds: 0,
    track: null,
    latestSnapshot: null,
    interpolatedPlayers: new Map(),
    scoreboard: [],
    scoreboardRevision: 0,
    raceResults: null,
  };
}
