import * as signalR from '@microsoft/signalr';
import type { AppState } from '../state/AppState';
import type {
  GameSnapshot,
  ScoreboardEntry,
  ScoreboardUpdate,
  RaceFinished,
  LobbyPlayer,
  PlayerSnapshot,
  TrackData,
} from '../state/types';

const HUB_URL = import.meta.env.VITE_HUB_URL ?? getDefaultHubUrl();

export class NetworkClient {
  private connection: signalR.HubConnection;
  private state: AppState;
  private onStateChange: () => void;

  constructor(state: AppState, onStateChange: () => void) {
    this.state = state;
    this.onStateChange = onStateChange;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();

    this.registerHandlers();
  }

  private registerHandlers() {
    this.connection.on('LobbyJoined', (data: { roomCode: string; playerId: string; displayName: string }) => {
      this.state.myPlayerId = data.playerId;
      this.state.myDisplayName = data.displayName;
      this.state.roomCode = data.roomCode;
      this.onStateChange();
    });

    this.connection.on('LobbyState', (data: { players: LobbyPlayer[]; roomCode: string }) => {
      this.state.lobbyPlayers = data.players;
      this.state.screen = 'lobby';
      this.onStateChange();
    });

    this.connection.on('PlayerJoined', (data: LobbyPlayer) => {
      if (!this.state.lobbyPlayers.find(p => p.playerId === data.playerId)) {
        this.state.lobbyPlayers.push(data);
        this.onStateChange();
      }
    });

    this.connection.on('PlayerLeft', (data: { playerId: string }) => {
      this.state.lobbyPlayers = this.state.lobbyPlayers.filter(p => p.playerId !== data.playerId);
      this.setScoreboard(this.state.scoreboard.filter(p => p.playerId !== data.playerId));
      if (this.state.latestSnapshot) {
        this.state.latestSnapshot.players = this.state.latestSnapshot.players.filter(
          p => p.playerId !== data.playerId
        );
      }
      this.onStateChange();
    });

    this.connection.on('PlayerReady', (data: { playerId: string }) => {
      const p = this.state.lobbyPlayers.find(lp => lp.playerId === data.playerId);
      if (p) p.isReady = true;
      this.onStateChange();
    });

    this.connection.on('RaceCountdown', (data: { secondsRemaining: number }) => {
      this.state.screen = 'countdown';
      this.state.countdownSeconds = data.secondsRemaining;
      this.onStateChange();
    });

    this.connection.on('RaceStarted', () => {
      this.state.screen = 'race';
      this.onStateChange();
    });

    this.connection.on('TrackLoaded', (track: TrackData) => {
      this.state.track = track;
      this.onStateChange();
    });

    this.connection.on('GameSnapshot', (snapshot: GameSnapshot) => {
      this.state.latestSnapshot = snapshot;
      // Simple interpolation: use latest known positions directly
      snapshot.players.forEach(p => {
        this.state.interpolatedPlayers.set(p.playerId, p);
      });
      this.setScoreboard(this.buildScoreboardFromPlayers(snapshot.players));
      this.onStateChange();
    });

    this.connection.on('ScoreboardUpdate', (data: ScoreboardUpdate) => {
      if (!this.state.latestSnapshot) {
        this.setScoreboard(data.rankings);
        this.onStateChange();
      }
    });

    this.connection.on('RaceFinished', (data: RaceFinished) => {
      this.state.raceResults = data;
      this.state.screen = 'results';
      this.onStateChange();
    });

    this.connection.on('ErrorMessage', (data: { message: string }) => {
      alert(`Error: ${data.message}`);
    });
  }

  async connect() {
    await this.connection.start();
  }

  async joinLobby(roomCode: string, displayName: string) {
    await this.connection.invoke('JoinLobby', { roomCode, displayName });
  }

  async readyUp() {
    await this.connection.invoke('ReadyUp');
  }

  sendInput(accelerate: boolean, brake: boolean, turnLeft: boolean, turnRight: boolean) {
    this.connection.send('SendInput', { accelerate, brake, turnLeft, turnRight });
  }

  private setScoreboard(nextScoreboard: ScoreboardEntry[]) {
    if (this.hasScoreboardOrderChanged(nextScoreboard)) {
      this.state.scoreboardRevision += 1;
    }
    this.state.scoreboard = nextScoreboard;
  }

  private hasScoreboardOrderChanged(nextScoreboard: ScoreboardEntry[]) {
    if (nextScoreboard.length !== this.state.scoreboard.length) {
      return true;
    }

    return nextScoreboard.some((entry, index) => {
      const current = this.state.scoreboard[index];
      return !current || current.playerId !== entry.playerId || current.rank !== entry.rank;
    });
  }

  private buildScoreboardFromPlayers(players: PlayerSnapshot[]): ScoreboardEntry[] {
    return [...players]
      .sort((left, right) => left.rank - right.rank || left.playerId.localeCompare(right.playerId))
      .map(player => ({
        rank: player.rank,
        playerId: player.playerId,
        displayName: player.displayName,
        lap: player.lap,
        bestLapMs: player.bestLapMs,
        finished: player.finished,
      }));
  }
}

function getDefaultHubUrl() {
  if (typeof window === 'undefined') {
    return 'http://localhost:5000/racehub';
  }

  return `${window.location.origin}/racehub`;
}
