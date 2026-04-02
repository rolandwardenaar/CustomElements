/**
 * Minimal gRPC-Web transport using the Fetch API.
 *
 * Supports unary calls and server-streaming RPCs against an ASP.NET Core
 * gRPC-Web endpoint (Grpc.AspNetCore.Web middleware).
 *
 * gRPC-Web wire format (application/grpc-web+proto):
 *   Each frame = 1-byte flag | 4-byte big-endian length | payload
 *   Flag 0x00 = data frame, flag 0x80 = trailer frame.
 */

/** A single gRPC-Web length-prefixed frame. */
interface GrpcFrame {
  isTrailer: boolean;
  data: Uint8Array;
}

/**
 * Perform a unary gRPC-Web call.
 *
 * @param baseUrl  gRPC server origin, e.g. "https://localhost:5001"
 * @param service  Fully-qualified service name, e.g. "drawing.DrawingCanvas"
 * @param method   Method name, e.g. "GetAvailableScenes"
 * @param requestBytes  Protobuf-encoded request message
 * @returns Protobuf-encoded response bytes
 */
export async function grpcUnary(
  baseUrl: string,
  service: string,
  method: string,
  requestBytes: Uint8Array,
): Promise<Uint8Array> {
  const url = `${baseUrl}/${service}/${method}`;
  const body = encodeFrame(requestBytes);

  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/grpc-web+proto',
      'X-Grpc-Web': '1',
      Accept: 'application/grpc-web+proto',
    },
    body: body as BodyInit,
  });

  if (!response.ok) {
    throw new Error(`gRPC-Web request failed: HTTP ${response.status}`);
  }

  const responseBytes = new Uint8Array(await response.arrayBuffer());
  const frames = parseFrames(responseBytes);

  const dataFrame = frames.find((f) => !f.isTrailer);
  if (!dataFrame) {
    // Check trailer for gRPC error status
    const trailer = frames.find((f) => f.isTrailer);
    const status = parseTrailerStatus(trailer);
    if (status && status.code !== 0) {
      throw new Error(`gRPC error ${status.code}: ${status.message}`);
    }
    throw new Error('gRPC-Web: no data frame in response');
  }

  // Verify trailer status if present
  const trailer = frames.find((f) => f.isTrailer);
  const status = parseTrailerStatus(trailer);
  if (status && status.code !== 0) {
    throw new Error(`gRPC error ${status.code}: ${status.message}`);
  }

  return dataFrame.data;
}

/**
 * Perform a server-streaming gRPC-Web call.
 *
 * Yields protobuf-encoded response message bytes as they arrive.
 *
 * @param baseUrl  gRPC server origin
 * @param service  Fully-qualified service name
 * @param method   Method name
 * @param requestBytes  Protobuf-encoded request message
 */
export async function* grpcServerStream(
  baseUrl: string,
  service: string,
  method: string,
  requestBytes: Uint8Array,
): AsyncGenerator<Uint8Array> {
  const url = `${baseUrl}/${service}/${method}`;
  const body = encodeFrame(requestBytes);

  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/grpc-web+proto',
      'X-Grpc-Web': '1',
      Accept: 'application/grpc-web+proto',
    },
    body: body as BodyInit,
  });

  if (!response.ok) {
    throw new Error(`gRPC-Web request failed: HTTP ${response.status}`);
  }

  if (!response.body) {
    throw new Error('gRPC-Web: response body is null (streaming not supported?)');
  }

  // Read the response body as a stream and parse gRPC frames incrementally
  const reader = response.body.getReader();
  let buffer: Uint8Array = new Uint8Array(0);

  try {
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      // Append new chunk to buffer
      buffer = concatBytes(buffer, value as Uint8Array);

      // Extract complete frames from the buffer
      while (buffer.length >= 5) {
        const frameLength = readUint32BE(buffer, 1);
        const totalFrameSize = 5 + frameLength;
        if (buffer.length < totalFrameSize) break; // wait for more data

        const isTrailer = (buffer[0] & 0x80) !== 0;
        const frameData = buffer.slice(5, totalFrameSize);
        buffer = buffer.slice(totalFrameSize);

        if (isTrailer) {
          // Parse trailer for errors
          const text = new TextDecoder().decode(frameData);
          const grpcStatus = parseTrailerText(text);
          if (grpcStatus && grpcStatus.code !== 0) {
            throw new Error(`gRPC error ${grpcStatus.code}: ${grpcStatus.message}`);
          }
          // Trailer marks end of stream
          return;
        }

        yield frameData;
      }
    }
  } finally {
    reader.releaseLock();
  }
}

// ── Internal helpers ───────────────────────────────────────────

/** Wrap protobuf bytes in a gRPC-Web data frame (flag 0x00). */
function encodeFrame(payload: Uint8Array): Uint8Array {
  const frame = new Uint8Array(5 + payload.length);
  frame[0] = 0x00; // data frame
  writeUint32BE(frame, 1, payload.length);
  frame.set(payload, 5);
  return frame;
}

/** Parse all gRPC-Web frames from a complete response buffer. */
function parseFrames(data: Uint8Array): GrpcFrame[] {
  const frames: GrpcFrame[] = [];
  let offset = 0;
  while (offset + 5 <= data.length) {
    const isTrailer = (data[offset] & 0x80) !== 0;
    const length = readUint32BE(data, offset + 1);
    offset += 5;
    if (offset + length > data.length) break;
    frames.push({ isTrailer, data: data.slice(offset, offset + length) });
    offset += length;
  }
  return frames;
}

function parseTrailerStatus(trailer: GrpcFrame | undefined): { code: number; message: string } | null {
  if (!trailer) return null;
  const text = new TextDecoder().decode(trailer.data);
  return parseTrailerText(text);
}

function parseTrailerText(text: string): { code: number; message: string } | null {
  const lines = text.split('\r\n');
  let code = 0;
  let message = '';
  for (const line of lines) {
    if (line.startsWith('grpc-status:')) {
      code = parseInt(line.substring('grpc-status:'.length).trim(), 10);
    } else if (line.startsWith('grpc-message:')) {
      message = decodeURIComponent(line.substring('grpc-message:'.length).trim());
    }
  }
  return { code, message };
}

function readUint32BE(buf: Uint8Array, offset: number): number {
  return (
    ((buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3]) >>> 0
  );
}

function writeUint32BE(buf: Uint8Array, offset: number, value: number): void {
  buf[offset] = (value >>> 24) & 0xff;
  buf[offset + 1] = (value >>> 16) & 0xff;
  buf[offset + 2] = (value >>> 8) & 0xff;
  buf[offset + 3] = value & 0xff;
}

function concatBytes(a: Uint8Array, b: Uint8Array): Uint8Array {
  const result = new Uint8Array(a.length + b.length);
  result.set(a);
  result.set(b, a.length);
  return result;
}
