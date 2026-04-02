// Canvas 2D interop for DrawingClient.Wasm
// Called from Blazor C# via IJSRuntime.

/**
 * Executes a batch of draw commands on a canvas element.
 * @param {string} canvasId - The id of the canvas element.
 * @param {object[]} commands - Array of {type, ...params} objects.
 */
export function executeDrawCommands(canvasId, commands) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    for (const cmd of commands) {
        switch (cmd.type) {
            case 'clearCanvas':
                ctx.clearRect(0, 0, cmd.width, cmd.height);
                break;
            case 'setColor':
                ctx.fillStyle = cmd.color;
                break;
            case 'setStrokeColor':
                ctx.strokeStyle = cmd.color;
                break;
            case 'setLineWidth':
                ctx.lineWidth = cmd.width;
                break;
            case 'fillRect':
                ctx.fillRect(cmd.x, cmd.y, cmd.width, cmd.height);
                break;
            case 'beginPath':
                ctx.beginPath();
                break;
            case 'closePath':
                ctx.closePath();
                break;
            case 'moveTo':
                ctx.moveTo(cmd.x, cmd.y);
                break;
            case 'lineTo':
                ctx.lineTo(cmd.x, cmd.y);
                break;
            case 'arc':
                ctx.arc(cmd.x, cmd.y, cmd.radius, cmd.startAngle, cmd.endAngle);
                break;
            case 'fill':
                ctx.fill();
                break;
            case 'stroke':
                ctx.stroke();
                break;
            case 'quadraticCurveTo':
                ctx.quadraticCurveTo(cmd.cpx, cmd.cpy, cmd.x, cmd.y);
                break;
            case 'bezierCurveTo':
                ctx.bezierCurveTo(cmd.cp1x, cmd.cp1y, cmd.cp2x, cmd.cp2y, cmd.x, cmd.y);
                break;
        }
    }
}

/**
 * Clears the entire canvas.
 * @param {string} canvasId
 */
export function clearCanvas(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (ctx) ctx.clearRect(0, 0, canvas.width, canvas.height);
}
