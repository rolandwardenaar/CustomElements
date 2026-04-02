/**
 * Type-safe gRPC client for the DrawingCanvas service.
 *
 * Wraps the low-level gRPC-Web transport with protobuf
 * encoding/decoding via the schema definitions.
 */
import { grpcUnary, grpcServerStream } from './transport';
import {
  Empty,
  SceneRequest,
  SceneList,
  DrawCommand,
  type DecodedDrawCommand,
} from '../proto/schema';

const SERVICE = 'drawing.DrawingCanvas';

export class DrawingCanvasClient {
  constructor(private readonly baseUrl: string) {}

  /** Fetch the list of available scene names. */
  async getAvailableScenes(): Promise<string[]> {
    const requestBytes = Empty.encode(Empty.create()).finish();
    const responseBytes = await grpcUnary(
      this.baseUrl,
      SERVICE,
      'GetAvailableScenes',
      requestBytes,
    );
    const sceneList = SceneList.toObject(SceneList.decode(responseBytes)) as { sceneNames?: string[] };
    return sceneList.sceneNames ?? [];
  }

  /** Stream draw commands for the given scene. */
  async *streamScene(sceneName: string): AsyncGenerator<DecodedDrawCommand> {
    const request = SceneRequest.create({ sceneName });
    const requestBytes = SceneRequest.encode(request).finish();

    for await (const frameBytes of grpcServerStream(
      this.baseUrl,
      SERVICE,
      'StreamScene',
      requestBytes,
    )) {
      const msg = DrawCommand.decode(frameBytes);
      yield toDecodedCommand(msg);
    }
  }
}

/** Map a decoded protobuf DrawCommand to a typed helper object. */
function toDecodedCommand(msg: protobuf.Message): DecodedDrawCommand {
  // oneofs: true  → populates the virtual 'command' property with the active field name
  // defaults: true → preserves zero-valued fields (x=0, startAngle=0, etc.)
  const plain = DrawCommand.toObject(msg, { oneofs: true, defaults: true }) as Record<string, unknown>;

  const command = plain.command as string | undefined;
  if (!command) {
    return { command: 'unknown' } as DecodedDrawCommand;
  }

  return { command, [command]: plain[command] } as DecodedDrawCommand;
}
