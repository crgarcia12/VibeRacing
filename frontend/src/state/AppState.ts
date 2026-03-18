import type { GameSnapshot, PlayerSnapshot, ScoreboardEntry, LobbyPlayer, RaceFinished } from './types';

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
  latestSnapshot: GameSnapshot | null;
  interpolatedPlayers: Map<string, PlayerSnapshot>;
  scoreboard: ScoreboardEntry[];

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
    latestSnapshot: null,
    interpolatedPlayers: new Map(),
    scoreboard: [],
    raceResults: null,
  };
}
