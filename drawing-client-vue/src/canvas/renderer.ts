/**
 * Translates DecodedDrawCommand objects to Canvas 2D API calls.
 *
 * Mirrors the command handling in DrawingClient.Wasm's canvas-interop.js
 * so both clients render identically.
 */
import type { DecodedDrawCommand } from '../proto/schema';

/** Execute a single draw command on the given canvas context. */
export function executeCommand(ctx: CanvasRenderingContext2D, cmd: DecodedDrawCommand): void {
  switch (cmd.command) {
    case 'clearCanvas':
      ctx.clearRect(0, 0, cmd.clearCanvas!.width, cmd.clearCanvas!.height);
      break;
    case 'setColor':
      ctx.fillStyle = cmd.setColor!.color;
      break;
    case 'setStrokeColor':
      ctx.strokeStyle = cmd.setStrokeColor!.color;
      break;
    case 'setLineWidth':
      ctx.lineWidth = cmd.setLineWidth!.width;
      break;
    case 'fillRect':
      ctx.fillRect(cmd.fillRect!.x, cmd.fillRect!.y, cmd.fillRect!.width, cmd.fillRect!.height);
      break;
    case 'beginPath':
      ctx.beginPath();
      break;
    case 'closePath':
      ctx.closePath();
      break;
    case 'moveTo':
      ctx.moveTo(cmd.moveTo!.x, cmd.moveTo!.y);
      break;
    case 'lineTo':
      ctx.lineTo(cmd.lineTo!.x, cmd.lineTo!.y);
      break;
    case 'arc':
      ctx.arc(cmd.arc!.x, cmd.arc!.y, cmd.arc!.radius, cmd.arc!.startAngle, cmd.arc!.endAngle);
      break;
    case 'fill':
      ctx.fill();
      break;
    case 'stroke':
      ctx.stroke();
      break;
    case 'quadraticCurveTo':
      ctx.quadraticCurveTo(
        cmd.quadraticCurveTo!.cpx, cmd.quadraticCurveTo!.cpy,
        cmd.quadraticCurveTo!.x, cmd.quadraticCurveTo!.y,
      );
      break;
    case 'bezierCurveTo':
      ctx.bezierCurveTo(
        cmd.bezierCurveTo!.cp1x, cmd.bezierCurveTo!.cp1y,
        cmd.bezierCurveTo!.cp2x, cmd.bezierCurveTo!.cp2y,
        cmd.bezierCurveTo!.x, cmd.bezierCurveTo!.y,
      );
      break;
  }
}

/** Execute a batch of draw commands. */
export function executeBatch(ctx: CanvasRenderingContext2D, commands: DecodedDrawCommand[]): void {
  for (const cmd of commands) {
    executeCommand(ctx, cmd);
  }
}

/** Clear the entire canvas. */
export function clearCanvas(canvas: HTMLCanvasElement): void {
  const ctx = canvas.getContext('2d');
  if (ctx) {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
  }
}
