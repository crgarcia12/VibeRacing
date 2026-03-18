export interface KeyState {
  accelerate: boolean;
  brake: boolean;
  turnLeft: boolean;
  turnRight: boolean;
}

export class InputHandler {
  private keys: KeyState = { accelerate: false, brake: false, turnLeft: false, turnRight: false };
  private sendFn: (a: boolean, b: boolean, l: boolean, r: boolean) => void;
  private intervalId: number | null = null;

  constructor(sendFn: (a: boolean, b: boolean, l: boolean, r: boolean) => void) {
    this.sendFn = sendFn;
    window.addEventListener('keydown', this.onKeyDown);
    window.addEventListener('keyup', this.onKeyUp);
  }

  private onKeyDown = (e: KeyboardEvent) => {
    this.applyKey(e.code, true);
  };

  private onKeyUp = (e: KeyboardEvent) => {
    this.applyKey(e.code, false);
  };

  private applyKey(code: string, down: boolean) {
    switch (code) {
      case 'ArrowUp':    case 'KeyW': this.keys.accelerate = down; break;
      case 'ArrowDown':  case 'KeyS': this.keys.brake      = down; break;
      case 'ArrowLeft':  case 'KeyA': this.keys.turnLeft   = down; break;
      case 'ArrowRight': case 'KeyD': this.keys.turnRight  = down; break;
    }
  }

  startSending() {
    // Send input at ~60Hz
    this.intervalId = window.setInterval(() => {
      this.sendFn(this.keys.accelerate, this.keys.brake, this.keys.turnLeft, this.keys.turnRight);
    }, 16);
  }

  stopSending() {
    if (this.intervalId !== null) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  destroy() {
    this.stopSending();
    window.removeEventListener('keydown', this.onKeyDown);
    window.removeEventListener('keyup', this.onKeyUp);
  }
}
